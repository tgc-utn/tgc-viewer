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

        //Capsula y Personaje
        private RigidBody capsuleRigidBody;
        private TgcSkeletalMesh personaje;
        private TGCVector3 director;

        //Pokebola
        private RigidBody pokeball;
        private TGCSphere sphereMesh;
        
        //Cajas varias
        private RigidBody box;
        private RigidBody boxB;
        private TGCBox boxMesh;
        private TGCBox boxMeshB;

        //Escaleras
        private TGCBox escalon;
        private List<RigidBody> escalonesRigidBodies = new List<RigidBody>();
        private RigidBody escalonRigidBody;

        public TGCVector3 getCharacterPosition()
        {
            return new TGCVector3(capsuleRigidBody.CenterOfMassPosition.X, capsuleRigidBody.CenterOfMassPosition.Y, capsuleRigidBody.CenterOfMassPosition.Z);
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

            //Capsula
            capsuleRigidBody = CreateCapsule(10,50, new TGCVector3(200, 500, 200));
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

            
            pokeball = CreateBall(10f, 0.5f, new TGCVector3(100f, 500f, 100f));
            dynamicsWorld.AddRigidBody(pokeball);

            var texturePokeball = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"Texturas\pokeball.jpg");
            var textureBox = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"MeshCreator\Textures\Madera\cajaMadera2.jpg");
            //Se crea una esfera de tamaño 1 para escalarla luego (en render)
            sphereMesh = new TGCSphere(1, texturePokeball, TGCVector3.Empty);
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
            var sizeBox = 20f;
            
            //Cajas
            box = CreateBox(new TGCVector3(sizeBox, sizeBox, sizeBox), 0, new TGCVector3(0, 12, 0), 0, 0, 0, 0.5f);
            dynamicsWorld.AddRigidBody(box);
            boxMesh = TGCBox.fromSize(new TGCVector3(30f, 30f, 30f), textureBox);
            boxMesh.updateValues();

            sizeBox = 40f;
            boxB = CreateBox(new TGCVector3(sizeBox,sizeBox,sizeBox), 0, new TGCVector3(100, 40, 0), 0, 0, 0, 0.5f);
            dynamicsWorld.AddRigidBody(boxB);
            boxMeshB = TGCBox.fromSize(new TGCVector3(80f, 80f, 80f), textureBox);
            boxMeshB.updateValues();

            //Escalera
            var a = 0;
            var textureStones = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"Texturas\stones.bmp");
            var size = new TGCVector3(50, 5, 10);
            escalon = TGCBox.fromSize(size, textureStones);
            /*
            while (a<10)
            {*/
                escalonRigidBody = CreateBox(size, 0, new TGCVector3(200, a * 5 + 30, 100), 0, 0, 0, 0.5f);
                //escalonesRigidBodies.Add(escalon);
                dynamicsWorld.AddRigidBody(escalonRigidBody);
            /*
                a++;
            }*/

            director = new TGCVector3(0, 0, 1);
        }

        public void Update(TgcD3dInput input)
        {
            dynamicsWorld.StepSimulation(1 / 60f, 100);
            var strenght = 1.30f;
            var angle = 5;
            var moving = false;

            if (input.keyDown(Key.W))
            {
                moving = true;
                //Activa el comportamiento de la simulacion fisica para la capsula
                capsuleRigidBody.ActivationState = ActivationState.ActiveTag;
                capsuleRigidBody.AngularVelocity = TGCVector3.Empty.ToBsVector;
                capsuleRigidBody.ApplyImpulse(-strenght * director.ToBsVector, new TGCVector3( capsuleRigidBody.CenterOfMassPosition.X, capsuleRigidBody.CenterOfMassPosition.Y - 25, capsuleRigidBody.CenterOfMassPosition.Z).ToBsVector);
            }

            if (input.keyDown(Key.S))
            {
                moving = true;
                //Activa el comportamiento de la simulacion fisica para la capsula
                capsuleRigidBody.ActivationState = ActivationState.ActiveTag;
                capsuleRigidBody.AngularVelocity = TGCVector3.Empty.ToBsVector;
                capsuleRigidBody.ApplyImpulse(strenght * director.ToBsVector, new TGCVector3(capsuleRigidBody.CenterOfMassPosition.X, capsuleRigidBody.CenterOfMassPosition.Y - 25, capsuleRigidBody.CenterOfMassPosition.Z).ToBsVector);
            }

            if (input.keyDown(Key.A))
            {
                director.TransformCoordinate(TGCMatrix.RotationY(-angle * 0.01f));
                personaje.Transform = TGCMatrix.Translation(TGCVector3.Empty) * TGCMatrix.RotationY(-angle * 0.01f) * new TGCMatrix(capsuleRigidBody.InterpolationWorldTransform);
                capsuleRigidBody.WorldTransform = personaje.Transform.ToBsMatrix;
            }

            if (input.keyDown(Key.D))
            {
                director.TransformCoordinate(TGCMatrix.RotationY(angle * 0.01f));
                personaje.Transform = TGCMatrix.Translation(TGCVector3.Empty) * TGCMatrix.RotationY(angle * 0.01f) * new TGCMatrix(capsuleRigidBody.InterpolationWorldTransform);
                capsuleRigidBody.WorldTransform = personaje.Transform.ToBsMatrix;
            }

            if (input.keyPressed(Key.Space))
            {
                //Activa el comportamiento de la simulacion fisica para la capsula
                capsuleRigidBody.ActivationState = ActivationState.ActiveTag;
                capsuleRigidBody.ApplyCentralImpulse(new TGCVector3(0, 80 * strenght, 0).ToBsVector);
            }

            if (moving)
            {
                personaje.playAnimation("Caminando", true);
            }
            else
            {
                personaje.playAnimation("Parado", true);
            }
        }

        public void Render(float elapsedTime)
        {
            //A cada mesh hay que aplicarle la matriz de interpolacion de world que se obtiene tras cada frame de simulacion
            //junto con cada transformacion que necesitemos.
            sphereMesh.Transform = TGCMatrix.Scaling(10, 10, 10) * new TGCMatrix(pokeball.InterpolationWorldTransform);
            sphereMesh.Render();

            personaje.Transform = TGCMatrix.Scaling(0.5f, 0.5f, 0.5f) * new TGCMatrix(capsuleRigidBody.InterpolationWorldTransform) * TGCMatrix.Translation(new TGCVector3(0,-35,0));
            personaje.animateAndRender(elapsedTime);

            boxMesh.Transform = new TGCMatrix(box.InterpolationWorldTransform);
            boxMesh.Render();

            boxMeshB.Transform = new TGCMatrix(boxB.InterpolationWorldTransform);
            boxMeshB.Render();
            /*
            foreach(RigidBody peldanio in escalonesRigidBodies)
            {*/
                escalon.Transform = new TGCMatrix(escalonRigidBody.InterpolationWorldTransform);
                escalon.Render();
            //}
        }

        public void Dispose()
        {
            //Dispose de los meshes.
            sphereMesh.Dispose();
            personaje.Dispose();
            boxMesh.Dispose();
            boxMeshB.Dispose();

            //Se hace dispose del modelo fisico. 
            dynamicsWorld.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            overlappingPairCache.Dispose();
        }

        /// <summary>
        ///     Se crea una esfera a partir de un radio, masa y posicion devolviendo el cuerpo rigido de una esfera.
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="mass"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public RigidBody CreateBall(float radius, float mass, TGCVector3 position)
        {
            //Creamos la forma de la esfera a partir de un radio
            var ballShape = new SphereShape(radius);

            //Armamos las matrices de transformacion de la esfera a partir de la posicion con la que queremos ubicarla
            var ballTransform = TGCMatrix.Identity;
            ballTransform.Origin = position;
            var ballMotionState = new DefaultMotionState(ballTransform.ToBsMatrix);

            //Se calcula el momento de inercia de la esfera a partir de la masa.
            var ballLocalInertia = ballShape.CalculateLocalInertia(mass);
            var ballInfo = new RigidBodyConstructionInfo(mass, ballMotionState, ballShape, ballLocalInertia);

            //Creamos el cuerpo rigido de la esfera a partir de la info.
            var ballBody = new RigidBody(ballInfo);
            ballBody.LinearFactor = TGCVector3.One.ToBsVector;
            ballBody.SetDamping(0.1f, 0.5f);
            ballBody.Restitution = 1f;
            return ballBody;
        }

        /// <summary>
        ///  Se crea una caja con una masa (si se quiere que sea estatica la masa debe ser 0),
        ///  con dimensiones x(ancho) ,y(alto) ,z(profundidad), Rotacion de ejes Yaw, Pitch, Roll y un coeficiente de rozamiento. 
        /// </summary>
        /// <param name="size"></param>
        /// <param name="mass"></param>
        /// <param name="position"></param>
        /// <param name="yaw"></param>
        /// <param name="pitch"></param>
        /// <param name="roll"></param>
        /// <param name="friction"></param>
        /// <returns></returns>
        public RigidBody CreateBox(TGCVector3 size, float mass, TGCVector3 position, float yaw, float pitch, float roll, float friction)
        {
            //Se crea una caja de tamaño 20 con rotaciones y origien en 10,100,10 y 1kg de masa.
            var boxShape = new BoxShape(size.X, size.Y, size.Z);
            var boxTransform = TGCMatrix.RotationYawPitchRoll(yaw, pitch, roll).ToBsMatrix;
            boxTransform.Origin = position.ToBsVector;
            DefaultMotionState boxMotionState = new DefaultMotionState(boxTransform);
            //Es importante calcular la inercia caso contrario el objeto no rotara.
            //Si se quiere que no rote el objeto hay que considerar la masa 0Kg
            var boxLocalInertia = boxShape.CalculateLocalInertia(mass);
            var boxInfo = new RigidBodyConstructionInfo(mass, boxMotionState, boxShape, boxLocalInertia);
            var boxBody = new RigidBody(boxInfo);
            boxBody.LinearFactor = TGCVector3.One.ToBsVector;
            //boxBody.SetDamping(0.7f, 0.9f);
            //boxBody.Restitution = 1f;
            boxBody.Friction = friction;
            return boxBody;
        }

        /// <summary>
        ///  Se crea una capsula a partir de un radio, una altura y una posicion.
        ///  Los valores de la masa y el calculo de inercia asociado estan fijos para que no haya comportamiento erratico.
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="height"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public RigidBody CreateCapsule(float radius, float height, TGCVector3 position)
        {
            //Creamos el shape de la Capsula a partir de un radio y una altura.
            var caspsuleShape = new CapsuleShape(radius, height);

            //Armamos las transformaciones que luego formaran parte del cuerpo rigido de la capsula.
            var capsuleTransform = TGCMatrix.Identity;
            capsuleTransform.Origin = position;
            var capsuleMotionState = new DefaultMotionState(capsuleTransform.ToBsMatrix);

            // Utilizamos una masa muy grande (1000 Kg) para calcular el momento de inercia de forma que la capsula no
            // genere una rotacion y termine volcando.
            var capsuleInertia = caspsuleShape.CalculateLocalInertia(1000);

            // Aqui usamos una masa bastante baja (1 Kg) para que cuando se arme el cuerpo rigido y se intente aplicar 
            // un impulso se facil de mover la capsula.
            var capsuleRigidBodyInfo = new RigidBodyConstructionInfo(1, capsuleMotionState, caspsuleShape, capsuleInertia);

            var localCapsuleRigidBody = new RigidBody(capsuleRigidBodyInfo);
            localCapsuleRigidBody.LinearFactor = TGCVector3.One.ToBsVector;
            localCapsuleRigidBody.SetDamping(0.1f, 0f);
            localCapsuleRigidBody.Restitution = 0.1f;
            localCapsuleRigidBody.Friction = 1;

            return localCapsuleRigidBody;
        }

    }
    }

