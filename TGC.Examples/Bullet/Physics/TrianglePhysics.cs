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
        private RigidBody rigidBodyPlataforma;
        private TGCBox plataforma;

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
            //Creamos el mundo fisico por defecto.
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            constraintSolver = new SequentialImpulseConstraintSolver();
            overlappingPairCache = new DbvtBroadphase(); //AxisSweep3(new BsVector3(-5000f, -5000f, -5000f), new BsVector3(5000f, 5000f, 5000f), 8192);
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, overlappingPairCache, constraintSolver, collisionConfiguration);
            dynamicsWorld.Gravity = new TGCVector3(0, -100f, 0).ToBsVector;

            //Capsula
            capsuleRigidBody = Core.BulletPhysics.BulletRigidBodyConstructor.CreateCapsule(10,50, new TGCVector3(200, 500, 200));
            dynamicsWorld.AddRigidBody(capsuleRigidBody);

            //Creamos el terreno
            var meshRigidBody = Core.BulletPhysics.BulletRigidBodyConstructor.CreateSurfaceFromHeighMap(triangleDataVB);
            dynamicsWorld.AddRigidBody(meshRigidBody);

            //Creamos una esfera para interactuar
            pokeball = Core.BulletPhysics.BulletRigidBodyConstructor.CreateBall(10f, 0.5f, new TGCVector3(100f, 500f, 100f));
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
            box = Core.BulletPhysics.BulletRigidBodyConstructor.CreateBox(new TGCVector3(sizeBox, sizeBox, sizeBox), 0, new TGCVector3(0, 12, 0), 0, 0, 0, 0.5f);
            dynamicsWorld.AddRigidBody(box);
            boxMesh = TGCBox.fromSize(new TGCVector3(30f, 30f, 30f), textureBox);
            boxMesh.updateValues();

            sizeBox = 40f;
            boxB = Core.BulletPhysics.BulletRigidBodyConstructor.CreateBox(new TGCVector3(sizeBox,sizeBox,sizeBox), 0, new TGCVector3(100, 40, 0), 0, 0, 0, 0.5f);
            dynamicsWorld.AddRigidBody(boxB);
            boxMeshB = TGCBox.fromSize(new TGCVector3(80f, 80f, 80f), textureBox);
            boxMeshB.updateValues();

            //Escalera
            var a = 0;
            var textureStones = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"Texturas\stones.bmp");
            //la altura de cualquier cubo que quiera subir una capsula debe ser menor o igual a la mitad del radio

            var size = new TGCVector3(50, 4, 20);
            escalon = TGCBox.fromSize(size, textureStones);
            
            while (a<10)
            {
                escalonRigidBody = Core.BulletPhysics.BulletRigidBodyConstructor.CreateBox(size, 0, new TGCVector3(200, a * 4 + 10, a * 20 + 100), 0, 0, 0, 0.1f);
                escalonesRigidBodies.Add(escalonRigidBody);
                dynamicsWorld.AddRigidBody(escalonRigidBody);
            
                a++;
            }

            //Plataforma
            textureStones = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"Texturas\cobblestone_quad.jpg");
            rigidBodyPlataforma = Core.BulletPhysics.BulletRigidBodyConstructor.CreateBox(new TGCVector3(50f, 15f, 50f), 0, new TGCVector3(200, 42.5f, 315), 0, 0, 0, 0.5f);
            dynamicsWorld.AddRigidBody(rigidBodyPlataforma);
            plataforma = TGCBox.fromSize(new TGCVector3(50f, 15f, 50f), textureStones);
            plataforma.updateValues();

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
            
            foreach(RigidBody peldanio in escalonesRigidBodies)
            {
                escalon.Transform = new TGCMatrix(peldanio.InterpolationWorldTransform);
                escalon.Render();
            }

            plataforma.Transform = new TGCMatrix(rigidBodyPlataforma.InterpolationWorldTransform);
            plataforma.Render();
        }

        public void Dispose()
        {
            //Dispose de los meshes.
            sphereMesh.Dispose();
            personaje.Dispose();
            boxMesh.Dispose();
            boxMeshB.Dispose();
            plataforma.Dispose();
            foreach (RigidBody peldanio in escalonesRigidBodies)
            {
                escalon.Dispose();
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