using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.Collision
{
    /// <summary>
    /// Ejemplo EjemploBoundingBox:
    /// Unidades Involucradas:
    ///     # Unidad 6 - Detección de Colisiones - BoundingBox
    /// 
    /// Carga un modelo 3D estático mediante la herramienta TgcSceneLoader
    /// y muestra como renderizar su BoundingBox.
    /// 
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploBoundingBox : TgcExample
    {
        TgcMesh mesh;


        public override string getCategory()
        {
            return "Collision";
        }

        public override string getName()
        {
            return "BoundingBox";
        }

        public override string getDescription()
        {
            return "Carga un modelo 3D estático mediante la herramienta TgcSceneLoader y muestra como renderizar su BoundingBox.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Cargar modelo estatico Box
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(
                GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\" + "Box-TgcScene.xml",
                GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\");
            mesh = scene.Meshes[0];

            //Desplazarlo
            mesh.move(0,10,0);

            //Alejar camara rotacional
            GuiController.Instance.RotCamera.CameraDistance = 100f;
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Renderizar modelo
            mesh.render();

            //Renderizar BoundingBox
            mesh.BoundingBox.render();
        }

        public override void close()
        {
            mesh.dispose();
        }

    }
}
