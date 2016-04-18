using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Core.Interpolation;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Utils;
using TGC.Util;

namespace TGC.Examples.Lights
{
    /// <summary>
    ///     Ejemplo EjemploMultiDiffuseLights:
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminación - Iluminación dinámica
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "Lights/EjemploMultipleLights".
    ///     Muestra como aplicar iluminación dinámica con PhongShading por pixel en un Pixel Shader.
    ///     Utiliza varias luces para un mismo objeto, en una misma pasada de Shaders.
    ///     Solo calcula el componente Diffuse, para acelerar los cálculos.
    ///     Las luces poseen atenuación por la distancia.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploMultiDiffuseLights : TgcExample
    {
        private Effect effect;
        private InterpoladorVaiven interp;
        private TgcBox[] lightMeshes;
        private Vector3[] origLightPos;
        private TgcScene scene;

        public override string getCategory()
        {
            return "Lights";
        }

        public override string getName()
        {
            return "Multi-DiffuseLights";
        }

        public override string getDescription()
        {
            return "Iluminación dinámicas con 4 luces Diffuse a la vez para un mismo mesh";
        }

        public override void init()
        {
            //Cargar escenario
            var loader = new TgcSceneLoader();
            scene =
                loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir +
                                         "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");

            //Camara en 1ra persona
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.MovementSpeed = 400f;
            GuiController.Instance.FpsCamera.JumpSpeed = 300f;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-210.0958f, 114.911f, -109.2159f),
                new Vector3(-209.559f, 114.8029f, -108.3791f));

            //Cargar Shader personalizado de MultiDiffuseLights
            /*
             * Cargar Shader personalizado de MultiDiffuseLights
             * Este Shader solo soporta TgcMesh con RenderType DIFFUSE_MAP (que son las unicas utilizadas en este ejemplo)
             * El shader toma 4 luces a la vez para iluminar un mesh.
             * Pero como hacer 4 veces los calculos en el shader es costoso, de cada luz solo calcula el componente Diffuse.
             */
            effect = TgcShaders.loadEffect(GuiController.Instance.ShadersDir + "MultiDiffuseLights.fx");

            //Crear 4 mesh para representar las 4 para la luces. Las ubicamos en distintas posiciones del escenario, cada una con un color distinto.
            lightMeshes = new TgcBox[4];
            origLightPos = new Vector3[lightMeshes.Length];
            var c = new Color[4] { Color.Red, Color.Blue, Color.Green, Color.Yellow };
            for (var i = 0; i < lightMeshes.Length; i++)
            {
                var co = c[i % c.Length];
                lightMeshes[i] = TgcBox.fromSize(new Vector3(10, 10, 10), co);
                origLightPos[i] = new Vector3(-40, 20 + i * 20, 400);
            }

            //Modifiers
            GuiController.Instance.Modifiers.addBoolean("lightEnable", "lightEnable", true);
            GuiController.Instance.Modifiers.addBoolean("lightMove", "lightMove", true);
            GuiController.Instance.Modifiers.addFloat("lightIntensity", 0, 150, 38);
            GuiController.Instance.Modifiers.addFloat("lightAttenuation", 0.1f, 2, 0.15f);

            GuiController.Instance.Modifiers.addColor("mEmissive", Color.Black);
            GuiController.Instance.Modifiers.addColor("mDiffuse", Color.White);

            //Interpolador para mover las luces de un lado para el otro
            interp = new InterpoladorVaiven();
            interp.Min = -200f;
            interp.Max = 200f;
            interp.Speed = 100f;
            interp.Current = 0f;
        }

        public override void render(float elapsedTime)
        {
            //Habilitar luz
            var lightEnable = (bool)GuiController.Instance.Modifiers["lightEnable"];
            Effect currentShader;
            string currentTechnique;
            if (lightEnable)
            {
                //Shader personalizado de iluminacion
                currentShader = effect;
                currentTechnique = "MultiDiffuseLightsTechnique";
            }
            else
            {
                //Sin luz: Restaurar shader default
                currentShader = TgcShaders.Instance.TgcMeshShader;
                currentTechnique = TgcShaders.Instance.getTgcMeshTechnique(TgcMesh.MeshRenderType.DIFFUSE_MAP);
            }

            //Aplicar a cada mesh el shader actual
            foreach (var mesh in scene.Meshes)
            {
                mesh.Effect = currentShader;
                mesh.Technique = currentTechnique;
            }

            //Configurar los valores de cada luz
            var move = new Vector3(0, 0,
                (bool)GuiController.Instance.Modifiers["lightMove"] ? interp.update(elapsedTime) : 0);
            var lightColors = new ColorValue[lightMeshes.Length];
            var pointLightPositions = new Vector4[lightMeshes.Length];
            var pointLightIntensity = new float[lightMeshes.Length];
            var pointLightAttenuation = new float[lightMeshes.Length];
            for (var i = 0; i < lightMeshes.Length; i++)
            {
                var lightMesh = lightMeshes[i];
                lightMesh.Position = origLightPos[i] + Vector3.Scale(move, i + 1);

                lightColors[i] = ColorValue.FromColor(lightMesh.Color);
                pointLightPositions[i] = TgcParserUtils.vector3ToVector4(lightMesh.Position);
                pointLightIntensity[i] = (float)GuiController.Instance.Modifiers["lightIntensity"];
                pointLightAttenuation[i] = (float)GuiController.Instance.Modifiers["lightAttenuation"];
            }

            //Renderizar meshes
            foreach (var mesh in scene.Meshes)
            {
                if (lightEnable)
                {
                    //Cargar variables de shader
                    mesh.Effect.SetValue("lightColor", lightColors);
                    mesh.Effect.SetValue("lightPosition", pointLightPositions);
                    mesh.Effect.SetValue("lightIntensity", pointLightIntensity);
                    mesh.Effect.SetValue("lightAttenuation", pointLightAttenuation);
                    mesh.Effect.SetValue("materialEmissiveColor",
                        ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mEmissive"]));
                    mesh.Effect.SetValue("materialDiffuseColor",
                        ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mDiffuse"]));
                }

                //Renderizar modelo
                mesh.render();
            }

            //Renderizar meshes de luz
            for (var i = 0; i < lightMeshes.Length; i++)
            {
                var lightMesh = lightMeshes[i];
                lightMesh.render();
            }
        }

        public override void close()
        {
            scene.disposeAll();
            effect.Dispose();
            for (var i = 0; i < lightMeshes.Length; i++)
            {
                var lightMesh = lightMeshes[i];
                lightMesh.dispose();
            }
        }
    }
}