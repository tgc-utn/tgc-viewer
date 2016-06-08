using System;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.ShadersExamples
{
    /// <summary>
    ///     Ejemplo EjemploShaderTgcMesh:
    ///     Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Muestra como utilizar shaders con un TgcMesh.
    ///     Carga un shader en formato .fx que posee varios Techniques.
    ///     El ejemplo permite elegir que Technique renderizar.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploShaderTgcMesh : TgcExample
    {
        private TgcMesh mesh;
        private Random r;

        public EjemploShaderTgcMesh(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Shaders";
            Name = "Shader con TgcMesh";
            Description = "Muestra como utilizar shaders en un TgcMesh.";
        }

        public override void Init()
        {
            //Crear loader
            var loader = new TgcSceneLoader();

            //Cargar mesh
            var scene =
                loader.loadSceneFromFile(MediaDir +
                                         "MeshCreator\\Meshes\\Vehiculos\\GruaExcavadora\\GruaExcavadora-TgcScene.xml");
            mesh = scene.Meshes[0];

            //Cargar Shader personalizado
            mesh.Effect = TgcShaders.loadEffect(ShadersDir + "Ejemplo1.fx");

            //Modifier para Technique de shader
            Modifiers.addInterval("Technique", new[]
            {
                "OnlyTexture",
                "OnlyColor",
                "Darkening",
                "Complementing",
                "MaskRedOut",
                "RedOnly",
                "RandomTexCoord",
                "RandomColorVS",
                "TextureOffset"
            }, 0);

            //Modifier para variables de shader
            Modifiers.addFloat("darkFactor", 0f, 1f, 0.5f);
            Modifiers.addFloat("textureOffset", 0f, 1f, 0.5f);
            r = new Random();

            //Centrar camara rotacional respecto a este mesh
            Camara = new TgcRotationalCamera(mesh.BoundingBox);
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            //Actualizar Technique
            mesh.Technique = (string)Modifiers["Technique"];

            //Cargar variables de shader
            mesh.Effect.SetValue("darkFactor", (float)Modifiers["darkFactor"]);
            mesh.Effect.SetValue("random", (float)r.NextDouble());
            mesh.Effect.SetValue("textureOffset", (float)Modifiers["textureOffset"]);

            mesh.render();

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            mesh.dispose();
            mesh.Effect.Dispose();
        }
    }
}