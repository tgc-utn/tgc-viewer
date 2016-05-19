using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

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

        public PortalRenderingFramework(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "PortalRendering";
            Name = "PortalRendering Framework";
            Description =
                "Muestra como utilizar la técnica Portal Rendering para optimizar el renderizado de un escenario.";
        }

        public override void Init()
        {
            //Cargar escenario con información especial exportada de PortalRendering
            var loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(MediaDir + "EscenarioPortal\\EscenarioPortal-TgcScene.xml");

            //Descactivar inicialmente a todos los modelos
            scene.setMeshesEnabled(false);

            //Camara en 1ra persona
            Camara = new TgcFpsCamera();
            ((TgcFpsCamera)Camara).MovementSpeed = 800f;
            ((TgcFpsCamera)Camara).JumpSpeed = 600f;
            Camara.setCamera(new Vector3(0, 0, 0), new Vector3(0, 0, 1));

            //Modifiers
            Modifiers.addBoolean("portalRendering", "PortalRendering", true);
            Modifiers.addBoolean("WireFrame", "WireFrame", false);
            Modifiers.addBoolean("showPortals", "Show Portals", false);

            //UserVars
            UserVars.addVar("MeshCount");

            //Crear portales debug
            scene.PortalRendering.createDebugPortals(Color.Purple);
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            var enablePortalRendering = (bool)Modifiers["portalRendering"];
            if (enablePortalRendering)
            {
                //Actualizar visibilidad con PortalRendering
                scene.PortalRendering.updateVisibility(Camara.getPosition());
            }
            else
            {
                //Habilitar todo todo
                scene.setMeshesEnabled(true);
            }

            //WireFrame
            var wireFrameEnable = (bool)Modifiers["WireFrame"];
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
            UserVars["MeshCount"] = meshCount.ToString();

            //Renderizar portales
            var showPortals = (bool)Modifiers["showPortals"];
            if (showPortals)
            {
                scene.PortalRendering.renderPortals();
            }

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            scene.disposeAll();
        }
    }
}