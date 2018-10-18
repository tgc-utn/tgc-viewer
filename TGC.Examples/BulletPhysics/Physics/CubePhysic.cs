using BulletSharp;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using TGC.Core.BulletPhysics;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;

namespace TGC.Examples.Tutorial.Physics
{
    public class CubePhysic
    {
        //Configuracion de la Simulacion Fisica
        private DiscreteDynamicsWorld dynamicsWorld;

        private CollisionDispatcher dispatcher;
        private DefaultCollisionConfiguration collisionConfiguration;
        private SequentialImpulseConstraintSolver constraintSolver;
        private BroadphaseInterface overlappingPairCache;

        private List<TgcMesh> meshes = new List<TgcMesh>();
        private RigidBody floorBody;

        private TgcMesh hummer;
        private RigidBody hummerBody;
        private TGCVector3 fowardback;
        private TGCVector3 leftright;

        public void setHummer(TgcMesh hummer)
        {
            this.hummer = hummer;
        }

        public TgcMesh getHummer()
        {
            return this.hummer;
        }

        public void setBuildings(List<TgcMesh> meshes)
        {
            this.meshes = meshes;
        }

        public TGCVector3 getBodyPos()
        {
            return new TGCVector3(hummerBody.CenterOfMassPosition.X, hummerBody.CenterOfMassPosition.Y, hummerBody.CenterOfMassPosition.Z);
        }

        public void Init(string MediaDir)
        {
            #region Configuracion Basica de World

            //Creamos el mundo fisico por defecto.
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            constraintSolver = new SequentialImpulseConstraintSolver();
            overlappingPairCache = new DbvtBroadphase(); //AxisSweep3(new BsVector3(-5000f, -5000f, -5000f), new BsVector3(5000f, 5000f, 5000f), 8192);
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, overlappingPairCache, constraintSolver, collisionConfiguration);
            dynamicsWorld.Gravity = new TGCVector3(0, -100f, 0).ToBulletVector3();

            #endregion Configuracion Basica de World

            foreach (var mesh in meshes)
            {
                var buildingbody = BulletRigidBodyFactory.Instance.CreateRigidBodyFromTgcMesh(mesh);
                dynamicsWorld.AddRigidBody(buildingbody);
            }

            //Se crea un plano ya que esta escena tiene problemas
            //con la definición de triangulos para el suelo
            var floorShape = new StaticPlaneShape(TGCVector3.Up.ToBulletVector3(), 10);
            floorShape.LocalScaling = new TGCVector3().ToBulletVector3();
            var floorMotionState = new DefaultMotionState();
            var floorInfo = new RigidBodyConstructionInfo(0, floorMotionState, floorShape);
            floorBody = new RigidBody(floorInfo);
            floorBody.Friction = 1;
            floorBody.RollingFriction = 1;
            floorBody.Restitution = 1f;
            floorBody.UserObject = "floorBody";
            dynamicsWorld.AddRigidBody(floorBody);

            var loader = new TgcSceneLoader();
            ///Se crea una caja para que haga las veces del Hummer dentro del modelo físico
            TgcTexture texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"\MeshCreator\Scenes\Deposito\Textures\box4.jpg");
            TGCBox boxMesh1 = TGCBox.fromSize(new TGCVector3(20, 20, 20), texture);
            boxMesh1.Position = new TGCVector3(0, 10, 0);
            hummer = boxMesh1.ToMesh("box");
            boxMesh1.Dispose();

            //Se crea el cuerpo rígido de la caja, en la definicio de CreateBox el ultimo parametro representa si se quiere o no
            //calcular el momento de inercia del cuerpo. No calcularlo lo que va a hacer es que la caja que representa el Hummer
            //no rote cuando colicione contra el mundo.
            hummerBody = BulletRigidBodyFactory.Instance.CreateBox(new TGCVector3(55, 20, 80), 10, hummer.Position, 0, 0, 0, 0.55f, false);
            hummerBody.Restitution = 0;
            hummerBody.Gravity = new TGCVector3(0, -100, 0).ToBulletVector3();
            dynamicsWorld.AddRigidBody(hummerBody);

            //Se carga el modelo del Hummer
            hummer = loader.loadSceneFromFile(MediaDir + @"MeshCreator\\Meshes\\Vehiculos\\Hummer\\Hummer-TgcScene.xml").Meshes[0];

            leftright = new TGCVector3(1, 0, 0);
            fowardback = new TGCVector3(0, 0, 1);
        }

        public void Update(TgcD3dInput input)
        {
            var strength = 30.30f;
            dynamicsWorld.StepSimulation(1 / 60f, 100);

            #region Comportamiento

            if (input.keyDown(Key.W))
            {
                //Activa el comportamiento de la simulacion fisica para la capsula
                hummerBody.ActivationState = ActivationState.ActiveTag;
                hummerBody.AngularVelocity = TGCVector3.Empty.ToBulletVector3();
                hummerBody.ApplyCentralImpulse(-strength * fowardback.ToBulletVector3());
            }

            if (input.keyDown(Key.S))
            {
                //Activa el comportamiento de la simulacion fisica para la capsula
                hummerBody.ActivationState = ActivationState.ActiveTag;
                hummerBody.AngularVelocity = TGCVector3.Empty.ToBulletVector3();
                hummerBody.ApplyCentralImpulse(strength * fowardback.ToBulletVector3());
            }

            if (input.keyDown(Key.D))
            {
                //Activa el comportamiento de la simulacion fisica para la capsula
                hummerBody.ActivationState = ActivationState.ActiveTag;
                hummerBody.AngularVelocity = TGCVector3.Empty.ToBulletVector3();
                hummerBody.ApplyCentralImpulse(-strength * leftright.ToBulletVector3());
            }

            if (input.keyDown(Key.A))
            {
                //Activa el comportamiento de la simulacion fisica para la capsula
                hummerBody.ActivationState = ActivationState.ActiveTag;
                hummerBody.AngularVelocity = TGCVector3.Empty.ToBulletVector3();
                hummerBody.ApplyCentralImpulse(strength * leftright.ToBulletVector3());
            }

            #endregion Comportamiento
        }

        public void Render(float time)
        {
            //Hacemos render de la escena.
            foreach (var mesh in meshes) mesh.Render();

            //Se hace el transform a la posicion que devuelve el el Rigid Body del Hummer
            hummer.Position = new TGCVector3(hummerBody.CenterOfMassPosition.X, hummerBody.CenterOfMassPosition.Y + 0, hummerBody.CenterOfMassPosition.Z);
            hummer.Transform = TGCMatrix.Translation(hummerBody.CenterOfMassPosition.X, hummerBody.CenterOfMassPosition.Y, hummerBody.CenterOfMassPosition.Z);
            hummer.Render();
        }

        public void Dispose()
        {
            //Se hace dispose del modelo fisico.
            dynamicsWorld.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            overlappingPairCache.Dispose();
            hummerBody.Dispose();
            floorBody.Dispose();

            //Dispose de Meshes
            foreach (TgcMesh mesh in meshes)
            {
                mesh.Dispose();
            }

            hummer.Dispose();
        }

        public TGCVector3 getPositionHummer()
        {
            return hummer.Position;
        }
    }
}