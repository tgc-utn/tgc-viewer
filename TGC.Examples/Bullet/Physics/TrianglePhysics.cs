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

namespace TGC.Examples.Bullet.Physics
{
    public class TrianglePhysics
    {
        private TgcPlane floorMesh;

        //Rigid Bodies:
        private RigidBody floorBody;
        private BvhTriangleMeshShape GroundShape;
        //private BvhTriangleMeshShape floorBody;
        private List<RigidBody> ballBodys;

        private DiscreteDynamicsWorld dynamicsWorld;
        private CollisionDispatcher dispatcher;
        private DefaultCollisionConfiguration collisionConfiguration;
        private SequentialImpulseConstraintSolver constraintSolver;
        private BroadphaseInterface overlappingPairCache;

        private float TriangleSize = 80f;
        private int NumVertsX = 30;
        private int NumVertsY = 30;
        private int NumDynamicBoxesX = 30;
        private int NumDynamicBoxesY = 30;

        private Stream _vertexStream;
        private BinaryWriter _vertexWriter;

        private TGCVector3 _worldMin = new TGCVector3(-1000, -1000, -1000);
        private TGCVector3 _worldMax = new TGCVector3(1000, 1000, 1000);

        private TriangleIndexVertexArray _indexVertexArrays;
        //private ConvexcastBatch _convexcastBatch;
        private RigidBody _groundObject;
        private ClosestConvexResultCallback _callback;

        //Datos del los triangulos del VertexBuffer
        private CustomVertex.PositionTextured[] triangleDataVB;
        private int totalTriangles;
        private int totalVerts;

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

        public void setTotalTriangles(int triangles)
        {
            totalTriangles = triangles;
        }

        public void setTotalVerts(int vertexes)
        {
            totalVerts = vertexes;
        }

        public void setNumVertsX(int vertsX)
        {
            NumVertsX = vertsX;
        }

        public void setNumVertsY(int vertsY)
        {
            NumVertsY = vertsY;
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
            dynamicsWorld.Gravity = new TGCVector3(0, -10f, 0).ToBsVector;

            //El piso es un plano estatico se dice que si tiene masa 0 es estatico.
            var floorShape = new StaticPlaneShape(TGCVector3.Up.ToBsVector, 0);
            var floorMotionState = new DefaultMotionState();
            var floorInfo = new RigidBodyConstructionInfo(0, floorMotionState, floorShape);
            floorBody = new RigidBody(floorInfo);
            dynamicsWorld.AddRigidBody(floorBody);

            int i;
            Matrix tr;
            Matrix vehicleTr;
            //if (UseTrimeshGround)
            //{
            const float scale = 20.0f;

            //create a triangle-mesh ground
            int vertStride = Vector3.SizeInBytes;
            int indexStride = 3 * sizeof(int);

            const int NUM_VERTS_X = 20;
            const int NUM_VERTS_Y = 20;
            const int totalVerts = NUM_VERTS_X * NUM_VERTS_Y;

            const int totalTriangles = 2 * (NUM_VERTS_X - 1) * (NUM_VERTS_Y - 1);

            TriangleIndexVertexArray vertexArray = new TriangleIndexVertexArray();
            IndexedMesh mesh = new IndexedMesh();
            mesh.Allocate(totalTriangles, totalVerts, indexStride, vertStride);

            if (_vertexWriter == null)
            {
                _vertexStream = _indexVertexArrays.GetVertexStream();
                _vertexWriter = new BinaryWriter(_vertexStream);
            }
            _vertexStream.Position = 0;
            for (i = 0; i < NUM_VERTS_X; i++)
            {
                for (int j = 0; j < NUM_VERTS_Y; j++)
                {
                    float wl = .2f;
                    float height = 20.0f * (float)(Math.Sin(i * wl) * Math.Cos(j * wl));

                    _vertexWriter.Write((i - NUM_VERTS_X * 0.5f) * scale);
                    _vertexWriter.Write(height);
                    _vertexWriter.Write((j - NUM_VERTS_Y * 0.5f) * scale);
                }
            }

            //int index = 0;
            var idata = mesh.GetTriangleStream();
            using (var indices = new BinaryWriter(idata))
            {
                for (i = 0; i < NUM_VERTS_X - 1; i++)
                {
                    for (int j = 0; j < NUM_VERTS_Y - 1; j++)
                    {
                        int row1Index = i + j;
                        int row2Index = row1Index + i;
                        indices.Write(row1Index);
                        indices.Write(row1Index + 1);
                        indices.Write(row2Index + 1);

                        indices.Write(row1Index);
                        indices.Write(row2Index + 1);
                        indices.Write(row2Index);
                        /*
                        idata[index++] = j * NUM_VERTS_X + i;
                        idata[index++] = j * NUM_VERTS_X + i + 1;
                        idata[index++] = (j + 1) * NUM_VERTS_X + i + 1;

                        idata[index++] = j * NUM_VERTS_X + i;
                        idata[index++] = (j + 1) * NUM_VERTS_X + i + 1;
                        idata[index++] = (j + 1) * NUM_VERTS_X + i;
                        */
                    }
                }
            }

            vertexArray.AddIndexedMesh(mesh);
            GroundShape = new BvhTriangleMeshShape(vertexArray, true);

            tr = Matrix.Identity;
            vehicleTr = Matrix.Translation(0, -2, 0);

            //create ground object
            RigidBody ground = PhysicsHelper.CreateStaticBody(tr, GroundShape, dynamicsWorld);
            ground.UserObject = "Ground";

            //CreateGround();

            capsule = CreateBall(10f, 1f, 10f, 500f, 10f);
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
            /*
            if (input.keyUp(Key.W))
            {
                capsule.LinearVelocity = new TGCVector3(1, 0, 0).ToBsVector;
                capsule.ApplyCentralForce(new TGCVector3(1, 0, 0).ToBsVector);
                capsule.ApplyCentralImpulse(new TGCVector3(1, 0, 0).ToBsVector);
                capsule.UpdateInertiaTensor();
            }

            if (input.keyUp(Key.S))
            {

            }

            if (input.keyUp(Key.A))
            {

            }

            if (input.keyUp(Key.D))
            {

            }*/
        }

