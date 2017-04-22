using Microsoft.DirectX;
using TGC.Core.Collision;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.Optimization
{
    /// <summary>
    ///     Ejemplo Escenario4toPiso:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Mesh
    ///     # Unidad 4 - Texturas e Iluminacion - Lightmap
    ///     # Unidad 7 - Tecnicas de Optimizacion - Frustum Culling, Fuerza Bruta
    ///     Utiliza la herramienta TgcSceneLoader para cargar un escenario 3D
    ///     similar al 4to piso de la sede Medrano de la facultad.
    ///     Este modelo fue utilizado en el segundo Trabajo Practico de Gestion de Datos de 2009.
    ///     El escenario no se encuentra optimizado (sin Frustum Culling ni Occlusion Culling) y
    ///     su performance puede variar de acuerdo a cada dispositivo grafico.
    ///     Permite aplicar Frustum Culling con metodo de Fuerza Bruta para mostrar el cambio de performance.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class Escenario4toPiso : TGCExampleViewer
    {
        private TgcScene tgcScene;

        public Escenario4toPiso(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Optimization";
            Name = "4to Piso Medrano";
            Description =
                "Utiliza la herramienta TgcSceneLoader para cargar el 4to Piso de la facultado modelado en formato TGC. " +
                "Muestra como utilizar FrustumCulling con fuerza bruta para acelerar la performance de renderizado.";
        }

        public override void Init()
        {
            //FPS Camara
            Camara = new TgcFpsCamera(new TGCVector3(-140f, 40f, -50f), 200f, 200f, Input);

            //Modifier para habilitar o deshabilitar FrustumCulling
            Modifiers.addBoolean("culling", "Frustum culling", true);

            //UserVar para contar la cantidad de meshes que se renderizan
            UserVars.addVar("Meshes renderizadas");

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
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

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
                        var r = TgcCollisionUtils.classifyFrustumAABB(Frustum, mesh.BoundingBox);
                        if (r != TgcCollisionUtils.FrustumResult.OUTSIDE)
                        {
                            mesh.Render();
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