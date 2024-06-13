using BulletSharp;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using TGC.Core.BulletPhysics;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.Textures;

namespace TGC.Examples.BulletPhysics.Physics
{
    internal class TriangleSpherePhysics
    {
        //Configuracion de la Simulacion Fisica
        private DiscreteDynamicsWorld dynamicsWorld;

        private CollisionDispatcher dispatcher;
        private DefaultCollisionConfiguration collisionConfiguration;
        private SequentialImpulseConstraintSolver constraintSolver;
        private BroadphaseInterface overlappingPairCache;

        //Datos del los triangulos del VertexBuffer
        private CustomVertex.PositionTextured[] triangleDataVB;

        //Dragon Ball
        private RigidBody dragonBall;

        private TGCSphere sphereMesh;
        private TGCVector3 director;

        public void SetTriangleDataVB(CustomVertex.PositionTextured[] newTriangleData)
        {
            triangleDataVB = newTriangleData;
        }

        public void Init(string MediaDir)
        {
            //Creamos el mundo fisico por defecto.
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            constraintSolver = new SequentialImpulseConstraintSolver();
            overlappingPairCache = new DbvtBroadphase(); //AxisSweep3(new BsVector3(-5000f, -5000f, -5000f), new BsVector3(5000f, 5000f, 5000f), 8192);
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, overlappingPairCache, constraintSolver, collisionConfiguration);
            dynamicsWorld.Gravity = new TGCVector3(0, -100f, 0).ToBulletVector3();

            //Creamos el terreno
            var meshRigidBody = BulletRigidBodyFactory.Instance.CreateSurfaceFromHeightMap(triangleDataVB);
            dynamicsWorld.AddRigidBody(meshRigidBody);

            //Creamos la esfera del dragon
            dragonBall = BulletRigidBodyFactory.Instance.CreateBall(30f, 0.75f, new TGCVector3(100f, 500f, 100f));
            dragonBall.SetDamping(0.1f, 0.5f);
            dragonBall.Restitution = 1f;
            dragonBall.Friction = 1;
            dynamicsWorld.AddRigidBody(dragonBall);
            var textureDragonBall = TGCTexture.CreateTexture(D3DDevice.Instance.Device, MediaDir + @"Texturas\dragonball.jpg");
            sphereMesh = new TGCSphere(1, textureDragonBall, TGCVector3.Empty);
            sphereMesh.updateValues();
            director = new TGCVector3(1, 0, 0);
        }

        public void Update(TgcD3dInput input, float elapsedTime, float timeBetweenFrames)
        {
            dynamicsWorld.StepSimulation(elapsedTime, 10, timeBetweenFrames);

            var strength = 1.50f;
            var angle = 5;

            if (input.keyDown(Key.W))
            {
                //Activa el comportamiento de la simulacion fisica para la capsula
                dragonBall.ActivationState = ActivationState.ActiveTag;
                dragonBall.ApplyCentralImpulse(-strength * director.ToBulletVector3());
            }

            if (input.keyDown(Key.S))
            {
                //Activa el comportamiento de la simulacion fisica para la capsula
                dragonBall.ActivationState = ActivationState.ActiveTag;
                dragonBall.ApplyCentralImpulse(strength * director.ToBulletVector3());
            }

            if (input.keyDown(Key.A))
            {
                director.TransformCoordinate(TGCMatrix.RotationY(-angle * 0.001f));
            }

            if (input.keyDown(Key.D))
            {
                director.TransformCoordinate(TGCMatrix.RotationY(angle * 0.001f));
            }

            if (input.keyPressed(Key.Space))
            {
                dragonBall.ActivationState = ActivationState.ActiveTag;
                dragonBall.ApplyCentralImpulse(TGCVector3.Up.ToBulletVector3() * 150);
            }
        }

        public void Render()
        {
            sphereMesh.Transform = TGCMatrix.Scaling(30, 30, 30) * new TGCMatrix(dragonBall.InterpolationWorldTransform);
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