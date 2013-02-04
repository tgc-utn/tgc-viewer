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

namespace Examples.AlphaBlending
{
    /// <summary>
    /// Ejemplo EjemploModeloAlpha:
    /// Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Alpha Blending
    ///     # Unidad 3 - Conceptos Básicos de 3D - Alpha Test
    /// 
    /// Carga una escena que posee modelos exportados desde 3Ds MAX con Alpha Blending activado.
    /// Estos modelos poseen texturas PNG-32 con transparencia.
    /// Los modelos fueron configurados en 3Ds MAX con un mapa de "Opacity" en el "Material Editor"
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploModeloAlpha : TgcExample
    {

        TgcScene scene;

        public override string getCategory()
        {
            return "AlphaBlending";
        }

        public override string getName()
        {
            return "Modelo con Alpha";
        }

        public override string getDescription()
        {
            return "Carga modelos que poseen texturas PNG-32 con transparencia.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            /* Cargar ecena que tiene un modelo configurado con AlphaBlending
             * Los modelos fueron exportados en 3Ds MAX con el mapa "Opacity" cargado en el "Material Editor"
             * Entonces el TgcSceneLoader automáticamente hace mesh.AlphaBlendEnable(true);
             */
            TgcSceneLoader loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vegetacion\\Pino\\Pino-TgcScene.xml");


            GuiController.Instance.RotCamera.targetObject(scene.BoundingBox);
        }


        public override void render(float elapsedTime)
        {
            scene.renderAll();
        }

        public override void close()
        {
            scene.disposeAll();
        }

    }
}
