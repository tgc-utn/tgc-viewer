using BulletSharp;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.Textures;

namespace TGC.Examples.Bullet.Physics
{
    class TriangleSpherePhysics
    {
        //Configuracion de la Simulacion Fisica
        private DiscreteDynamicsWorld dynamicsWorld;
        private CollisionDispatcher dispatcher;
        private DefaultCollisionConfiguration collisionConfiguration;
        private SequentialImpulseConstraintSolver constraintSolver;
        private BroadphaseInterface overlappingPairCache;

        private TGCVector3 _worldMin = new TGCVector3(-1000, -1000, -1000);
        private TGCVector3 _worldMax = new TGCVector3(1000, 1000, 1000);

        //Datos del los triangulos del VertexBuffer
        private CustomVertex.PositionTextured[] triangleDataVB;

        //Dragon Ball
        private RigidBody dragonBall;
        private TGCSphere sphereMesh;
        private TGCVector3 director;

        public void setTriangleDataVB(CustomVertex.PositionTextured[] newTriangleData)
        {
            triangleDataVB = newTriangleData;
        }

        public void Init(String MediaDir)
        {
            //Creamos el mundo fisico por defecto.
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            constraintSolver = new SequentialImpulseConstraintSolver();
            overlappingPairCache = new DbvtBroadphase(); //AxisSweep3(new BsVector3(-5000f, -5000f, -5000f), new BsVector3(5000f, 5000f, 5000f), 8192);
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, overlappingPairCache, constraintSolver, collisionConfiguration);
            dynamicsWorld.Gravity = new TGCVector3(0, -100f, 0).ToBsVector;

            //Creamos el terreno
            var meshRigidBody = Core.BulletPhysics.BulletRigidBodyConstructor.CreateSurfaceFromHeighMap(triangleDataVB);
            meshRigidBody.Friction = 1;
            dynamicsWorld.AddRigidBody(meshRigidBody);

            //Creamos la esfera del dragon
            dragonBall = Core.BulletPhysics.BulletRigidBodyConstructor.CreateBall(60f, 0.5f, new TGCVector3(100f, 500f, 100f));
            dragonBall.Friction = 1;
            dynamicsWorld.AddRigidBody(dragonBall);
            var textureDragonBall = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"Texturas\dragonball.jpg");
            sphereMesh = new TGCSphere(1, textureDragonBall, TGCVector3.Empty);
            sphereMesh.updateValues();
            director = new TGCVector3(1,0,0);
        }

        public void Update(TgcD3dInput input)
        {
            dynamicsWorld.StepSimulation(1 / 60f, 100);
            var strenght = 0.10f;
            var angle = 5;

            if (input.keyDown(Key.W))
            {
                //Activa el comportamiento de la simulacion fisica para la capsula
                dragonBall.ActivationState = ActivationState.ActiveTag;
                dragonBall.AngularVelocity = TGCVector3.Empty.ToBsVector;
                dragonBall.ApplyImpulse(-strenght * director.ToBsVector, new TGCVector3(dragonBall.CenterOfMassPosition.X, dragonBall.CenterOfMassPosition.Y , dragonBall.CenterOfMassPosition.Z).ToBsVector);
            }

            if (input.keyDown(Key.S))
            {
                //Activa el comportamiento de la simulacion fisica para la capsula
                dragonBall.ActivationState = ActivationState.ActiveTag;
                dragonBall.AngularVelocity = TGCVector3.Empty.ToBsVector;
                dragonBall.ApplyImpulse(strenght * director.ToBsVector, new TGCVector3(dragonBall.CenterOfMassPosition.X, dragonBall.CenterOfMassPosition.Y , dragonBall.CenterOfMassPosition.Z).ToBsVector);
            }

            if (input.keyDown(Key.A))
            {
                director.TransformCoordinate(TGCMatrix.RotationY(-angle * 0.01f));
            }

            if (input.keyDown(Key.D))
            {
                director.TransformCoordinate(TGCMatrix.RotationY(angle * 0.01f));
            }
        }

        public void Render()
        {
            sphereMesh.Transform = TGCMatrix.Scaling(60, 60, 60) * new TGCMatrix(dragonBall.InterpolationWorldTransform);
            sphereMesh.Render();
        }

        public void Dispose()
        {
            sphereMesh.Dispose();

            //Se hace dispose del modelo fisico. 
            dynamicsWorld.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            overlappingPairCache.Dispose();
        }
    }
}
