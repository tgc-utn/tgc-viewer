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
    /// Ejemplo EjemploLinternaPhong:
    /// Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    /// 
    /// Ejemplo avanzado. Ver primero ejemplos "Shaders/EjemploPhongShading" y "PortalRendering/PortalRenderingFramework"
    /// 
    /// Carga un escenario con Portal Rendering y muestra como utilizar PhongShading para generar el efecto de una linterna
    /// en primera persona.
    /// El ejemplo permite modificar los parámetros de iluminación para ver como afectan sobre el objeto
    /// en tiempo real.
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploLinternaPhong: TgcExample
    {
        TgcScene scene;
        Effect effect;
        TgcBox lightBox;

        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "Linterna Phong";
        }

        public override string getDescription()
        {
            return "Escenario en 1ra persona con Linterna en tiempo real usando Phong Shading";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Cargar escenario con información especial exportada de PortalRendering
            TgcSceneLoader loader = new TgcSceneLoader();
            //Configurar MeshFactory customizado
            loader.MeshFactory = new CustomMeshShaderFactory();
            scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesDir + "PortalRendering\\EscenarioPortal\\EscenarioPortal-TgcScene.xml");

            //Descactivar inicialmente a todos los modelos
            scene.setMeshesEnabled(false);
            
            //Camara en 1ra persona
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.MovementSpeed = 800f;
            GuiController.Instance.FpsCamera.JumpSpeed = 600f;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(0, 0, 0), new Vector3(0, 0, 1));



            //Cargar Shader de PhonhShading
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Shaders\\PhongShading.fx", null, null, ShaderFlags.None, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique
            effect.Technique = "DefaultTechnique";

            //Agregar el shader en cada Mesh
            foreach (TgcMeshShader mesh in scene.Meshes)
            {
                mesh.Effect = effect;
            }


            //Modifier para variables de shader
            GuiController.Instance.Modifiers.addColor("AmbientColor", Color.White);
            GuiController.Instance.Modifiers.addColor("DiffuseColor", Color.Green);
            GuiController.Instance.Modifiers.addColor("SpecularColor", Color.Red);
            GuiController.Instance.Modifiers.addFloat("SpecularPower", 1, 100, 16);

            lightBox = TgcBox.fromSize(new Vector3(1, 1, 1));
        }

        


        public override void render(float elapsedTime)
        {
            Device device = GuiController.Instance.D3dDevice;

            //Actualizar visibilidad con PortalRendering
            Vector3 cameraPos = GuiController.Instance.FpsCamera.Position;
            scene.PortalRendering.updateVisibility(cameraPos);

            
            


            lightBox.Position = cameraPos + Vector3.Scale(Vector3.Normalize(GuiController.Instance.FpsCamera.LookAt - cameraPos), 100);
            Vector3 lightPosition = lightBox.Position;
            Vector3 eyePosition = cameraPos;


            //Cargar variables de shader comunes a todos los mesh
            effect.SetValue("fvLightPosition", TgcParserUtils.vector3ToFloat3Array(lightPosition));
            effect.SetValue("fvEyePosition", TgcParserUtils.vector3ToFloat3Array(eyePosition));
            effect.SetValue("fvAmbient", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["AmbientColor"]));
            effect.SetValue("fvDiffuse", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["DiffuseColor"]));
            effect.SetValue("fvSpecular", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["SpecularColor"]));
            effect.SetValue("fSpecularPower", (float)GuiController.Instance.Modifiers["SpecularPower"]);


            //Renderizar meshes
            foreach (TgcMeshShader mesh in scene.Meshes)
            {
                //Renderizar modelo y luego desactivarlo para el próximo cuadro
                mesh.render();
                mesh.Enabled = false;
            }


            lightBox.render();
        }



        public override void close()
        {
            scene.disposeAll();
            effect.Dispose();

            lightBox.dispose();
        }

    }

    

}
