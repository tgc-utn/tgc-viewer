using BulletSharp;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using TGC.Core.BulletPhysics;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;

namespace TGC.Examples.BulletPhysics.Physics
{
    public class WallBullet : PhysicsGame
    {
        private TgcPlane floorMesh;
        private TGCBox boxMesh1;
        private TGCBox boxMesh2;
        private TGCBox boxMesh3;
        private TGCBox boxMesh4;
        private TGCSphere sphereMesh;

        //Rigid Bodies:
        private RigidBody floorBody;

        private List<RigidBody> ballBodys = new List<RigidBody>();
        private List<RigidBody> boxBodys = new List<RigidBody>();

        public override int GetElements()
        {
            return ballBodys.Count + boxBodys.Count;
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
            floorBody.Friction = 1;
            floorBody.RollingFriction = 1;
            floorBody.Restitution = 1f;
            floorBody.UserObject = "floorBody";
            dynamicsWorld.AddRigidBody(floorBody);

            for (var i = -10; i < 10; i++)
            {
                for (var j = 0; j < 10; j++)
                {
                    var boxBody = BulletRigidBodyFactory.Instance.CreateBox(new TGCVector3(10, 10, 10), 1, new TGCVector3(i * 20f + 5f, j * 20f + 5f, 0f), 0, 0, 0, 0.5f, true);
                    boxBodys.Add(boxBody);
                    dynamicsWorld.AddRigidBody(boxBody);
                }
            }

            //Cargamos objetos de render del framework.
            var floorTexture = TGCTexture.CreateTexture(D3DDevice.Instance.Device, Ctx.MediaDir + @"Texturas\granito.jpg");
            floorMesh = new TgcPlane(new TGCVector3(-2000, 0, -2000), new TGCVector3(4000, 0f, 4000), TgcPlane.Orientations.XZplane, floorTexture);

            var texture = TGCTexture.CreateTexture(D3DDevice.Instance.Device, Ctx.MediaDir + @"\MeshCreator\Scenes\Deposito\Textures\box1.jpg");
            //Es importante crear todos los mesh con centro en el 0,0,0 y que este coincida con el centro de masa definido caso contrario rotaria de otra forma diferente a la dada por el motor de fisica.
            boxMesh1 = TGCBox.fromSize(new TGCVector3(20, 20, 20), texture);
            texture = TGCTexture.CreateTexture(D3DDevice.Instance.Device, Ctx.MediaDir + @"\MeshCreator\Scenes\Deposito\Textures\box4.jpg");
            boxMesh2 = TGCBox.fromSize(new TGCVector3(20, 20, 20), texture);
            texture = TGCTexture.CreateTexture(D3DDevice.Instance.Device, Ctx.MediaDir + @"\MeshCreator\Scenes\Deposito\Textures\box3.jpg");
            boxMesh3 = TGCBox.fromSize(new TGCVector3(20, 20, 20), texture);
            texture = TGCTexture.CreateTexture(D3DDevice.Instance.Device, Ctx.MediaDir + @"\MeshCreator\Scenes\Deposito\Textures\box4.jpg");
            boxMesh4 = TGCBox.fromSize(new TGCVector3(20, 20, 20), texture);

            texture = TGCTexture.CreateTexture(D3DDevice.Instance.Device, Ctx.MediaDir + @"\Texturas\pokeball.jpg");
            //Se crea una esfera de tamaño 1 para escalarla luego (en render)
            sphereMesh = new TGCSphere(1, texture, TGCVector3.Empty);
            //Tgc no crea el vertex buffer hasta invocar a update values.
            sphereMesh.updateValues();
        }

