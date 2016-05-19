using Microsoft.DirectX;
using System;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Terrain;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.Outdoor
{
    /// <summary>
    ///     Ejemplo EjemploSimpleTerrain:
    ///     Unidades Involucradas:
    ///     # Unidad 7 - Técnicas de Optimización - Heightmap
    ///     Muestra como crear un terreno en base a una textura de Heightmap y
    ///     le aplica arriba una textura de color (DiffuseMap).
    ///     Se utiliza la herramienta TgcSimpleTerrain
    ///     Posee modifiers para variar la textura de Heightmap, el DiffuseMap y el vector de escala del terreno.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploSimpleTerrain : TgcExample
    {
        private string currentHeightmap;
        private float currentScaleXZ;
        private float currentScaleY;
        private string currentTexture;
        private TgcSimpleTerrain terrain;

        public EjemploSimpleTerrain(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Outdoor";
            Name = "SimpleTerrain";
            Description =
                "Muestra como crear un terreno en base a una textura de HeightMap utilizando la herramietna del Framework TgcSimpleTerrain.";
        }

        public override void Init()
        {
            //Path de Heightmap default del terreno y Modifier para cambiarla
            currentHeightmap = MediaDir + "Heighmaps\\" + "Heightmap2.jpg";
            Modifiers.addTexture("heightmap", currentHeightmap);

            //Modifiers para variar escala del mapa
            currentScaleXZ = 20f;
            Modifiers.addFloat("scaleXZ", 0.1f, 100f, currentScaleXZ);
            currentScaleY = 1.3f;
            Modifiers.addFloat("scaleY", 0.1f, 10f, currentScaleY);

            //Path de Textura default del terreno y Modifier para cambiarla
            currentTexture = MediaDir + "Heighmaps\\" + "TerrainTexture2.jpg";
            Modifiers.addTexture("texture", currentTexture);

            //Cargar terreno: cargar heightmap y textura de color
            terrain = new TgcSimpleTerrain();
            terrain.loadHeightmap(currentHeightmap, currentScaleXZ, currentScaleY, new Vector3(0, 0, 0));
            terrain.loadTexture(currentTexture);

            //Configurar FPS Camara
            Camara = new TgcFpsCamera();
            ((TgcFpsCamera)Camara).MovementSpeed = 100f;
            ((TgcFpsCamera)Camara).JumpSpeed = 100f;
            Camara.setCamera(new Vector3(-722.6171f, 495.0046f, -31.2611f), new Vector3(164.9481f, 35.3185f, -61.5394f));
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            //Ver si cambio el heightmap
            var selectedHeightmap = (string)Modifiers["heightmap"];
            if (currentHeightmap != selectedHeightmap)
            {
                //Volver a cargar el Heightmap
                currentHeightmap = selectedHeightmap;
                terrain.loadHeightmap(currentHeightmap, currentScaleXZ, currentScaleY, new Vector3(0, 0, 0));
            }

            //Ver si cambio alguno de los valores de escala
            var selectedScaleXZ = (float)Modifiers["scaleXZ"];
            var selectedScaleY = (float)Modifiers["scaleY"];
            if (currentScaleXZ != selectedScaleXZ || currentScaleY != selectedScaleY)
            {
                //Volver a cargar el Heightmap
                currentScaleXZ = selectedScaleXZ;
                currentScaleY = selectedScaleY;
                terrain.loadHeightmap(currentHeightmap, currentScaleXZ, currentScaleY, new Vector3(0, 0, 0));
            }

            //Ver si cambio la textura del terreno
            var selectedTexture = (string)Modifiers["texture"];
            if (currentTexture != selectedTexture)
            {
                //Volver a cargar el DiffuseMap
                currentTexture = selectedTexture;
                terrain.loadTexture(currentTexture);
            }

            //Renderizar terreno
            terrain.render();

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            terrain.dispose();
        }
    }
}