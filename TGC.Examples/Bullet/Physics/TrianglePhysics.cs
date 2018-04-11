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
using Microsoft.DirectX.Direct3D;

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

        private float TriangleSize = 8.0f;
        private const int NumVertsX = 30;
        private const int NumVertsY = 30;
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

        //Datos del los triangulos del VertexBuffer
        private CustomVertex.PositionTextured[] triangleDataVB;
        private int totalTriangles;
        private int totalVerts;

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
            CreateGround();
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
            var ballInfo = new RigidBodyConstructionInfo(1, ballMotionState, ballShape, ballLocalInertia);
            var ballBody = new RigidBody(ballInfo);
            ballBody.LinearFactor = TGCVector3.One.ToBsVector;
            ballBody.SetDamping(0.1f, 0.5f);
            ballBody.Restitution = 0.5f;
            return ballBody;
        }

        private void CreateGround()
        {
            
            //const int totalVerts = NumVertsX * NumVertsY;

            //const int totalTriangles = 2 * (NumVertsX - 1) * (NumVertsY - 1);

            const int triangleIndexStride = 3 * sizeof(int);
            const int vertexStride = Vector3.SizeInBytes;

            var mesh = new IndexedMesh();
            mesh.Allocate(totalTriangles, totalVerts, triangleIndexStride, vertexStride);

            //los vertices X e Y de aqui representan vertice X y vertice Z
            var indicesStream = mesh.GetTriangleStream();
            // Hay que ciclar por los vertices que se cargaron en el VertexBuffer
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

            //Posiciona los vertices segun las posiciones del modelo en xyz
            // o por lo menos deberia hacer esto. En el ejemplo original, armaba unas olas 
            //a partir de una altura y un offset
            SetVertexPositions(); //TODO: ver como pasar la data del VertexBuffer para no tener que recalcular las posiciones

            const bool useQuantizedAabbCompression = true;
            floorBody = new BvhTriangleMeshShape(_indexVertexArrays, useQuantizedAabbCompression);

           // _groundObject = PhysicsHelper.CreateStaticBody(Matrix.Identity, floorBody, World);
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
            for (int i = 0; i < NumVertsX; i++)
            {
                for (int j = 0; j < NumVertsY; j++)
                {
                    _vertexWriter.Write((i - NumVertsX * 0.5f) * TriangleSize);
                    _vertexWriter.Write((float)Math.Sin(i) * (float)Math.Cos(j));
                    _vertexWriter.Write((j - NumVertsY * 0.5f) * TriangleSize);
                }
            }
        }
    }
}