        public override void Update(float elapsedTime, float timeBetweenFrames)
        {
            dynamicsWorld.StepSimulation(elapsedTime, 10, timeBetweenFrames);

            if (Ctx.Input.keyUp(Key.Space))
            {
                var ballBody = BulletRigidBodyFactory.Instance.CreateBall(10f, 1f, new TGCVector3(Ctx.Camera.Position.X, Ctx.Camera.Position.Y, Ctx.Camera.Position.Z));
                var dir = new TGCVector3(Ctx.Camera.LookAt.X - Ctx.Camera.Position.X, Ctx.Camera.LookAt.Y - Ctx.Camera.Position.Y, Ctx.Camera.LookAt.Z - Ctx.Camera.Position.Z).ToBulletVector3();
                dir.Normalize();
                ballBody.LinearVelocity = dir * 900;
                ballBody.LinearFactor = TGCVector3.One.ToBulletVector3();
                //ballBody.SetDamping(0.1f, 0.5f);
                //ballBody.Restitution = 0.5f;
                ballBodys.Add(ballBody);
                dynamicsWorld.AddRigidBody(ballBody);
            }

            if (Ctx.Input.keyUp(Key.Q))
            {
                var ballBody = BulletRigidBodyFactory.Instance.CreateBall(10f, 10f, new TGCVector3(Ctx.Camera.Position.X, Ctx.Camera.Position.Y, Ctx.Camera.Position.Z));
                var dir = new TGCVector3(Ctx.Camera.LookAt.X - Ctx.Camera.Position.X, Ctx.Camera.LookAt.Y - Ctx.Camera.Position.Y, Ctx.Camera.LookAt.Z - Ctx.Camera.Position.Z).ToBulletVector3();
                dir.Normalize();
                ballBody.LinearVelocity = dir * 900;
                ballBody.LinearFactor = TGCVector3.One.ToBulletVector3();
                //ballBody.SetDamping(0.1f, 0.5f);
                //ballBody.Restitution = 0.5f;
                ballBodys.Add(ballBody);
                dynamicsWorld.AddRigidBody(ballBody);
            }

            if (Ctx.Input.keyUp(Key.W))
            {
                var ballBody = BulletRigidBodyFactory.Instance.CreateBall(10f, 0.1f, new TGCVector3(Ctx.Camera.Position.X, Ctx.Camera.Position.Y, Ctx.Camera.Position.Z));
                var dir = new TGCVector3(Ctx.Camera.LookAt.X - Ctx.Camera.Position.X, Ctx.Camera.LookAt.Y - Ctx.Camera.Position.Y, Ctx.Camera.LookAt.Z - Ctx.Camera.Position.Z).ToBulletVector3();
                dir.Normalize();
                ballBody.LinearVelocity = dir * 900;
                ballBody.LinearFactor = TGCVector3.One.ToBulletVector3();
                //ballBody.SetDamping(0.1f, 0.1f);
                //ballBody.Restitution = 0.9f;
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
        /// y la anterior de Render(). Es util para animar e interpolar valores.
        /// </summary>
        public override void Render()
        {
            var count = boxBodys.Count;
            var count4 = boxBodys.Count / 4;
            for (var i = 0; i < count; i++)
            {
                RigidBody boxBody = boxBodys[i];
                if (i % 4 == 1)
                {
                    //Obtenemos la matrix de directx con la transformacion que corresponde a la caja.
                    boxMesh1.Transform = new TGCMatrix(boxBody.InterpolationWorldTransform);
                    //Dibujar las cajas en pantalla
                    boxMesh1.Render();
                }
                if (i % 4 == 2)
                {
                    //Obtenemos la matrix de directx con la transformacion que corresponde a la caja.
                    boxMesh2.Transform = new TGCMatrix(boxBody.InterpolationWorldTransform);
                    //Dibujar las cajas en pantalla
                    boxMesh2.Render();
                }
                if (i % 4 == 3)
                {
                    //Obtenemos la matrix de directx con la transformacion que corresponde a la caja.
                    boxMesh3.Transform = new TGCMatrix(boxBody.InterpolationWorldTransform);
                    //Dibujar las cajas en pantalla
                    boxMesh3.Render();
                }
                if (i % 4 == 0)
                {
                    //Obtenemos la matrix de directx con la transformacion que corresponde a la caja.
                    boxMesh4.Transform = new TGCMatrix(boxBody.InterpolationWorldTransform);
                    //Dibujar las cajas en pantalla
                    boxMesh4.Render();
                }
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

            boxMesh1.Dispose();
            boxMesh2.Dispose();
            boxMesh3.Dispose();
            boxMesh4.Dispose();
            sphereMesh.Dispose();
            floorMesh.Dispose();
        }
    }
}