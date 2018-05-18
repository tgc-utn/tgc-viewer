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
using TGC.Core.SkeletalAnimation;

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
        private CapsuleShape capsulilla;
        private RigidBody capsuleRigidBody;
        private RigidBody capsule;
        private TGCSphere sphereMesh;
        private TgcSkeletalMesh personaje;
        private RigidBody box;
        private RigidBody boxB;
        private TGCBox boxMesh;
        private TGCBox boxMeshB;
        private TGCVector3 director;

        public TGCVector3 getBallPosition()
        {
            //Capsula
            //return new TGCVector3(capsuleRigidBody.CenterOfMassPosition.X, capsuleRigidBody.CenterOfMassPosition.Y, capsuleRigidBody.CenterOfMassPosition.Z);
            //Esfera
            return new TGCVector3(capsule.CenterOfMassPosition.X, capsule.CenterOfMassPosition.Y, capsule.CenterOfMassPosition.Z);
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

            //Capsula?
            capsulilla = new CapsuleShape(10, 50);
            
            var capsuleTransform = TGCMatrix.Identity;
            capsuleTransform.Origin = new TGCVector3(200, 500, 200);
            var capsuleMotionState = new DefaultMotionState(capsuleTransform.ToBsMatrix);
            var capsuleInertia = capsulilla.CalculateLocalInertia(1000);
            var capsuleRigidBodyInfo = new RigidBodyConstructionInfo(1, capsuleMotionState, capsulilla, capsuleInertia);
            capsuleRigidBody = new RigidBody(capsuleRigidBodyInfo);
            capsuleRigidBody.LinearFactor = TGCVector3.One.ToBsVector;
            capsuleRigidBody.SetDamping(0.1f, 0f);
            capsuleRigidBody.Restitution = 1f;
            
            dynamicsWorld.AddRigidBody(capsuleRigidBody);

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
            TGCVector3 vector0;
            TGCVector3 vector1;
            TGCVector3 vector2;

            while (i < triangleDataVB.Length)
            {
                var triangle = new Triangle();
                vector0 = new TGCVector3(triangleDataVB[i].X, triangleDataVB[i].Y, triangleDataVB[i].Z);
                vector1 = new TGCVector3(triangleDataVB[i + 1].X, triangleDataVB[i + 1].Y, triangleDataVB[i + 1].Z);
                vector2 = new TGCVector3(triangleDataVB[i + 2].X, triangleDataVB[i + 2].Y, triangleDataVB[i + 2].Z);

                i = i + 3;

                triangleMesh.AddTriangle(vector0.ToBsVector, vector1.ToBsVector, vector2.ToBsVector, false);
            }

            CollisionShape meshCollisionShape = new BvhTriangleMeshShape(triangleMesh, true);
            var meshMotionState = new DefaultMotionState();
            var meshRigidBodyInfo = new RigidBodyConstructionInfo(0,meshMotionState,meshCollisionShape);
            RigidBody meshRigidBody = new RigidBody(meshRigidBodyInfo);
            dynamicsWorld.AddRigidBody(meshRigidBody);

            
            capsule = CreateBall(10f, 10f, 200f, 500f, 200f);
            dynamicsWorld.AddRigidBody(capsule);

            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"Texturas\pokeball.jpg");
            var textureBox = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"MeshCreator\Textures\Madera\cajaMadera2.jpg");
            //Se crea una esfera de tamaño 1 para escalarla luego (en render)
            sphereMesh = new TGCSphere(1, texture, TGCVector3.Empty);
            //Tgc no crea el vertex buffer hasta invocar a update values.
            sphereMesh.updateValues();

            //Cargamos personaje
            var skeletalLoader = new TgcSkeletalLoader();
            personaje = skeletalLoader.loadMeshAndAnimationsFromFile(
                    MediaDir + "SkeletalAnimations\\Robot\\Robot-TgcSkeletalMesh.xml",
                    MediaDir + "SkeletalAnimations\\Robot\\",
                    new[]
                    {
                        MediaDir + "SkeletalAnimations\\Robot\\Caminando-TgcSkeletalAnim.xml",
                        MediaDir + "SkeletalAnimations\\Robot\\Parado-TgcSkeletalAnim.xml"
                    });

            //Le cambiamos la textura para diferenciarlo un poco
            personaje.changeDiffuseMaps(new[]
            {
                TgcTexture.createTexture(D3DDevice.Instance.Device,
                    MediaDir + "SkeletalAnimations\\Robot\\Textures\\uvwGreen.jpg")
            });

            //Configurar animacion inicial
            personaje.playAnimation("Parado", true);
            
            box = CreateStaticBox(20, new Vector3(0,12,0));
            dynamicsWorld.AddRigidBody(box);
            boxMesh = TGCBox.fromSize(new TGCVector3(30f, 30f, 30f), textureBox);
            boxMesh.updateValues();

            boxB = CreateStaticBox(40, new Vector3(100, 40, 0));
            dynamicsWorld.AddRigidBody(boxB);
            boxMeshB = TGCBox.fromSize(new TGCVector3(80f, 80f, 80f), textureBox);
            boxMeshB.updateValues();

            director = new TGCVector3(0, 0, 1);
        }

        public void Update(TgcD3dInput input)
        {
            dynamicsWorld.StepSimulation(1 / 60f, 100);
            var strenght = 2.30f;
            var angle = 5;
            
            /*
            if (input.keyDown(Key.W))
            {
                //Activa el comportamiento de la simulacion fisica
                capsule.ActivationState = ActivationState.ActiveTag;
                capsule.ApplyCentralImpulse(new TGCVector3(strenght, 0, 0).ToBsVector);
            }

            if (input.keyDown(Key.S))
            {
                //Activa el comportamiento de la simulacion fisica
                capsule.ActivationState = ActivationState.ActiveTag;
                capsule.ApplyCentralImpulse( new TGCVector3(-strenght, 0, 0).ToBsVector);
            }
            
            if (input.keyDown(Key.A))
            {
                //Activa el comportamiento de la simulacion fisica
                capsule.ActivationState = ActivationState.ActiveTag;
                capsule.ApplyCentralImpulse(new TGCVector3(0, 0, strenght).ToBsVector);
            }

            if (input.keyDown(Key.D))
            {
                //Activa el comportamiento de la simulacion fisica
                capsule.ActivationState = ActivationState.ActiveTag;
                capsule.ApplyCentralImpulse(new TGCVector3(0, 0, -strenght).ToBsVector);
            }
            
            if (input.keyDown(Key.Space))
            {
                //Activa el comportamiento de la simulacion fisica
                capsule.ActivationState = ActivationState.ActiveTag;
                capsule.ApplyCentralImpulse(new TGCVector3(0,strenght,0).ToBsVector);
            }*/
            
            if (input.keyDown(Key.W))
            {
                //Activa el comportamiento de la simulacion fisica
                capsuleRigidBody.ActivationState = ActivationState.ActiveTag;
                capsuleRigidBody.AngularVelocity = TGCVector3.Empty.ToBsVector;
                capsuleRigidBody.ApplyImpulse(-strenght * director.ToBsVector, new TGCVector3( capsuleRigidBody.CenterOfMassPosition.X, capsuleRigidBody.CenterOfMassPosition.Y - 25, capsuleRigidBody.CenterOfMassPosition.Z).ToBsVector);
                //capsuleRigidBody.LinearVelocity = (new TGCVector3(strenght, 0, 0).ToBsVector);
            }

            if (input.keyDown(Key.S))
            {
                //Activa el comportamiento de la simulacion fisica
                capsuleRigidBody.ActivationState = ActivationState.ActiveTag;
                capsuleRigidBody.AngularVelocity = TGCVector3.Empty.ToBsVector;
                capsuleRigidBody.ApplyImpulse(new TGCVector3(0, 0, strenght).ToBsVector, new TGCVector3(capsuleRigidBody.CenterOfMassPosition.X, capsuleRigidBody.CenterOfMassPosition.Y - 25, capsuleRigidBody.CenterOfMassPosition.Z).ToBsVector);
                //capsuleRigidBody.LinearVelocity = (new TGCVector3(-strenght, 0, 0).ToBsVector);
            }

            if (input.keyDown(Key.A))
            {
                //Activa el comportamiento de la simulacion fisica
                capsuleRigidBody.ActivationState = ActivationState.ActiveTag;
                capsuleRigidBody.ApplyImpulse(new TGCVector3(0, 0, strenght).ToBsVector, new TGCVector3(capsuleRigidBody.CenterOfMassPosition.X + 10 , capsuleRigidBody.CenterOfMassPosition.Y - 25, capsuleRigidBody.CenterOfMassPosition.Z).ToBsVector);
                //capsuleRigidBody.LinearVelocity = (new TGCVector3(0, 0, strenght).ToBsVector);
            }

            if (input.keyDown(Key.D))
            {
                //Activa el comportamiento de la simulacion fisica
                capsuleRigidBody.ActivationState = ActivationState.ActiveTag;
                capsuleRigidBody.ApplyCentralImpulse(new TGCVector3(0, 0, -strenght).ToBsVector);
                //capsuleRigidBody.LinearVelocity = (new TGCVector3(0, 0, -strenght).ToBsVector);
            }

            if (input.keyDown(Key.Space))
            {
                //Activa el comportamiento de la simulacion fisica
                capsuleRigidBody.ActivationState = ActivationState.ActiveTag;
                capsuleRigidBody.ApplyCentralImpulse(new TGCVector3(0, strenght, 0).ToBsVector);
                //capsuleRigidBody.LinearVelocity = (new TGCVector3(0, strenght, 0).ToBsVector);
            }
        }

        public void Render()
        {
            //A cada mesh hay que aplicarle la matriz de world de interpolacion que se obtiene luego del frame de simulacion
            //junto con cada transformacion que necesitemos.
            sphereMesh.Transform = TGCMatrix.Scaling(10, 10, 10) * new TGCMatrix(capsule.InterpolationWorldTransform);
            sphereMesh.Render();

            personaje.Transform = TGCMatrix.Scaling(0.5f, 0.5f, 0.5f) * new TGCMatrix(capsuleRigidBody.InterpolationWorldTransform) * TGCMatrix.Translation(new TGCVector3(0,-35,0));
            personaje.Render();

            boxMesh.Transform = new TGCMatrix(box.InterpolationWorldTransform);
            boxMesh.Render();

            boxMeshB.Transform = new TGCMatrix(boxB.InterpolationWorldTransform);
            boxMeshB.Render();
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
            ballBody.Restitution = 1f;
            return ballBody;
        }

        /// <summary>
        /// Crea un box sin masa, por lo tanto sin inercia que actua como elemnto estatico
        /// </summary>
        /// <param name="size"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public RigidBody CreateStaticBox(float size, Vector3 position)
        {
            CollisionShape boxShape = new BoxShape(size, size, size);
            Matrix boxMatrix = Matrix.Identity;
            boxMatrix.Origin = position;

            var boxMotionState = new DefaultMotionState(boxMatrix);
            var boxRigidBodyInfo = new RigidBodyConstructionInfo(0, boxMotionState, boxShape);

            var boxBody = new RigidBody(boxRigidBodyInfo);
            
            return boxBody;
        }

        public RigidBody CreateBox(float size, float mass, float x, float y, float z, float yaw, float pitch, float roll)
        {
            //Se crea una caja de tamaño 20 con rotaciones y origien en 10,100,10 y 1kg de masa.
            var boxShape = new BoxShape(size, size, size);
            var boxTransform = TGCMatrix.RotationYawPitchRoll(yaw, pitch, roll).ToBsMatrix;
            boxTransform.Origin = new TGCVector3(x, y, z).ToBsVector;
            DefaultMotionState boxMotionState = new DefaultMotionState(boxTransform);
            //Es importante calcular la inercia caso contrario el objeto no rotara.
            var boxLocalInertia = boxShape.CalculateLocalInertia(mass);
            var boxInfo = new RigidBodyConstructionInfo(1f, boxMotionState, boxShape, boxLocalInertia);
            var boxBody = new RigidBody(boxInfo);
            boxBody.LinearFactor = TGCVector3.One.ToBsVector;
            //boxBody.SetDamping(0.7f, 0.9f);
            //boxBody.Restitution = 1f;
            boxBody.Friction = 0f;
            return boxBody;
        }

    }
    }

