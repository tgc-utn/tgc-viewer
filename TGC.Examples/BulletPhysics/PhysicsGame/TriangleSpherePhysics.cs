using BulletSharp;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using TGC.Core.BulletPhysics;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.Textures;

namespace TGC.Examples.Bullet.Physics
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

        //BB8
        private RigidBody bb8rigidbody;
        private TGCSphere bb8meshbody;
        private TGCSphere bb8meshhead;
        private TGCVector3 director;
        private TGCVector3 directorSide;
        private TGCVector3 offsetHead;

        //Portal Box
        private TGCBox portalBox;
        private RigidBody portalBoxRigidBody;
        private List<RigidBody> portalBoxes;

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
            overlappingPairCache = new DbvtBroadphase(); 
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, overlappingPairCache, constraintSolver, collisionConfiguration);
            dynamicsWorld.Gravity = new TGCVector3(0, -100f, 0).ToBulletVector3();

            //Creamos el terreno
            RigidBody meshRigidBody = BulletRigidBodyFactory.Instance.CreateSurfaceFromHeighMap(triangleDataVB);
            dynamicsWorld.AddRigidBody(meshRigidBody);

            //Creamos el cuerpo de BB8
            bb8rigidbody = BulletRigidBodyFactory.Instance.CreateBall(30f, 0.75f, new TGCVector3(100f, 500f, 100f));
            bb8rigidbody.SetDamping(0.1f, 0.5f);
            bb8rigidbody.Restitution = 1f;
            bb8rigidbody.Friction = 1;
            dynamicsWorld.AddRigidBody(bb8rigidbody);

            TgcTexture bb8BodyTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"BB8\bb8-3D-model_D.jpg");
            bb8meshbody = new TGCSphere(1, bb8BodyTexture, TGCVector3.Empty);
            bb8meshbody.BasePoly = TGCSphere.eBasePoly.CUBE;
            bb8meshbody.LevelOfDetail = 5;
            bb8meshbody.updateValues();

            TgcTexture bb8HeadTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"BB8\D.jpg");
            bb8meshhead = new TGCSphere(1, bb8HeadTexture, TGCVector3.Empty);
            bb8meshhead.updateValues();
            offsetHead = new TGCVector3(0, 25, 0);

            #region Portal Boxes Init
            //Cajas Portal
            /*
            TgcTexture portalBoxTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"BB8\portalCubeFace.jpg");
            portalBox = TGCBox.fromSize(new TGCVector3(20, 20, 20), portalBoxTexture);

            portalBoxes = new List<RigidBody>();
            for (int i = 0; i < 180; i++)
            {
                portalBoxRigidBody = BulletRigidBodyFactory.Instance.CreateBox(new TGCVector3(40, 40, 40), 0.2f, new TGCVector3(900*FastMath.Cos(50*i + FastMath.PI), 450 + (50*i) , 1400*FastMath.Sin(i*30)), 0, 0, 0, 1, true);
                dynamicsWorld.AddRigidBody(portalBoxRigidBody);
                portalBoxes.Add(portalBoxRigidBody);
            }*/
            #endregion

            director = new TGCVector3(1, 0, 0);
            directorSide = new TGCVector3(0, 0, 1);

        }

        public void Update(TgcD3dInput input)
        {
            dynamicsWorld.StepSimulation(1 / 60f, 100);
            float strength = 1.50f;
            int angle = 5;

            if (input.keyDown(Key.W))
            {
                //Activa el comportamiento de la simulacion fisica para la capsula
                bb8rigidbody.ActivationState = ActivationState.ActiveTag;
                bb8rigidbody.ApplyCentralImpulse(-strength * director.ToBulletVector3());
            }

            if (input.keyDown(Key.S))
            {
                //Activa el comportamiento de la simulacion fisica para la capsula
                bb8rigidbody.ActivationState = ActivationState.ActiveTag;
                bb8rigidbody.ApplyCentralImpulse(strength * director.ToBulletVector3());
            }

            if (input.keyDown(Key.A))
            {
                bb8rigidbody.ActivationState = ActivationState.ActiveTag;
                bb8rigidbody.ApplyCentralImpulse(-strength * directorSide.ToBulletVector3());
                //director.TransformCoordinate(TGCMatrix.RotationY(-angle * 0.001f));
            }

            if (input.keyDown(Key.D))
            {
                bb8rigidbody.ActivationState = ActivationState.ActiveTag;
                bb8rigidbody.ApplyCentralImpulse(strength * directorSide.ToBulletVector3());
                //director.TransformCoordinate(TGCMatrix.RotationY(angle * 0.001f));
            }

            bb8meshhead.Position = new TGCVector3(bb8rigidbody.CenterOfMassPosition) + offsetHead;
        }

        public void Render()
        {
            bb8meshbody.Transform = TGCMatrix.Scaling(30, 30, 30) * new TGCMatrix(bb8rigidbody.InterpolationWorldTransform);
            bb8meshbody.Render();

            bb8meshhead.Transform = TGCMatrix.Scaling(15, 15, 15) * TGCMatrix.Translation(bb8meshhead.Position);
            bb8meshhead.Render();

            #region Portal Boxes Render
            /*
            foreach (var portalBoxRigidBody in portalBoxes)
            {
                portalBox.Transform = TGCMatrix.Scaling(4, 4, 4) * new TGCMatrix(portalBoxRigidBody.InterpolationWorldTransform);
                portalBox.Render();
            }*/
            #endregion

        }

        public void Dispose()
        {
            bb8meshbody.Dispose();

            //Se hace dispose del modelo fisico.
            dynamicsWorld.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            overlappingPairCache.Dispose();
        }
    }
}