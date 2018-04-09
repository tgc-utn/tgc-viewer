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

namespace TGC.Examples.Bullet.Physics
{
    public class TrianglePhysics
    {
        //Rigid Bodies:
        private BvhTriangleMeshShape floorBody;
        private List<RigidBody> ballBodys;

        private DiscreteDynamicsWorld dynamicsWorld;
        private CollisionDispatcher dispatcher;
        private DefaultCollisionConfiguration collisionConfiguration;
        private SequentialImpulseConstraintSolver constraintSolver;
        private BroadphaseInterface overlappingPairCache;

        private const float TriangleSize = 8.0f;
        private const int NumVertsX = 30;
        private const int NumVertsY = 30;
        private const float WaveHeight = 3.0f;
        private const int NumDynamicBoxesX = 30;
        private const int NumDynamicBoxesY = 30;

        private bool _animatedMesh = true;
        private Stream _vertexStream;
        private BinaryWriter _vertexWriter;

        private TGCVector3 _worldMin = new TGCVector3(-1000, -1000, -1000);
        private TGCVector3 _worldMax = new TGCVector3(1000, 1000, 1000);

        private TriangleIndexVertexArray _indexVertexArrays;
        //private ConvexcastBatch _convexcastBatch;
        private RigidBody _groundObject;
        private ClosestConvexResultCallback _callback;

        public void Init()
        {
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
           // floorBody = new RigidBody(floorInfo);
            //dynamicsWorld.AddRigidBody(floorBody);

            //Creamos una esfera
            var ballBody = this.CreateBall(10f, 1f, 0f, 50f, 0f);
            ballBodys.Add(ballBody);
            dynamicsWorld.AddRigidBody(ballBody);

        }

        public void Update()
        {
            dynamicsWorld.StepSimulation(1 / 60f, 10);
        }

        public void Render()
        {

        }

        public void Dispoose()
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
            var ballInfo = new RigidBodyConstructionInfo(1, ballMotionState, ballShape, ballLocalInertia);
            var ballBody = new RigidBody(ballInfo);
            ballBody.LinearFactor = TGCVector3.One.ToBsVector;
            ballBody.SetDamping(0.1f, 0.5f);
            ballBody.Restitution = 0.5f;
            return ballBody;
        }

        //Ideas para generar el terreno para Bullet
        //We are getting a llitle bit crazy xD https://es.wikipedia.org/wiki/Paraboloide
        //Paraboloide Hiperbolico 
        // definicion matematica
        //(x / a) ^ 2 - ( y / b) ^ 2 - z = 0.
        //
        //DirectX
        //(x / a) ^ 2 - ( z / b) ^ 2 - y = 0.
        private void CreateGround()
        {
            const int totalVerts = NumVertsX * NumVertsY;
            const int totalTriangles = 2 * (NumVertsX - 1) * (NumVertsY - 1);
            const int triangleIndexStride = 3 * sizeof(int);
            const int vertexStride = Vector3.SizeInBytes;

            var mesh = new IndexedMesh();
            mesh.Allocate(totalTriangles, totalVerts, triangleIndexStride, vertexStride);

            var indicesStream = mesh.GetTriangleStream();
            using (var indices = new BinaryWriter(indicesStream))
            {
                for (int x = 0; x < NumVertsX - 1; x++)
                {
                    for (int y = 0; y < NumVertsY - 1; y++)
                    {
                        int row1Index = x * NumVertsX + y;
                        int row2Index = row1Index + NumVertsX;
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

            SetVertexPositions(WaveHeight, 0.0f);

            const bool useQuantizedAabbCompression = true;
            floorBody = new BvhTriangleMeshShape(_indexVertexArrays, useQuantizedAabbCompression);

           // _groundObject = PhysicsHelper.CreateStaticBody(Matrix.Identity, floorBody, World);
            _groundObject.CollisionFlags |= CollisionFlags.StaticObject;
            _groundObject.UserObject = "Ground";
        }

        private void SetVertexPositions(float waveHeight, float offset)
        {
            if (_vertexWriter == null)
            {
                _vertexStream = _indexVertexArrays.GetVertexStream();
                _vertexWriter = new BinaryWriter(_vertexStream);
            }
            _vertexStream.Position = 0;
            for (int i = 0; i < NumVertsX; i++)
            {
                for (int j = 0; j < NumVertsY; j++)
                {
                    _vertexWriter.Write((i - NumVertsX * 0.5f) * TriangleSize);
                    _vertexWriter.Write(waveHeight * (float)Math.Sin(i + offset) * (float)Math.Cos(j + offset));
                    _vertexWriter.Write((j - NumVertsY * 0.5f) * TriangleSize);
                }
            }
        }
        /*
        private void CreateTrimeshGround()
        {
            const float scale = 20.0f;

            //create a triangle-mesh ground
            const int NumVertsX = 20;
            const int NumVertsY = 20;
            const int totalVerts = NumVertsX * NumVertsY;

            const int totalTriangles = 2 * (NumVertsX - 1) * (NumVertsY - 1);

            var vertexArray = new TriangleIndexVertexArray();
            var mesh = new IndexedMesh();
            mesh.Allocate(totalTriangles, totalVerts);
            mesh.NumTriangles = totalTriangles;
            mesh.NumVertices = totalVerts;
            mesh.TriangleIndexStride = 3 * sizeof(int);
            mesh.VertexStride = Vector3.SizeInBytes;
            using (var indicesStream = mesh.GetTriangleStream())
            {
                var indices = new BinaryWriter(indicesStream);
                for (int i = 0; i < NumVertsX - 1; i++)
                {
                    for (int j = 0; j < NumVertsY - 1; j++)
                    {
                        indices.Write(j * NumVertsX + i);
                        indices.Write(j * NumVertsX + i + 1);
                        indices.Write((j + 1) * NumVertsX + i + 1);

                        indices.Write(j * NumVertsX + i);
                        indices.Write((j + 1) * NumVertsX + i + 1);
                        indices.Write((j + 1) * NumVertsX + i);
                    }
                }
                indices.Dispose();
            }

            using (var vertexStream = mesh.GetVertexStream())
            {
                var vertices = new BinaryWriter(vertexStream);
                for (int i = 0; i < NumVertsX; i++)
                {
                    for (int j = 0; j < NumVertsY; j++)
                    {
                        const float waveLength = .2f;
                        float height = (float)(Math.Sin(i * waveLength) * Math.Cos(j * waveLength));

                        vertices.Write(i - NumVertsX * 0.5f);
                        vertices.Write(height);
                        vertices.Write(j - NumVertsY * 0.5f);
                    }
                }
                vertices.Dispose();
            }

            vertexArray.AddIndexedMesh(mesh);
            var groundShape = new BvhTriangleMeshShape(vertexArray, true);
            var groundScaled = new ScaledBvhTriangleMeshShape(groundShape, new Vector3(scale));

            RigidBody ground = PhysicsHelper.CreateStaticBody(Matrix.Identity, groundScaled, World);
            ground.UserObject = "Ground";

            Matrix vehicleTransform = Matrix.Translation(0, -2, 0);
            CreateVehicle(vehicleTransform);
        }*/
    }
}
