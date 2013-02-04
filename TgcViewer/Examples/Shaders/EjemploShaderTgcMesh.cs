using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.Shaders
{
    /// <summary>
    /// Ejemplo EjemploShaderTgcMesh:
    /// Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    /// 
    /// Ejemplo avanzado. Ver primero ejemplo "SceneLoader/CustomMesh".
    /// Muestra como extender la clase TgcMesh para renderizado comportamiento de Shaders.
    /// Carga un shader en formato .fx que posee varios Techniques.
    /// El ejemplo permite elegir que Technique renderizar.
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploShaderTgcMesh: TgcExample
    {

        TgcMeshShader mesh;
        Random r;


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
            return "Muestra como extender la clase TgcMesh para renderizado comportamiento de Shaders.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            //Configurar MeshFactory customizado
            loader.MeshFactory = new CustomMeshShaderFactory();

            //Cargar mesh
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\GruaExcavadora\\GruaExcavadora-TgcScene.xml");
            mesh = (TgcMeshShader)scene.Meshes[0];

            //Cargar Shader
            string compilationErrors;
            mesh.Effect = Effect.FromFile(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Shaders\\Ejemplo1.fx", null, null, ShaderFlags.None, null, out compilationErrors);
            if (mesh.Effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }


            //Modifier para Technique de shader
            GuiController.Instance.Modifiers.addInterval("Technique", new string[] { 
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
            Device device = GuiController.Instance.D3dDevice;


            //Actualizar Technique
            mesh.Effect.Technique = (string)GuiController.Instance.Modifiers["Technique"];

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
