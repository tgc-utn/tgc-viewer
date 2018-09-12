using BulletSharp;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using TGC.Core.BulletPhysics;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;

namespace TGC.Examples.Bullet.Physics
{
    public class HelloWorldBullet2 : PhysicsGame
    {
        private TgcPlane floorMesh;
        private TGCBox boxMesh;
        private TGCSphere sphereMesh;

        //Rigid Bodies:
        private RigidBody floorBody;

        private List<RigidBody> ballBodys = new List<RigidBody>();
        private List<RigidBody> boxBodys = new List<RigidBody>();

        public override int GetElements()
        {
            return boxBodys.Count + ballBodys.Count;
        }

        public override void Init(BulletExample3 ctx)
        {
            base.Init(ctx);

            //Creamos shapes y bodies.

            //El piso es un plano estatico se dice que si tiene masa 0 es estatico.
            var floorShape = new StaticPlaneShape(TGCVector3.Up.ToBsVector, 0);
            var floorMotionState = new DefaultMotionState();
            var floorInfo = new RigidBodyConstructionInfo(0, floorMotionState, floorShape);
            floorBody = new RigidBody(floorInfo);
            dynamicsWorld.AddRigidBody(floorBody);

            var boxBody = BulletRigidBodyConstructor.CreateBox(new TGCVector3(10, 10, 10), 1f, new TGCVector3(10f, 100f, 10f), MathUtil.SIMD_HALF_PI, MathUtil.SIMD_QUARTER_PI, MathUtil.SIMD_2_PI, 0);
            boxBodys.Add(boxBody);
            dynamicsWorld.AddRigidBody(boxBody);

            var ballBody = BulletRigidBodyConstructor.CreateBall(10f, 1f, new TGCVector3(0f, 50f, 0f));
            ballBodys.Add(ballBody);
            dynamicsWorld.AddRigidBody(ballBody);

            //Cargamos objetos de render del framework.
            var floorTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, Ctx2.MediaDir + @"Texturas\granito.jpg");
            floorMesh = new TgcPlane(new TGCVector3(-2000, 0, -2000), new TGCVector3(4000, 0f, 4000), TgcPlane.Orientations.XZplane, floorTexture);

            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, Ctx2.MediaDir + @"MeshCreator\Scenes\Deposito\Textures\boxMetal.jpg");
            //Es importante crear todos los mesh con centro en el 0,0,0 y que este coincida con el centro de masa definido caso contrario rotaria de otra forma diferente a la dada por el motor de fisica.
            boxMesh = TGCBox.fromSize(new TGCVector3(20, 20, 20), texture);

            texture = TgcTexture.createTexture(D3DDevice.Instance.Device, Ctx2.MediaDir + @"Texturas\pokeball.jpg");
            //Se crea una esfera de tamaño 1 para escalarla luego (en render)
            sphereMesh = new TGCSphere(1, texture, TGCVector3.Empty);
            //Tgc no crea el vertex buffer hasta invocar a update values.
            sphereMesh.updateValues();
        }

        public override void Update()
        {
            dynamicsWorld.StepSimulation(1 / 60f, 10);

            if (Ctx2.Input.keyUp(Key.A))
            {
                var ballBody = BulletRigidBodyConstructor.CreateBall(10f, 1f, new TGCVector3(0f, 100f, 0f));
                ballBodys.Add(ballBody);
                dynamicsWorld.AddRigidBody(ballBody);
            }

            if (Ctx2.Input.keyUp(Key.S))
            {
                var boxBody = BulletRigidBodyConstructor.CreateBox(new TGCVector3(10, 10, 10), 1f, new TGCVector3(5f, 150f, 5f), MathUtil.SIMD_HALF_PI, MathUtil.SIMD_QUARTER_PI, MathUtil.SIMD_2_PI, 0.3f);
                boxBodys.Add(boxBody);
                dynamicsWorld.AddRigidBody(boxBody);
            }

            if (Ctx2.Input.keyUp(Key.Space))
            {
                var ballBody = BulletRigidBodyConstructor.CreateBall(10, 1f, new TGCVector3(Ctx2.Camara.Position.X, Ctx2.Camara.Position.Y, Ctx2.Camara.Position.Z));
                ballBody.LinearVelocity = new TGCVector3(-Ctx2.Camara.Position.X, -Ctx2.Camara.Position.Y, -Ctx2.Camara.Position.Z).ToBsVector * 0.2f;
                ballBody.Restitution = 0.9f;
                ballBodys.Add(ballBody);
                dynamicsWorld.AddRigidBody(ballBody);
            }
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
            foreach (RigidBody boxBody in boxBodys)
            {
                //Obtenemos la matrix de directx con la transformacion que corresponde a la caja.
                boxMesh.Transform = new TGCMatrix(boxBody.InterpolationWorldTransform);
                //Dibujar las cajas en pantalla
                boxMesh.Render();
            }

            foreach (RigidBody ballBody in ballBodys)
            {
                //Obtenemos la transformacion de la pelota, en este caso se ve como se puede escalar esa transformacion.
                sphereMesh.Transform = TGCMatrix.Scaling(10, 10, 10) * new TGCMatrix(ballBody.InterpolationWorldTransform);
                sphereMesh.Render();
            }

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
            foreach (RigidBody boxBody in boxBodys)
            {
                boxBody.Dispose();
            }
            foreach (RigidBody ballBody in ballBodys)
            {
                ballBody.Dispose();
            }
            floorBody.Dispose();

            boxMesh.Dispose();
            sphereMesh.Dispose();
            floorMesh.Dispose();
        }
    }
}