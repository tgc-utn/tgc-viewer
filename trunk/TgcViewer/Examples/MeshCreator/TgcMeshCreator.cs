using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using System.Windows.Forms;
using TgcViewer.Utils.Terrain;
using System.Xml;
using System.Globalization;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Input;

namespace Examples.MeshCreator
{
    /// <summary>
    /// Ejemplo TgcMeshCreator:
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh, Transformaciones, GameEngine
    ///     # Unidad 6 - Detección de Colisiones - BoundingBox, Picking
    /// 
    /// Herramienta para crear modelos 3D en base a primitivas simples.
    /// Permite luego exportarlos a un un XML de formato TgcScene.
    /// El ejemplo crea su propio Modifier con todos los controles visuales de .NET que necesita.
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class TgcMeshCreator : TgcExample
    {
        MeshCreatorModifier modifier;

        public override string getCategory()
        {
            return "Utils";
        }

        public override string getName()
        {
            return "MeshCreator";
        }

        public override string getDescription()
        {
            return "Creador de objetos 3D a paritir de primitivas simples." +
                "Luego se puede exportar a un archivo XML de formato TgcScee para su posterior uso.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Configurar camara FPS
            GuiController.Instance.RotCamera.Enable = false;

            //Color de fondo
            GuiController.Instance.BackgroundColor = Color.FromArgb(38, 38, 38);

            //Crear modifier especial para este editor
            modifier = new MeshCreatorModifier("TgcMeshCreator", this);
            GuiController.Instance.Modifiers.add(modifier);
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Delegar render al control
            modifier.Control.render();
        }

        public override void close()
        {
            //Delegar al control
            modifier.dispose();
            modifier.Control.close();
        }

        
    }
}
