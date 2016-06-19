using Microsoft.DirectX;
using System;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.SceneLoader
{
    /// <summary>
    ///     Ejemplo Escenario4toPiso:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     # Unidad 4 - Texturas e Iluminación - Lightmap
    ///     # Unidad 7 - Técnicas de Optimización - Frustum Culling, Fuerza Bruta
    ///     Utiliza la herramienta TgcSceneLoader para cargar un escenario 3D
    ///     similar al 4to piso de la sede Medrano de la facultad.
    ///     Este modelo fue utilizado en el segundo Trabajo Práctico de Gestión de Datos de 2009.
    ///     El escenario no se encuentra optimizado (sin Frustum Culling ni Occlusion Culling) y
    ///     su performance puede variar de acuerdo a cada dispositivo gráfico.
    ///     Permite aplicar Frustum Culling con método de Fuerza Bruta para mostrar el cambio de performance.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class Escenario4toPiso : TgcExample
    {
        private TgcScene tgcScene;

        public Escenario4toPiso(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "SceneLoader";
            Name = "4toPiso";
            Description =
                "Utiliza la herramienta TgcSceneLoader para cargar el 4to Piso de la facultado modelado en formato TGC. " +
                "Muestra como utilizar FrustumCulling con fuerza bruta para acelerar la performance de renderizado.";
        }

        public override void Init()
        {
            //FPS Camara
            Camara = new TgcFpsCamera(new Vector3(-140f, 40f, -50f), 200f, 200f);

            //Cargar escena desde archivo ZIP
            var loader = new TgcSceneLoader();
            tgcScene = loader.loadSceneFromZipFile("4toPiso-TgcScene.xml", MediaDir + "4toPiso\\4toPiso.zip",
                MediaDir + "4toPiso\\Extract\\");

            /*
            //Version para cargar escena desde carpeta descomprimida
            TgcSceneLoader loader = new TgcSceneLoader();
            tgcScene = loader.loadSceneFromFile(
                this.MediaDir + "4toPiso\\Extract\\4toPiso-TgcScene.xml",
                this.MediaDir + "4toPiso\\Extract\\");
            */

            //Modifier para habilitar o deshabilitar FrustumCulling
            Modifiers.addBoolean("culling", "Frustum culling", true);

            //UserVar para contar la cantidad de meshes que se renderizan
            UserVars.addVar("Meshes renderizadas");
        }

        public override void Update()
        {
            base.PreUpdate();
        }

        public override void Render()
        {
            base.PreRender();
            

            var frustumCullingEnabled = (bool)Modifiers["culling"];

            //Renderizar sin ninguna optimizacion
            if (!frustumCullingEnabled)
            {
                tgcScene.renderAll();
                UserVars.setValue("Meshes renderizadas", tgcScene.Meshes.Count);
            }

            //Renderizar con Frustum Culling
            else
            {
                //Analizar cada malla contra el Frustum - con fuerza bruta
                var totalMeshes = 0;
                foreach (var mesh in tgcScene.Meshes)
                {
                    //Nos ocupamos solo de las mallas habilitadas
                    if (mesh.Enabled)
                    {
                        //Solo mostrar la malla si colisiona contra el Frustum
                        var r = TgcCollisionUtils.classifyFrustumAABB(TgcFrustum.Instance, mesh.BoundingBox);
                        if (r != TgcCollisionUtils.FrustumResult.OUTSIDE)
                        {
                            mesh.render();
                            totalMeshes++;
                        }
                    }
                }
                //Actualizar cantidad de meshes dibujadas
                UserVars.setValue("Meshes renderizadas", totalMeshes);
            }

            PostRender();
        }

        public override void Dispose()
        {
            

            tgcScene.disposeAll();
        }
    }
}