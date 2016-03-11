using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Viewer;

namespace TGC.Examples.PortalRendering
{
    /// <summary>
    ///     Ejemplo PortalRenderingFramework
    ///     Unidades Involucradas:
    ///     # Unidad 7 - Optimización - Portal Rendering
    ///     Muestra como utilizar la técnica Portal Rendering para optimizar el renderizado de un escenario.
    ///     Utiliza la información de Celdas y Portales exportada por el plugin de 3Ds MAX.
    ///     Utiliza la clase TgcPortalRenderingManager de TgcScene para manejar todo el renderizado de esta técnica.
    ///     Esta clase posee una implementación básica de la estrategia de Portal Rendering. Hay muchos puntos pendientes
    ///     aún de mejorar y optimizar.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class PortalRenderingFramework : TgcExample
    {
        private TgcScene scene;

        public override string getCategory()
        {
            return "PortalRendering";
        }

        public override string getName()
        {
            return "PortalRendering Framework";
        }

        public override string getDescription()
        {
            return "Muestra como utilizar la técnica Portal Rendering para optimizar el renderizado de un escenario.";
        }

        public override void init()
        {
            //Cargar escenario con información especial exportada de PortalRendering
            var loader = new TgcSceneLoader();
            scene =
                loader.loadSceneFromFile(GuiController.Instance.ExamplesDir +
                                         "PortalRendering\\EscenarioPortal\\EscenarioPortal-TgcScene.xml");

            //Descactivar inicialmente a todos los modelos
            scene.setMeshesEnabled(false);

            //Camara en 1ra persona
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.MovementSpeed = 800f;
            GuiController.Instance.FpsCamera.JumpSpeed = 600f;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(0, 0, 0), new Vector3(0, 0, 1));

            //Modifiers
            GuiController.Instance.Modifiers.addBoolean("portalRendering", "PortalRendering", true);
            GuiController.Instance.Modifiers.addBoolean("WireFrame", "WireFrame", false);
            GuiController.Instance.Modifiers.addBoolean("showPortals", "Show Portals", false);

            //UserVars
            GuiController.Instance.UserVars.addVar("MeshCount");

            //Crear portales debug
            scene.PortalRendering.createDebugPortals(Color.Purple);
        }

        public override void render(float elapsedTime)
        {
            var enablePortalRendering = (bool)GuiController.Instance.Modifiers["portalRendering"];
            if (enablePortalRendering)
            {
                //Actualizar visibilidad con PortalRendering
                scene.PortalRendering.updateVisibility(CamaraManager.Instance.CurrentCamera.getPosition());
            }
            else
            {
                //Habilitar todo todo
                scene.setMeshesEnabled(true);
            }

            //WireFrame
            var wireFrameEnable = (bool)GuiController.Instance.Modifiers["WireFrame"];
            if (wireFrameEnable)
            {
                D3DDevice.Instance.Device.RenderState.FillMode = FillMode.WireFrame;
            }
            else
            {
                D3DDevice.Instance.Device.RenderState.FillMode = FillMode.Solid;
            }

            var meshCount = 0;
            //Renderizar modelos visibles, primero todos los modelos opacos (sin alpha)
            foreach (var mesh in scene.Meshes)
            {
                //Contador de modelos
                if (mesh.Enabled && !mesh.AlphaBlendEnable)
                {
                    meshCount++;
                }

                //Renderizar modelo y luego desactivarlo para el próximo cuadro
                mesh.render();
                mesh.Enabled = false;
            }

            //Luego renderizar modelos visibles con alpha
            foreach (var mesh in scene.Meshes)
            {
                //Contador de modelos
                if (mesh.Enabled && mesh.AlphaBlendEnable)
                {
                    meshCount++;
                }

                //Renderizar modelo y luego desactivarlo para el próximo cuadro
                mesh.render();
                mesh.Enabled = false;
            }

            //Contador de modelos visibles
            GuiController.Instance.UserVars["MeshCount"] = meshCount.ToString();

            //Renderizar portales
            var showPortals = (bool)GuiController.Instance.Modifiers["showPortals"];
            if (showPortals)
            {
                scene.PortalRendering.renderPortals();
            }
        }

        public override void close()
        {
            scene.disposeAll();
        }
    }
}