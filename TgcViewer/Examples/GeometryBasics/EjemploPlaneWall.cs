using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.GeometryBasics
{
    /// <summary>
    /// Ejemplo EjemploPlaneWall.
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    /// 
    /// Muestra como utilizar la herramienta TgcPlaneWall para crear
    /// paredes planas con textura.
    /// Permite editar su posición, tamaño, textura y mapeo de textura.
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploPlaneWall : TgcExample
    {
        TgcPlaneWall wall;
        TgcTexture currentTexture;

        public override string getCategory()
        {
            return "GeometryBasics";
        }

        public override string getName()
        {
            return "PlaneWall";
        }

        public override string getDescription()
        {
            return "Muestra como utilizar la herramienta TgcPlaneWall para crear paredes planas con textura." +
                "Permite editar su posición, tamaño, textura y mapeo de textura.";
        }

        

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Modifiers para variar parámetros de la pared
            GuiController.Instance.Modifiers.addVertex3f("origin", new Vector3(-100, -100, -100), new Vector3(100, 100, 100), new Vector3(0, 0, 0));
            GuiController.Instance.Modifiers.addVertex3f("dimension", new Vector3(-100, -100, -100), new Vector3(1000, 1000, 100), new Vector3(100, 100, 100));
            GuiController.Instance.Modifiers.addInterval("orientation", new string[] { "XY", "XZ", "YZ" }, 0);
            GuiController.Instance.Modifiers.addVertex2f("tiling", new Vector2(0,0), new Vector2(10,10),new Vector2(1,1));
            GuiController.Instance.Modifiers.addBoolean("autoAdjust", "autoAdjust", false);
            
            //Modifier de textura
            string texturePath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\TexturePack2\\brick1_1.jpg";
            currentTexture = TgcTexture.createTexture(d3dDevice, texturePath);
            GuiController.Instance.Modifiers.addTexture("texture", currentTexture.FilePath);

            //Crear pared
            wall = new TgcPlaneWall();
            wall.setTexture(currentTexture);

            //Actualizar según valores cargados
            updateWall();
        
        }

        /// <summary>
        /// Actualizar parámetros de la pared según los valores cargados
        /// </summary>
        private void updateWall()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Origen, dimensiones, tiling y AutoAdjust
            Vector3 origin = (Vector3)GuiController.Instance.Modifiers["origin"];
            Vector3 dimension = (Vector3)GuiController.Instance.Modifiers["dimension"];
            Vector2 tiling = (Vector2)GuiController.Instance.Modifiers["tiling"];
            bool autoAdjust = (bool)GuiController.Instance.Modifiers["autoAdjust"];

            //Cambiar orienación
            string orientation = (string)GuiController.Instance.Modifiers["orientation"];
            TgcPlaneWall.Orientations or;
            if(orientation == "XY") or = TgcPlaneWall.Orientations.XYplane;
            else if(orientation == "XZ") or = TgcPlaneWall.Orientations.XZplane;
            else or = TgcPlaneWall.Orientations.YZplane;

            //Cambiar textura
            string text = (string)GuiController.Instance.Modifiers["texture"];
            if (text != currentTexture.FilePath)
            {
                currentTexture = TgcTexture.createTexture(d3dDevice, text);
                wall.setTexture(currentTexture);
            }

            //Aplicar valores en pared
            wall.Origin = origin;
            wall.Size = dimension;
            wall.Orientation = or;
            wall.AutoAdjustUv = autoAdjust;
            wall.UTile = tiling.X;
            wall.VTile = tiling.Y;

            //Es necesario ejecutar updateValues() para que los cambios tomen efecto
            wall.updateValues();


            //Ajustar camara segun tamaño de la pared
            GuiController.Instance.RotCamera.targetObject(wall.BoundingBox);
        }

        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Actualizar valrores de pared
            updateWall();

            //Renderizar pared
            wall.render();
        }

        public override void close()
        {
            wall.dispose();
        }

    }
}
