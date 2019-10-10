﻿using BulletSharp;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using TGC.Core.BulletPhysics;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.SkeletalAnimation;
using TGC.Core.Textures;

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

        //Datos del los triangulos del VertexBuffer
        private CustomVertex.PositionTextured[] triangleDataVB;

        //Capsula y Personaje
        private RigidBody capsuleRigidBody;

        private TgcSkeletalMesh personaje;
        private TGCVector3 director;

        //Pokebola
        private RigidBody pokeball;

        private TGCSphere sphereMesh;

        //Columna
        private RigidBody columnaRigidBody;

        private TgcMesh columnaMesh;

        //Cajas varias
        private RigidBody box;

        private RigidBody boxB;
        private RigidBody box45;
        private RigidBody boxPush;
        private TGCBox boxMesh;
        private TGCBox boxMeshB;
        private TGCBox boxMeshPush;
        private RigidBody rigidBodyPlataforma;
        private TGCBox plataforma;

        //Escaleras
        private TGCBox escalon;

        private List<RigidBody> escalonesRigidBodies = new List<RigidBody>();
        private RigidBody escalonRigidBody;

        public TGCVector3 GetCharacterPosition()
        {
            return new TGCVector3(capsuleRigidBody.CenterOfMassPosition.X, capsuleRigidBody.CenterOfMassPosition.Y, capsuleRigidBody.CenterOfMassPosition.Z);
        }

        public void SetTriangleDataVB(CustomVertex.PositionTextured[] newTriangleData)
        {
            triangleDataVB = newTriangleData;
        }

        public void Init(string MediaDir)
        {
            #region Configuracion Basica de World

            //Creamos el mundo fisico por defecto.
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            constraintSolver = new SequentialImpulseConstraintSolver();
            overlappingPairCache = new DbvtBroadphase(); //AxisSweep3(new BsVector3(-5000f, -5000f, -5000f), new BsVector3(5000f, 5000f, 5000f), 8192);
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, overlappingPairCache, constraintSolver, collisionConfiguration);
            dynamicsWorld.Gravity = new TGCVector3(0, -100f, 0).ToBulletVector3();

            #endregion Configuracion Basica de World

            #region Capsula

            //Cuerpo rigido de una capsula basica
            capsuleRigidBody = BulletRigidBodyFactory.Instance.CreateCapsule(10, 50, new TGCVector3(200, 500, 200), 10, false);

            //Valores que podemos modificar a partir del RigidBody base
            capsuleRigidBody.SetDamping(0.1f, 0f);
            capsuleRigidBody.Restitution = 0.1f;
            capsuleRigidBody.Friction = 1;

            //Agregamos el RidigBody al World
            dynamicsWorld.AddRigidBody(capsuleRigidBody);

            #endregion Capsula

            #region Terreno

            //Creamos el RigidBody basico del Terreno
            var meshRigidBody = BulletRigidBodyFactory.Instance.CreateSurfaceFromHeighMap(triangleDataVB);

            //Agregamos algo de friccion al RigidBody ya que este va a interactuar con objetos moviles
            //del World
            meshRigidBody.Friction = 0.5f;

            //Agregamos el RigidBody del terreno al World
            dynamicsWorld.AddRigidBody(meshRigidBody);

            #endregion Terreno

            #region Esfera

            //Creamos una esfera para interactuar
            pokeball = BulletRigidBodyFactory.Instance.CreateBall(10f, 0.5f, new TGCVector3(100f, 500f, 100f));
            pokeball.SetDamping(0.1f, 0.5f);
            pokeball.Restitution = 1f;
            //Agregamos la pokebola al World
            dynamicsWorld.AddRigidBody(pokeball);

            //Textura de pokebola
            var texturePokeball = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"Texturas\pokeball.jpg");

            //Se crea una esfera de tamaño 1 para escalarla luego (en render)
            sphereMesh = new TGCSphere(1, texturePokeball, TGCVector3.Empty);

            //Tgc no crea el vertex buffer hasta invocar a update values.
            sphereMesh.updateValues();

            #endregion Esfera

            #region Personaje

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

            #endregion Personaje

            #region Cajas

            var sizeBox = 20f;

            //Textura de caja
            var textureBox = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"MeshCreator\Textures\Madera\cajaMadera2.jpg");

            box = BulletRigidBodyFactory.Instance.CreateBox(new TGCVector3(sizeBox, sizeBox, sizeBox), 0, new TGCVector3(0, 12, 0), 0, 0, 0, 0.5f, true);
            dynamicsWorld.AddRigidBody(box);
            boxMesh = TGCBox.fromSize(new TGCVector3(40f, 40f, 40f), textureBox);
            boxMesh.updateValues();

            sizeBox = 40f;
            boxB = BulletRigidBodyFactory.Instance.CreateBox(new TGCVector3(sizeBox, sizeBox, sizeBox), 0, new TGCVector3(100, 40, 0), 0, 0, 0, 0.5f, true);
            dynamicsWorld.AddRigidBody(boxB);
            boxMeshB = TGCBox.fromSize(new TGCVector3(80f, 80f, 80f), textureBox);
            boxMeshB.updateValues();

            box45 = BulletRigidBodyFactory.Instance.CreateBox(new TGCVector3(sizeBox, sizeBox, sizeBox), 0, new TGCVector3(200, 40, 0), BulletSharp.MathUtil.SIMD_QUARTER_PI, 0, 0, 0.5f, true);
            dynamicsWorld.AddRigidBody(box45);

            boxPush = BulletRigidBodyFactory.Instance.CreateBox(new TGCVector3(sizeBox, sizeBox, sizeBox), 0.5f, new TGCVector3(-200, 60, 0), BulletSharp.MathUtil.SIMD_QUARTER_PI, 0, 0, 0.25f, true);
            dynamicsWorld.AddRigidBody(boxPush);

            boxMeshPush = TGCBox.fromSize(new TGCVector3(80f, 80f, 80f), textureBox);
            boxMeshPush.updateValues();

            #endregion Cajas

            #region Escalera

            var a = 0;
            var textureStones = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"Texturas\stones.bmp");

            //la altura de cualquier cubo que quiera subir una capsula debe ser menor a la mitad del radio
            var size = new TGCVector3(50, 4, 20);
            escalon = TGCBox.fromSize(size, textureStones);

            //Se crean 10 escalonescd d
            while (a < 10)
            {
                escalonRigidBody = BulletRigidBodyFactory.Instance.CreateBox(size, 0, new TGCVector3(200, a * 4 + 10, a * 20 + 100), 0, 0, 0, 0.1f, true);

                escalonesRigidBodies.Add(escalonRigidBody);

                dynamicsWorld.AddRigidBody(escalonRigidBody);

                a++;
            }

            #endregion Escalera

            #region Plataforma

            textureStones = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"Texturas\cobblestone_quad.jpg");
            rigidBodyPlataforma = BulletRigidBodyFactory.Instance.CreateBox(new TGCVector3(50f, 15f, 50f), 0, new TGCVector3(200, 42.5f, 315), 0, 0, 0, 0.5f, true);
            dynamicsWorld.AddRigidBody(rigidBodyPlataforma);
            plataforma = TGCBox.fromSize(new TGCVector3(50f, 15f, 50f), textureStones);
            plataforma.updateValues();

            #endregion Plataforma

            #region Columna

            columnaRigidBody = BulletRigidBodyFactory.Instance.CreateCylinder(new TGCVector3(10, 50, 10), new TGCVector3(100, 50, 100), 0);
            dynamicsWorld.AddRigidBody(columnaRigidBody);
            var columnaLoader = new TgcSceneLoader();
            columnaMesh = columnaLoader.loadSceneFromFile(MediaDir + @"MeshCreator\Meshes\Cimientos\PilarEgipcio\PilarEgipcio-TgcScene.xml", MediaDir + @"MeshCreator\Meshes\Cimientos\PilarEgipcio\").Meshes[0];
            columnaMesh.Position = new TGCVector3(100, 7.5f, 100);
            columnaMesh.UpdateMeshTransform();

            #endregion Columna

            director = new TGCVector3(0, 0, 1);
        }

        public void Update(TgcD3dInput input)
        {
            dynamicsWorld.StepSimulation(1 / 60f, 100);
            var strength = 10.30f;
            var angle = 5;
            var moving = false;

            #region Comoportamiento

            if (input.keyDown(Key.W))
            {
                moving = true;
                //Activa el comportamiento de la simulacion fisica para la capsula
                capsuleRigidBody.ActivationState = ActivationState.ActiveTag;
                capsuleRigidBody.AngularVelocity = TGCVector3.Empty.ToBulletVector3();
                capsuleRigidBody.ApplyCentralImpulse(-strength * director.ToBulletVector3());
            }

            if (input.keyDown(Key.S))
            {
                moving = true;
                //Activa el comportamiento de la simulacion fisica para la capsula
                capsuleRigidBody.ActivationState = ActivationState.ActiveTag;
                capsuleRigidBody.AngularVelocity = TGCVector3.Empty.ToBulletVector3();
                capsuleRigidBody.ApplyCentralImpulse(strength * director.ToBulletVector3());
            }

            if (input.keyDown(Key.A))
            {
                director.TransformCoordinate(TGCMatrix.RotationY(-angle * 0.01f));
                personaje.Transform = TGCMatrix.Translation(TGCVector3.Empty) * TGCMatrix.RotationY(-angle * 0.01f) * new TGCMatrix(capsuleRigidBody.InterpolationWorldTransform);
                capsuleRigidBody.WorldTransform = personaje.Transform.ToBulletMatrix();
            }

            if (input.keyDown(Key.D))
            {
                director.TransformCoordinate(TGCMatrix.RotationY(angle * 0.01f));
                personaje.Transform = TGCMatrix.Translation(TGCVector3.Empty) * TGCMatrix.RotationY(angle * 0.01f) * new TGCMatrix(capsuleRigidBody.InterpolationWorldTransform);
                capsuleRigidBody.WorldTransform = personaje.Transform.ToBulletMatrix();
            }

            if (input.keyPressed(Key.Space))
            {
                //Activa el comportamiento de la simulacion fisica para la capsula
                capsuleRigidBody.ActivationState = ActivationState.ActiveTag;
                capsuleRigidBody.ApplyCentralImpulse(new TGCVector3(0, 80 * strength, 0).ToBulletVector3());
            }

            #endregion Comoportamiento

            #region Animacion

            if (moving)
            {
                personaje.playAnimation("Caminando", true);
            }
            else
            {
                personaje.playAnimation("Parado", true);
            }

            #endregion Animacion
        }

        public void Render(float elapsedTime)
        {
            //A cada mesh hay que aplicarle la matriz de interpolacion de world que se obtiene tras cada frame de simulacion
            //junto con cada transformacion que necesitemos.
            sphereMesh.Transform = TGCMatrix.Scaling(10, 10, 10) * new TGCMatrix(pokeball.InterpolationWorldTransform);
            sphereMesh.Render();

            personaje.Transform = TGCMatrix.Scaling(0.5f, 0.5f, 0.5f) * new TGCMatrix(capsuleRigidBody.InterpolationWorldTransform) * TGCMatrix.Translation(new TGCVector3(0, -35, 0));
            personaje.animateAndRender(elapsedTime);

            boxMesh.Transform = new TGCMatrix(box.InterpolationWorldTransform);
            boxMesh.Render();

            boxMeshB.Transform = new TGCMatrix(boxB.InterpolationWorldTransform);
            boxMeshB.Render();

            boxMeshB.Transform = new TGCMatrix(box45.InterpolationWorldTransform);
            boxMeshB.Render();

            boxMeshB.Transform = new TGCMatrix(boxPush.InterpolationWorldTransform);
            boxMeshB.Render();

            foreach (RigidBody peldanio in escalonesRigidBodies)
            {
                escalon.Transform = new TGCMatrix(peldanio.InterpolationWorldTransform);
                escalon.Render();
            }

            columnaMesh.Transform = new TGCMatrix(columnaRigidBody.InterpolationWorldTransform);
            columnaMesh.Scale = new TGCVector3(1, 1.2f, 1);
            columnaMesh.UpdateMeshTransform();
            columnaMesh.Render();

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