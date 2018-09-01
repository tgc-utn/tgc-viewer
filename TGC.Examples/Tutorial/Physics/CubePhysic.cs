using BulletSharp;
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
    class CubePhysic
    {
        //Configuracion de la Simulacion Fisica
        private DiscreteDynamicsWorld dynamicsWorld;

        private CollisionDispatcher dispatcher;
        private DefaultCollisionConfiguration collisionConfiguration;
        private SequentialImpulseConstraintSolver constraintSolver;
        private BroadphaseInterface overlappingPairCache;

        private List<TgcMesh> meshes = new List<TgcMesh>();

        private TgcMesh hummer;
        RigidBody hummerBody;

        void Init()
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

            foreach(TgcMesh mesh in meshes)
            {
                var buildingBody = BulletRigidBodyConstructor.CreateBox(mesh.BoundingBox.calculateSize(), 0, mesh.BoundingBox.Position, 0, 0, 0, 0.5f);
                dynamicsWorld.AddRigidBody(buildingBody);
            }

            hummerBody = BulletRigidBodyConstructor.CreateBox(hummer.BoundingBox.calculateSize(), 10, hummer.BoundingBox.Position, 0, 0, 0, 0.5f);
            dynamicsWorld.AddRigidBody(hummerBody);
        }

        public void Update(TgcD3dInput input)
        {
            dynamicsWorld.StepSimulation(1 / 60f, 100);

        }

        public void Render()
        {

            foreach (TgcMesh mesh in meshes)
            {
                var buildingBody = BulletRigidBodyConstructor.CreateBox(mesh.BoundingBox.calculateSize(), 0, mesh.BoundingBox.Position, 0, 0, 0, 0.5f);
                dynamicsWorld.AddRigidBody(buildingBody);
            }

        }

        public void Dispose()
        {
            //Dispose de Meshes

            //Se hace dispose del modelo fisico.
            dynamicsWorld.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            overlappingPairCache.Dispose();
        }
    }
}
