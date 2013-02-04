using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.Terrain;

namespace Examples.Outdoor
{

    /// <summary>
    /// Ejemplo EjemploSimpleTerrain:
    /// Unidades Involucradas:
    ///     # Unidad 7 - T�cnicas de Optimizaci�n - Heightmap
	///
    /// Muestra como crear un terreno en base a una textura de Heightmap y
    /// le aplica arriba una textura de color (DiffuseMap).
    /// Se utiliza la herramienta TgcSimpleTerrain
    /// Posee modifiers para variar la textura de Heightmap, el DiffuseMap y el vector de escala del terreno.
    /// 
    /// Autor: Mat�as Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploSimpleTerrain : TgcExample
    {

        TgcSimpleTerrain terrain;
        string currentHeightmap;
        string currentTexture;
        float currentScaleXZ;
        float currentScaleY;


        public override string getCategory()
        {
            return "Outdor";
        }

        public override string getName()
        {
            return "SimpleTerrain";
        }

        public override string getDescription()
        {
            return "Muestra como crear un terreno en base a una textura de HeightMap utilizando la herramietna del Framework TgcSimpleTerrain";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Path de Heightmap default del terreno y Modifier para cambiarla
            currentHeightmap = GuiController.Instance.ExamplesMediaDir + "Heighmaps\\" + "Heightmap2.jpg";
            GuiController.Instance.Modifiers.addTexture("heightmap", currentHeightmap);
            
            //Modifiers para variar escala del mapa
            currentScaleXZ = 20f;
            GuiController.Instance.Modifiers.addFloat("scaleXZ", 0.1f, 100f, currentScaleXZ);
            currentScaleY = 1.3f;
            GuiController.Instance.Modifiers.addFloat("scaleY", 0.1f, 10f, currentScaleY);

            //Path de Textura default del terreno y Modifier para cambiarla
            currentTexture = GuiController.Instance.ExamplesMediaDir + "Heighmaps\\" + "TerrainTexture2.jpg";
            GuiController.Instance.Modifiers.addTexture("texture", currentTexture);


            //Cargar terreno: cargar heightmap y textura de color
            terrain = new TgcSimpleTerrain();
            terrain.loadHeightmap(currentHeightmap, currentScaleXZ, currentScaleY, new Vector3(0,0,0));
            terrain.loadTexture(currentTexture);


            //Configurar FPS Camara
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.MovementSpeed = 100f;
            GuiController.Instance.FpsCamera.JumpSpeed = 100f;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-722.6171f, 495.0046f, -31.2611f), new Vector3(164.9481f, 35.3185f, -61.5394f));
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Ver si cambio el heightmap
            string selectedHeightmap = (string)GuiController.Instance.Modifiers["heightmap"];
            if (currentHeightmap != selectedHeightmap)
            {
                //Volver a cargar el Heightmap
                currentHeightmap = selectedHeightmap;
                terrain.loadHeightmap(currentHeightmap, currentScaleXZ, currentScaleY, new Vector3(0, 0, 0));
            }

            //Ver si cambio alguno de los valores de escala
            float selectedScaleXZ = (float)GuiController.Instance.Modifiers["scaleXZ"];
            float selectedScaleY = (float)GuiController.Instance.Modifiers["scaleY"];
            if (currentScaleXZ != selectedScaleXZ || currentScaleY != selectedScaleY)
            {
                //Volver a cargar el Heightmap
                currentScaleXZ = selectedScaleXZ;
                currentScaleY = selectedScaleY;
                terrain.loadHeightmap(currentHeightmap, currentScaleXZ, currentScaleY, new Vector3(0, 0, 0));
            }

            //Ver si cambio la textura del terreno
            string selectedTexture = (string)GuiController.Instance.Modifiers["texture"];
            if (currentTexture != selectedTexture)
            {
                //Volver a cargar el DiffuseMap
                currentTexture = selectedTexture;
                terrain.loadTexture(currentTexture);
            }


            //Renderizar terreno
            terrain.render();
        }

        public override void close()
        {
            terrain.dispose();
        }

    }
}
