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

            //Cargar modelo estatico
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(
                GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\Buggy\\Buggy-TgcScene.xml");
            mesh = scene.Meshes[0];


            //Alejar camara rotacional segun tamaño del BoundingBox del objeto
            GuiController.Instance.RotCamera.targetObject(mesh.BoundingBox);
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
