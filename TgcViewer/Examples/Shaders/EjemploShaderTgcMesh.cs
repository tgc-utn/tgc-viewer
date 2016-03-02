using System;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Viewer;

namespace TGC.Examples.Shaders
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

        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "Shader con TgcMesh";
        }

        public override string getDescription()
        {
            return "Muestra como utilizar shaders en un TgcMesh.";
        }

        public override void init()
        {
            //Crear loader
            var loader = new TgcSceneLoader();

            //Cargar mesh
            var scene =
                loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir +
                                         "MeshCreator\\Meshes\\Vehiculos\\GruaExcavadora\\GruaExcavadora-TgcScene.xml");
            mesh = scene.Meshes[0];

            //Cargar Shader personalizado
            mesh.Effect = TgcShaders.loadEffect(GuiController.Instance.ExamplesMediaDir + "Shaders\\Ejemplo1.fx");

            //Modifier para Technique de shader
            GuiController.Instance.Modifiers.addInterval("Technique", new[]
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
            GuiController.Instance.Modifiers.addFloat("darkFactor", 0f, 1f, 0.5f);
            GuiController.Instance.Modifiers.addFloat("textureOffset", 0f, 1f, 0.5f);
            r = new Random();

            //Centrar camara rotacional respecto a este mesh
            GuiController.Instance.RotCamera.targetObject(mesh.BoundingBox);
        }

        public override void render(float elapsedTime)
        {
            //Actualizar Technique
            mesh.Technique = (string)GuiController.Instance.Modifiers["Technique"];

            //Cargar variables de shader
            mesh.Effect.SetValue("darkFactor", (float)GuiController.Instance.Modifiers["darkFactor"]);
            mesh.Effect.SetValue("random", (float)r.NextDouble());
            mesh.Effect.SetValue("textureOffset", (float)GuiController.Instance.Modifiers["textureOffset"]);

            mesh.render();
        }

        public override void close()
        {
            mesh.dispose();
            mesh.Effect.Dispose();
        }
    }
}