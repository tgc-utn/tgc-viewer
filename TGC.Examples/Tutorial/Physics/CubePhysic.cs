using BulletSharp;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.BulletPhysics;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

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
        private List<RigidBody> buildings = new List<RigidBody>();

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
        
        public void Init(String MediaDir)
        {
            #region Configuracion Basica de World

            //Creamos el mundo fisico por defecto.
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            constraintSolver = new SequentialImpulseConstraintSolver();
            overlappingPairCache = new DbvtBroadphase(); //AxisSweep3(new BsVector3(-5000f, -5000f, -5000f), new BsVector3(5000f, 5000f, 5000f), 8192);
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, overlappingPairCache, constraintSolver, collisionConfiguration);
            dynamicsWorld.Gravity = new TGCVector3(0, -100f, 0).ToBsVector;

            #endregion Configuracion Basica de World

            var floorShape = new StaticPlaneShape(TGCVector3.Up.ToBsVector, 0);
            var floorMotionState = new DefaultMotionState();
            var floorInfo = new RigidBodyConstructionInfo(0, floorMotionState, floorShape);
            var floorBody = new RigidBody(floorInfo);
            floorBody.Restitution = 0;
            dynamicsWorld.AddRigidBody(floorBody);

            //foreach (TgcMesh mesh in meshes)
            //{
            var sizeX = meshes[0].BoundingBox.PMax.X - meshes[0].BoundingBox.PMin.X;
            var sizey = meshes[0].BoundingBox.PMax.Y - meshes[0].BoundingBox.PMin.Y;
            var sizez = meshes[0].BoundingBox.PMax.Z - meshes[0].BoundingBox.PMin.Z;
            var buildingBody = BulletRigidBodyConstructor.CreateBox(new TGCVector3(sizeX/16,sizey/16,sizez/16), 1000, meshes[0].Position/*new TGCVector3((meshes[0].BoundingBox.Position.X + meshes[0].BoundingBox.calculateSize().X/2 ), (meshes[0].BoundingBox.Position.Y + meshes[0].BoundingBox.calculateSize().Y/2 ), (meshes[0].BoundingBox.Position.Z + meshes[0].BoundingBox.calculateSize().Z/2 ))*/ , 0, 0, 0, 0.5f);
                dynamicsWorld.AddRigidBody(buildingBody);
                buildings.Add(buildingBody);
            //}

            var loader = new TgcSceneLoader();
            hummer = loader.loadSceneFromFile(MediaDir + @"MeshCreator\\Meshes\\Vehiculos\\Hummer\\Hummer-TgcScene.xml").Meshes[0];
            hummer.Position = new TGCVector3(0,500, 0);
            hummer.UpdateMeshTransform();
            hummer.Enabled = true;
            hummerBody = BulletRigidBodyConstructor.CreateBox(hummer.BoundingBox.calculateSize(), 10, hummer.Position, 0, 0, 0, 0.5f);
            hummerBody.Restitution = 0;
            dynamicsWorld.AddRigidBody(hummerBody);
           
            leftright = new TGCVector3(1, 0, 0);
            fowardback = new TGCVector3(0, 0, 1);
        }

        public void Update(TgcD3dInput input)
        {
            var strength = 10.30f;
            dynamicsWorld.StepSimulation(1 / 60f, 100);

            #region Comportamiento
            if (input.keyDown(Key.W))
            {
                //Activa el comportamiento de la simulacion fisica para la capsula
                hummerBody.ActivationState = ActivationState.ActiveTag;
                hummerBody.AngularVelocity = TGCVector3.Empty.ToBsVector;
                hummerBody.ApplyCentralImpulse(-strength * fowardback.ToBsVector);
            }

            if (input.keyDown(Key.S))
            {
                //Activa el comportamiento de la simulacion fisica para la capsula
                hummerBody.ActivationState = ActivationState.ActiveTag;
                hummerBody.AngularVelocity = TGCVector3.Empty.ToBsVector;
                hummerBody.ApplyCentralImpulse(strength * fowardback.ToBsVector);
            }

            if (input.keyDown(Key.D))
            {
                //Activa el comportamiento de la simulacion fisica para la capsula
                hummerBody.ActivationState = ActivationState.ActiveTag;
                hummerBody.AngularVelocity = TGCVector3.Empty.ToBsVector;
                hummerBody.ApplyCentralImpulse(-strength * leftright.ToBsVector);
            }

            if (input.keyDown(Key.A))
            {
                //Activa el comportamiento de la simulacion fisica para la capsula
                hummerBody.ActivationState = ActivationState.ActiveTag;
                hummerBody.AngularVelocity = TGCVector3.Empty.ToBsVector;
                hummerBody.ApplyCentralImpulse(strength * leftright.ToBsVector);
            }

            #endregion

        }

        public void Render(float time)
        {

            //for(int i = 0 ; i < 20 ; i++ )
            //{
            meshes[0].Transform = new TGCMatrix(buildings[0].InterpolationWorldTransform);// * TGCMatrix.Translation(new TGCVector3(0,-100,0)) * TGCMatrix.Translation(new TGCVector3(buildings[0].CenterOfMassPosition.X, buildings[0].CenterOfMassPosition.Y, buildings[0].CenterOfMassPosition.Z));
                //meshes[0].Scale = new TGCVector3(1.5f, 1.5f, 1.5f);
                //meshes[0].Position = new TGCVector3( buildings[0].CenterOfMassPosition.X, buildings[0].CenterOfMassPosition.Y, buildings[0].CenterOfMassPosition.Z);
                //meshes[0].UpdateMeshTransform();
                meshes[0].Render();
            meshes[0].BoundingBox.transform(new TGCMatrix(buildings[0].InterpolationWorldTransform));
            meshes[0].BoundingBox.Render();
            //}

            hummer.Transform = new TGCMatrix(hummerBody.InterpolationWorldTransform);
            hummer.Position = new TGCVector3(hummerBody.CenterOfMassPosition.X, hummerBody.CenterOfMassPosition.Y, hummerBody.CenterOfMassPosition.Z);
            //hummer.Scale = new TGCVector3()
            hummer.UpdateMeshTransform();
            hummer.Render();
        }

        public void Dispose()
        {
            //Dispose de Meshes
            foreach (TgcMesh mesh in meshes)
            {
                mesh.Dispose();
            }

            //Se hace dispose del modelo fisico.
            dynamicsWorld.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            overlappingPairCache.Dispose();
        }

        public TGCVector3 getPositionHummer()
        {
            return hummer.Position;
        }
    }
}
