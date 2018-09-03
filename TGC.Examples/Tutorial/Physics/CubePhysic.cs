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
        RigidBody hummerBody;
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
        
        public void Init()
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
                buildings.Add(buildingBody);
            }

            hummerBody = BulletRigidBodyConstructor.CreateBox(hummer.BoundingBox.calculateSize(), 10, hummer.BoundingBox.Position, 0, 0, 0, 0.5f);
            dynamicsWorld.AddRigidBody(hummerBody);

            fowardback = new TGCVector3(0, 0, 1);
            leftright = new TGCVector3(1, 0, 0);
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

        public void Render()
        {
            for(int i = 0 ; i < buildings.Capacity ; i++ )
            {
                meshes[i].Transform = new TGCMatrix(buildings[i].InterpolationWorldTransform);
                meshes[i].Render();
            }

            hummer.Transform = new TGCMatrix(hummerBody.InterpolationWorldTransform);
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
    }
}