        public void Render()
        {
            sphereMesh.Transform = TGCMatrix.Scaling(10, 10, 10) * new TGCMatrix(capsule.InterpolationWorldTransform);
            sphereMesh.Render();

            //El render del piso deberia manejarse con el shader de tgcterrain
            //floorMesh.Render();
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

        private void CreateGround()
        {

            int totalVertsI = NumVertsX * NumVertsY;

            int totalTrianglesI = 2 * (NumVertsX - 1) * (NumVertsY - 1);

            const int triangleIndexStride = 3 * sizeof(int);
            const int vertexStride = Vector3.SizeInBytes;

            //triangleDataVB.Length;

            foreach (var vertex in triangleDataVB)
            {

            }

            var mesh = new IndexedMesh();
            mesh.Allocate(338, 196, triangleIndexStride, vertexStride);

            //los vertices X e Y de aqui representan vertice X y vertice Z
            var indicesStream = mesh.GetTriangleStream();
            // Hay que ciclar por los vertices que se cargaron en el VertexBuffer
            using (var indices = new BinaryWriter(indicesStream))
            {
                for (int x = 0; x < NumVertsX - 1; x++)
                {
                    for (int y = 0; y < NumVertsY - 1; y++)
                    {
                        int row1Index = x + y;
                        int row2Index = row1Index + x;
                        indices.Write(row1Index);
                        indices.Write(row1Index + 1);
                        indices.Write(row2Index + 1);

                        indices.Write(row1Index);
                        indices.Write(row2Index + 1);
                        indices.Write(row2Index);
                    }
                }
            }

            _indexVertexArrays = new TriangleIndexVertexArray();
            _indexVertexArrays.AddIndexedMesh(mesh);

            //Posiciona los vertices segun las posiciones del modelo en xyz
            // o por lo menos deberia hacer esto. En el ejemplo original, armaba unas olas 
            //a partir de una altura y un offset
            //Copiar la construccion de la superficie que este en la clase BulletSurface
            //SetVertexPositions(); //TODO: ver como pasar la data del VertexBuffer para no tener que recalcular las posiciones
            /*
            const bool useQuantizedAabbCompression = true;
            floorBody = new BvhTriangleMeshShape(_indexVertexArrays, useQuantizedAabbCompression);
            */

            const bool useQuantizedAabbCompression = true;
            GroundShape = new BvhTriangleMeshShape(_indexVertexArrays, useQuantizedAabbCompression);

            _groundObject = PhysicsHelper.CreateStaticBody(Matrix.Identity, GroundShape, dynamicsWorld);
            _groundObject.CollisionFlags |= CollisionFlags.StaticObject;
            _groundObject.UserObject = "Ground";
         }

         
         private void SetVertexPositions()
         {
             if (_vertexWriter == null)
             {
                 _vertexStream = _indexVertexArrays.GetVertexStream();
                 _vertexWriter = new BinaryWriter(_vertexStream);
             }
             _vertexStream.Position = 0;
             for (int i = 0; i < 14; i++)
             {
                 for (int j = 0; j < 14; j++)
                 {
                    var x = (i - 14 ) * TriangleSize;
                    var z = (j - 14 ) * TriangleSize;
                    _vertexWriter.Write(x);
                    _vertexWriter.Write(30);
                     //_vertexWriter.Write((FastMath.Pow2((x) / 32) - FastMath.Pow2((z) / 32)));// (float)Math.Sin(i) * (float)Math.Cos(j));
                    _vertexWriter.Write(z);
                 }
             }
         }
        }
    }

