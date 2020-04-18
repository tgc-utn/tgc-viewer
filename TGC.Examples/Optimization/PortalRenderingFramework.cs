using Microsoft.DirectX.Direct3D;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.Optimization
{
    /// <summary>
    ///     Ejemplo PortalRenderingFramework
    ///     Unidades Involucradas:
    ///     # Unidad 7 - Optimizacion - Portal Rendering
    ///     Muestra como utilizar la tecnica Portal Rendering para optimizar el renderizado de un escenario.
    ///     Utiliza la informacion de Celdas y Portales exportada por el plugin de 3Ds MAX.
    ///     Utiliza la clase TgcPortalRenderingManager de TgcScene para manejar todo el renderizado de esta tecnica.
    ///     Esta clase posee una implementacion basica de la estrategia de Portal Rendering. Hay muchos puntos pendientes
    ///     aun de mejorar y optimizar.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class PortalRenderingFramework : TGCExampleViewer
    {
        private TGCBooleanModifier portalRenderingModifier;
        private TGCBooleanModifier wireFrameModifier;
        private TGCBooleanModifier showPortalsModifier;

        private TgcScene scene;

        public PortalRenderingFramework(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Optimization";
            Name = "PortalRendering Framework";
            Description =
                "Muestra como utilizar la tecnica Portal Rendering para optimizar el renderizado de un escenario.";
        }

        public override void Init()
        {
            //Cargar escenario con informacion especial exportada de PortalRendering
            var loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(MediaDir + "EscenarioPortal\\EscenarioPortal-TgcScene.xml");

            //Descactivar inicialmente a todos los modelos
            scene.setMeshesEnabled(false);

            //Camara en 1ra persona
            Camera = new TgcFpsCamera(TGCVector3.Empty, 800f, 600f, Input);

            //Modifiers
            portalRenderingModifier = AddBoolean("portalRendering", "PortalRendering", true);
            wireFrameModifier = AddBoolean("WireFrame", "WireFrame", false);
            showPortalsModifier = AddBoolean("showPortals", "Show Portals", false);

            //UserVars
            UserVars.addVar("MeshCount");

            //Crear portales debug
            scene.PortalRendering.createDebugPortals(Color.Purple);
        }

        public override void Update()
        {
            //  Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones ante ellas.
        }

        public override void Render()
        {
            PreRender();

            var enablePortalRendering = portalRenderingModifier.Value;
            if (enablePortalRendering)
            {
                //Actualizar visibilidad con PortalRendering
                scene.PortalRendering.updateVisibility(Camera.Position, Frustum);
            }
            else
            {
                //Habilitar todo todo
                scene.setMeshesEnabled(true);
            }

            //WireFrame
            var wireFrameEnable = wireFrameModifier.Value;
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

                //Renderizar modelo y luego desactivarlo para el proximo cuadro
                mesh.Render();
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

                //Renderizar modelo y luego desactivarlo para el proximo cuadro
                mesh.Render();
                mesh.Enabled = false;
            }

            //Contador de modelos visibles
            UserVars["MeshCount"] = meshCount.ToString();

            //Renderizar portales
            var showPortals = showPortalsModifier.Value;
            if (showPortals)
            {
                scene.PortalRendering.renderPortals();
            }

            PostRender();
        }

        public override void Dispose()
        {
            scene.DisposeAll();
        }
    }
}