using System;
using System.Windows.Forms;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.ShadersExamples
{
    /// <summary>
    ///     Ejemplo EjemploShaderTgcMesh:
    ///     Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Muestra como utilizar shaders con un TgcMesh.
    ///     Carga un shader en formato .fx que posee varios Techniques.
    ///     El ejemplo permite elegir que Technique renderizar.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploShaderTgcMesh : TGCExampleViewer
    {
        private TGCIntervalModifier techniqueModifier;
        private TGCFloatModifier darkFactorModifier;
        private TGCFloatModifier textureOffsetModifier;

        private TgcMesh mesh;
        private Random r;

        public EjemploShaderTgcMesh(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Pixel y Vertex Shaders";
            Name = "TgcMesh Shaders Basicos";
            Description = "Muestra como utilizar shaders en un TgcMesh.";
        }

        public override void Init()
        {
            //Crear loader
            var loader = new TgcSceneLoader();

            //Cargar mesh
            var scene =
                loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\GruaExcavadora\\GruaExcavadora-TgcScene.xml");
            mesh = scene.Meshes[0];

            //Cargar Shader personalizado
            mesh.Effect = TGCShaders.Instance.LoadEffect(ShadersDir + "Ejemplo1.fx");

            //Modifier para Technique de shader
            techniqueModifier = AddInterval("Technique", new[]
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
            darkFactorModifier = AddFloat("darkFactor", 0f, 1f, 0.5f);
            textureOffsetModifier = AddFloat("textureOffset", 0f, 1f, 0.5f);
            r = new Random();

            //Centrar camara rotacional respecto a este mesh
            Camara = new TgcRotationalCamera(mesh.BoundingBox.calculateBoxCenter(), mesh.BoundingBox.calculateBoxRadius() * 2, Input);
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Actualizar Technique
            mesh.Technique = techniqueModifier.Value.ToString();

            //Cargar variables de shader
            mesh.Effect.SetValue("darkFactor", darkFactorModifier.Value);
            mesh.Effect.SetValue("random", (float)r.NextDouble());
            mesh.Effect.SetValue("textureOffset", textureOffsetModifier.Value);

            mesh.Render();

            PostRender();
        }

        public override void Dispose()
        {
            mesh.Dispose();
            mesh.Effect.Dispose();
        }
    }
}