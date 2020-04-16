using BulletSharp;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;

namespace TGC.Examples.BulletPhysics.Physics
{
    public class HelloWorldBullet : PhysicsGame
    {
        private TgcPlane floorMesh;
        private TGCBox boxMesh;
        private TGCSphere sphereMesh;

        //Rigid Bodies:
        private RigidBody floorBody;

        private RigidBody boxBody;
        private RigidBody ballBody;

        public override int GetElements()
        {
            return 2;
        }

        public override void Init(BulletExampleWall ctx)
        {
            base.Init(ctx);

            //Creamos shapes y bodies.

            //El piso es un plano estatico se dice que si tiene masa 0 es estatico.
            var floorShape = new StaticPlaneShape(TGCVector3.Up.ToBulletVector3(), 0);
            var floorMotionState = new DefaultMotionState();
            var floorInfo = new RigidBodyConstructionInfo(0, floorMotionState, floorShape);
            floorBody = new RigidBody(floorInfo);
            dynamicsWorld.AddRigidBody(floorBody);

            //Se crea una caja de tamaño 20 con rotaciones y origien en 10,100,10 y 1kg de masa.
            var boxShape = new BoxShape(10, 10, 10);
            var boxTransform = TGCMatrix.RotationYawPitchRoll(MathUtil.SIMD_HALF_PI, MathUtil.SIMD_QUARTER_PI, MathUtil.SIMD_2_PI).ToBulletMatrix();
            boxTransform.Origin = new TGCVector3(10, 100, 10).ToBulletVector3();
            DefaultMotionState boxMotionState = new DefaultMotionState(boxTransform);
            //Es importante calcular la inercia caso contrario el objeto no rotara.
            var boxLocalInertia = boxShape.CalculateLocalInertia(1f);
            var boxInfo = new RigidBodyConstructionInfo(1f, boxMotionState, boxShape, boxLocalInertia);
            boxBody = new RigidBody(boxInfo);
            dynamicsWorld.AddRigidBody(boxBody);

            //Crea una bola de radio 10 origen 50 de 1 kg.
            var ballShape = new SphereShape(10);
            var ballTransform = TGCMatrix.Identity;
            ballTransform.Origin = new TGCVector3(0, 50, 0);
            var ballMotionState = new DefaultMotionState(ballTransform.ToBulletMatrix());
            //Podriamos no calcular la inercia para que no rote, pero es correcto que rote tambien.
            var ballLocalInertia = ballShape.CalculateLocalInertia(1f);
            var ballInfo = new RigidBodyConstructionInfo(1, ballMotionState, ballShape, ballLocalInertia);
            ballBody = new RigidBody(ballInfo);
            dynamicsWorld.AddRigidBody(ballBody);

            //Cargamos objetos de render del framework.
            var floorTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, Ctx.MediaDir + @"Texturas\granito.jpg");
            floorMesh = new TgcPlane(new TGCVector3(-200, 0, -200), new TGCVector3(400, 0f, 400), TgcPlane.Orientations.XZplane, floorTexture);

            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, Ctx.MediaDir + @"Texturas\madera.jpg");
            //Es importante crear todos los mesh con centro en el 0,0,0 y que este coincida con el centro de masa definido caso contrario rotaria de otra forma diferente a la dada por el motor de fisica.
            boxMesh = TGCBox.fromSize(new TGCVector3(20, 20, 20), texture);
            //Se crea una esfera de tamaño 1 para escalarla luego (en render)
            sphereMesh = new TGCSphere(1, texture.Clone(), TGCVector3.Empty);
            //Tgc no crea el vertex buffer hasta invocar a update values.
            sphereMesh.updateValues();
        }

        public override void Update(float lastFrameTime, float timeBetweenFrames)
        {
            dynamicsWorld.StepSimulation(lastFrameTime, 10, timeBetweenFrames);
        }

        /// <summary>
        /// Metodo que se invoca todo el tiempo. Es el render-loop de una aplicacion grafica.
        /// En este metodo se deben dibujar todos los objetos que se desean mostrar.
        /// Antes de llamar a este metodo el framework limpia toda la pantalla.
        /// Por lo tanto para que un objeto se vea hay volver a dibujarlo siempre.
        /// La variable elapsedTime indica la cantidad de segundos que pasaron entre esta invocacion
        /// y la anterior de render(). Es util para animar e interpolar valores.
        /// </summary>
        public override void Render()
        {
            //Obtenemos la matrix de directx con la transformacion que corresponde a la caja.
            boxMesh.Transform = new TGCMatrix(boxBody.InterpolationWorldTransform);
            //Dibujar las cajas en pantalla
            boxMesh.Render();

            //Obtenemos la transformacion de la pelota, en este caso se ve como se puede escalar esa transformacion.
            sphereMesh.Transform = TGCMatrix.Scaling(10, 10, 10) * new TGCMatrix(ballBody.InterpolationWorldTransform);
            sphereMesh.Render();

            floorMesh.Render();
        }

        /// <summary>
        /// Metodo que se invoca una sola vez al finalizar el ejemplo.
        /// Se debe liberar la memoria de todos los recursos utilizados.
        /// </summary>
        public override void Dispose()
        {
            //Liberar memoria de las cajas 3D.
            //Por mas que estamos en C# con Garbage Collector igual hay que liberar la memoria de los recursos gráficos.
            //Porque están utilizando memoria de la placa de video (y ahí no hay Garbage Collector).
            dynamicsWorld.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            overlappingPairCache.Dispose();
            boxBody.Dispose();
            ballBody.Dispose();
            floorBody.Dispose();

            boxMesh.Dispose();
            sphereMesh.Dispose();
            floorMesh.Dispose();
        }
    }
}