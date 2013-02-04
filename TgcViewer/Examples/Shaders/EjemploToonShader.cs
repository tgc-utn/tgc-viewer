using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;
using System.Drawing;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.Shaders
{
    /// <summary>
    /// Ejemplo EjemploToonShader:
    /// Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    /// 
    /// Ejemplo avanzado. Ver primero ejemplo "SceneLoader/CustomMesh" y luego "Shaders/EjemploShaderTgcMesh".
    /// Muestra como aplicar la técnica Toon Shader para generar un renderizado que parezca de "caricatura".
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploToonShader: TgcExample
    {

        TgcMeshShader mesh;
        TgcBox ligtBox;


        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "ToonShader";
        }

        public override string getDescription()
        {
            return "ToonShader";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            //Configurar MeshFactory customizado
            loader.MeshFactory = new CustomMeshShaderFactory();

            //Cargar mesh
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Robot\\Robot-TgcScene.xml");
            //TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Pirata\\Pirata-TgcScene.xml");
            mesh = (TgcMeshShader)scene.Meshes[0];

            //Cargar Shader de PhonhShading
            string compilationErrors;
            mesh.Effect = Effect.FromFile(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Shaders\\ToonShader.fx", null, null, ShaderFlags.None, null, out compilationErrors);
            if (mesh.Effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique
            mesh.Effect.Technique = "DefaultTechnique";

            //Configurar evento para setear variables del shader antes de renderizar
            mesh.ShaderBegin += new TgcMeshShader.ShaderBeginHandler(mesh_ShaderBegin);
            mesh.ShaderPassBegin += new TgcMeshShader.ShaderPassBeginHandler(mesh_ShaderPassBegin);

            /*
            //Modifier para variables de shader
            GuiController.Instance.Modifiers.addVertex3f("LightPosition", new Vector3(-1000, -1000, -1000), new Vector3(1000, 1000, 1000), new Vector3(0, 400, 0));
            GuiController.Instance.Modifiers.addColor("AmbientColor", Color.White);
            GuiController.Instance.Modifiers.addColor("DiffuseColor", Color.Green);
            GuiController.Instance.Modifiers.addColor("SpecularColor", Color.Red);
            GuiController.Instance.Modifiers.addFloat("SpecularPower", 1, 100, 16);            
            */

            //Crear caja para indicar ubicacion de la luz
            ligtBox = TgcBox.fromSize(new Vector3(50, 50, 50), Color.Yellow);
            

            //Centrar camara rotacional respecto a este mesh
            GuiController.Instance.RotCamera.targetObject(mesh.BoundingBox);
        }

        


        public override void render(float elapsedTime)
        {
            Device device = GuiController.Instance.D3dDevice;

            /*
            Vector3 lightPosition = (Vector3)GuiController.Instance.Modifiers["LightPosition"];

            //Cargar variables de shader
            mesh.Effect.SetValue("fvLightPosition", TgcParserUtils.vector3ToFloat3Array(lightPosition));
            mesh.Effect.SetValue("fvEyePosition", TgcParserUtils.vector3ToFloat3Array(GuiController.Instance.RotCamera.getPosition()));
            mesh.Effect.SetValue("matView", device.Transform.View);
            mesh.Effect.SetValue("matViewProjection", device.Transform.View * device.Transform.Projection);
            mesh.Effect.SetValue("fvAmbient", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["AmbientColor"]));
            mesh.Effect.SetValue("fvDiffuse", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["DiffuseColor"]));
            mesh.Effect.SetValue("fvSpecular", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["SpecularColor"]));
            mesh.Effect.SetValue("fSpecularPower", (float)GuiController.Instance.Modifiers["SpecularPower"]);
            */


            /*
            //Mover mesh que representa la luz
            ligtBox.Position = lightPosition;
            ligtBox.render();
            */

            mesh.render();
            
        }


        void mesh_ShaderBegin(TgcMeshShader mesh)
        {
            Device device = GuiController.Instance.D3dDevice;

            //Cargar variables de shader
            mesh.Effect.SetValue("World", device.Transform.World);
            mesh.Effect.SetValue("View", device.Transform.View);
            mesh.Effect.SetValue("Projection", device.Transform.Projection);
            mesh.Effect.SetValue("WorldInverseTranspose", Matrix.TransposeMatrix(Matrix.Invert(device.Transform.World)));
        }

        void mesh_ShaderPassBegin(TgcMeshShader mesh, int pass)
        {
            Device device = GuiController.Instance.D3dDevice;

            /*
            switch (pass)
            {
                case 0:
                    device.RenderState.CullMode = Cull.Clockwise;
                    break;
                case 1:
                    device.RenderState.CullMode = Cull.CounterClockwise;
                    break;
                default:
                    break;
            }
            */
        }


        public override void close()
        {
            mesh.dispose();
            mesh.Effect.Dispose();
            ligtBox.dispose();
        }

    }

    

}
