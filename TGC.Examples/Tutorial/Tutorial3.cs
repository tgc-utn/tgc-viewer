﻿using Microsoft.DirectX.DirectInput;
using System.Windows.Forms;
using TGC.Core.Collision;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.Tutorial.Physics;
using TGC.Examples.UserControls;

namespace TGC.Examples.Tutorial
{
    /// <summary>
    ///     Tutorial 3:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Mesh
    /// 	# Unidad 6 - Detección de Colisiones - Bounding Box.
    ///     Muestra como cargar una escena 3D y como mover un modelo dentra de ella con el teclado evitando chocar con el resto de los objetos.
    ///     Autor: Matias Leone
    /// </summary>
    public class Tutorial3 : TGCExampleViewer
    {
        private const float MOVEMENT_SPEED = 200f;
        private TgcThirdPersonCamera camaraInterna;
        private TgcMesh mainMesh;
        private TgcScene scene;

        //Fisica
        private CubePhysic physicsExample;

        public Tutorial3(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Tutorial";
            Name = "Tutorial 3";
            Description = "Muestra como cargar una escena 3D y como mover un modelo dentra de ella con el teclado evitando chocar con el resto de los objetos.";
        }

        public override void Init()
        {
            //En este ejemplo primero cargamos una escena 3D entera.
            var loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Ciudad\\Ciudad-TgcScene.xml");
            /*
            //Luego cargamos otro modelo aparte que va a hacer el objeto que controlamos con el teclado
            var scene2 =
                loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\Hummer\\Hummer-TgcScene.xml");

            //Solo nos interesa el primer modelo de esta escena (tiene solo uno)
            mainMesh = scene2.Meshes[0];
            //Movemos el mesh un poco para arriba. Porque sino choca con el piso todo el tiempo y no se puede mover.
            mainMesh.Position = new TGCVector3(0, 50, 0);
            mainMesh.UpdateMeshTransform();*/

            physicsExample = new CubePhysic();
            //physicsExample.setHummer(mainMesh);
            scene.Meshes[0].Position = TGCVector3.Empty;
            physicsExample.setBuildings(scene.Meshes);
            physicsExample.Init(MediaDir);

            //Vamos a utilizar la camara en 3ra persona para que siga al objeto principal a medida que se mueve
            camaraInterna = new TgcThirdPersonCamera(physicsExample.getPositionHummer(), 500, 750);
            Camara = camaraInterna;

            UserVars.addVar("HummerPosition");
            UserVars.addVar("HummerBodyPosition");
        }

        public override void Update()
        {
            PreUpdate();

            physicsExample.Update(Input);
            UserVars.setValue("HummerPosition", physicsExample.getHummer().Position);
            UserVars.setValue("HummerBodyPosition", physicsExample.getBodyPos());

            camaraInterna.Target = physicsExample.getHummer().Position;

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            physicsExample.Render(ElapsedTime);

            PostRender();
        }

        public override void Dispose()
        {
            scene.DisposeAll();
            //mainMesh.Dispose();
            physicsExample.Dispose();
        }
    }
}