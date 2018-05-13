using BulletSharp;
using TGC.Core.Mathematica;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp.Math;
using System.IO;
using System.Globalization;
using System.Windows.Forms;
using DemoFramework;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Direct3D;
using TGC.Core.Textures;
using TGC.Core.Input;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Terrain;

namespace TGC.Examples.Bullet.Physics
{
    public class TrianglePhysics
    {
        private TgcPlane floorMesh;

        private DiscreteDynamicsWorld dynamicsWorld;
        private CollisionDispatcher dispatcher;
        private DefaultCollisionConfiguration collisionConfiguration;
        private SequentialImpulseConstraintSolver constraintSolver;
        private BroadphaseInterface overlappingPairCache;

        private TGCVector3 _worldMin = new TGCVector3(-1000, -1000, -1000);
        private TGCVector3 _worldMax = new TGCVector3(1000, 1000, 1000);

        //Datos del los triangulos del VertexBuffer
        private CustomVertex.PositionTextured[] triangleDataVB;

        //Capsula
        private RigidBody capsule;
        private TGCSphere sphereMesh;

        public TGCVector3 getBallPosition()
        {
           return new TGCVector3(capsule.CenterOfMassPosition.X,capsule.CenterOfMassPosition.Y,capsule.CenterOfMassPosition.Z);
        }

        public void setTriangleDataVB(CustomVertex.PositionTextured[] newTriangleData)
        {
            triangleDataVB = newTriangleData;
        }

        public void Init(String MediaDir)
        {
            //Cargamos objetos de render del framework.
            var floorTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "//Texturas//pasto.jpg");
            floorMesh = new TgcPlane(new TGCVector3(-2000, 0, -2000), new TGCVector3(4000, 0f, 4000), TgcPlane.Orientations.XZplane, floorTexture);

            //Creamos el mundo fisico por defecto.
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            constraintSolver = new SequentialImpulseConstraintSolver();
            overlappingPairCache = new DbvtBroadphase(); //AxisSweep3(new BsVector3(-5000f, -5000f, -5000f), new BsVector3(5000f, 5000f, 5000f), 8192);
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, overlappingPairCache, constraintSolver, collisionConfiguration);
            dynamicsWorld.Gravity = new TGCVector3(0, -100f, 0).ToBsVector;

            /*
             * This come from a bullet page
             * http://www.bulletphysics.org/mediawiki-1.5.8/index.php?title=Code_Snippets
            btTriangleMesh *mTriMesh = new btTriangleMesh();

            while(!done) {
                // For whatever your source of triangles is
                //   give the three points of each triangle:
                btVector3 v0(x0,y0,z0);
                btVector3 v1(x1,y1,z1);
                btVector3 v2(x2,y2,z2);

                // Then add the triangle to the mesh:
                mTriMesh->addTriangle(v0,v1,v2);
            }

            btCollisionShape *mTriMeshShape = new btBvhTriangleMeshShape(mTriMesh,true);

            // Now use mTriMeshShape as your collision shape.
            // Everything else is like a normal rigid body
            */

            /*
            * Para 1 solo triangulo
            var triangle = new Triangle();
            TGCVector3 vector0 = new TGCVector3(0, 0, 0);
            TGCVector3 vector1 = new TGCVector3(100, 0, 0);
            TGCVector3 vector2 = new TGCVector3(0, 0, 100);

            triangleMesh.AddTriangle(vector0.ToBsVector,vector1.ToBsVector,vector2.ToBsVector,false);
            */

            //Triangulos
            var triangleMesh = new TriangleMesh();
            int i = 0;

            while (i < triangleDataVB.Length)
            {
                var triangle = new Triangle();
                TGCVector3 vector0 = new TGCVector3( triangleDataVB[i].X, triangleDataVB[i].Y, triangleDataVB[i].Z);
                TGCVector3 vector1 = new TGCVector3(triangleDataVB[i + 1].X, triangleDataVB[i + 1].Y, triangleDataVB[i + 1].Z);
                TGCVector3 vector2 = new TGCVector3(triangleDataVB[i + 2].X, triangleDataVB[i + 2].Y, triangleDataVB[i + 2].Z);

                i++;
                i++;
                i++;

                triangleMesh.AddTriangle(vector0.ToBsVector, vector1.ToBsVector, vector2.ToBsVector, false);
            }

            CollisionShape meshCollisionShape = new BvhTriangleMeshShape(triangleMesh, true);
            var meshMotionState = new DefaultMotionState();
            var meshRigidBodyInfo = new RigidBodyConstructionInfo(0,meshMotionState,meshCollisionShape);
            RigidBody meshRigidBody = new RigidBody(meshRigidBodyInfo);
            dynamicsWorld.AddRigidBody(meshRigidBody);
            
            capsule = CreateBall(10f, 1f, 200f, 500f, 200f);
            dynamicsWorld.AddRigidBody(capsule);
            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"Texturas\pokeball.jpg");
            //Se crea una esfera de tamaño 1 para escalarla luego (en render)
            sphereMesh = new TGCSphere(1, texture, TGCVector3.Empty);
            //Tgc no crea el vertex buffer hasta invocar a update values.
            sphereMesh.updateValues();

            /*
            //Creamos una esfera
            var ballBody = this.CreateBall(10f, 1f, 0f, 50f, 0f);
            ballBodys.Add(ballBody);
            dynamicsWorld.AddRigidBody(ballBody);*/

        }

        public void Update(TgcD3dInput input)
        {
            dynamicsWorld.StepSimulation(1 / 60f, 100);
            var velocity = 3;

            if (input.keyDown(Key.W) && !( input.keyDown(Key.A) || input.keyDown(Key.D) ))
            {
                capsule.ApplyImpulse(new TGCVector3(velocity, 0, 0).ToBsVector,capsule.CenterOfMassPosition);
            }

            if (input.keyDown(Key.S))
            {
                capsule.LinearVelocity = new TGCVector3(-velocity, 0, 0).ToBsVector;
            }
            
            if (input.keyDown(Key.A))
            {
                capsule.LinearVelocity = new TGCVector3(0, 0, velocity).ToBsVector;
            }

            if (input.keyDown(Key.D))
            {
                capsule.LinearVelocity = new TGCVector3(0, 0, -velocity).ToBsVector;
            }
        }

        public void Render()
        {
            sphereMesh.Transform = TGCMatrix.Scaling(10, 10, 10) * new TGCMatrix(capsule.InterpolationWorldTransform);
            sphereMesh.Render();
        }

        public void Dispose()
        {

        }

        public RigidBody CreateBall(float size, float mass, float originX, float originY, float originZ)
        {
            //Crea una bola de radio 10 origen 50 de 1 kg.
            var ballShape = new SphereShape(size);
            var ballTransform = TGCMatrix.Identity;
            ballTransform.Origin = new TGCVector3(originX, originY, originZ);
            var ballMotionState = new DefaultMotionState(ballTransform.ToBsMatrix);
            //Podriamos no calcular la inercia para que no rote, pero es correcto que rote tambien.
            var ballLocalInertia = ballShape.CalculateLocalInertia(mass);
            var ballInfo = new RigidBodyConstructionInfo(10, ballMotionState, ballShape, ballLocalInertia);
            var ballBody = new RigidBody(ballInfo);
            ballBody.LinearFactor = TGCVector3.One.ToBsVector;
            ballBody.SetDamping(0.1f, 0.5f);
            ballBody.Restitution = 0.9f;
            return ballBody;
        }

        }
    }

