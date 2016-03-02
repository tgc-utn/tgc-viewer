using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Core.Utils;
using TGC.Viewer;

namespace TGC.Examples.Lights
{
    /// <summary>
    ///     Ejemplo EjemploIntegrador:
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminación - Iluminación dinámica
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "Lights/EjemploMultipleLights" y "Lights/EjemploEnvironmentMap"
    ///     Muestra como integrar distintas tecnicas aisladas de iluminacion que se vieron en otros ejemplos.
    ///     Carga un escenario de 3D MAX.
    ///     El escenario tiene tres layers:
    ///     - Lights: contiene objetos que sirven para posicionar luces dentro del escenario
    ///     - BumpMap: contiene meshes que tienen NormalMap
    ///     - otros layers: meshes comunes
    ///     El ejemplo utiliza los efectos: BumpMapping, EnvironmentMap y Phong-Shading con PointLight.
    ///     Cada mesh se ve afectado por una solo la luz (la mas cercana).
    ///     Todos los meshes del layer BumpMap tienen una textura de NormalMap. Por convención se llama:
    ///     NOMBRE_ORIGINAL_NormalMap.png
    ///     Todos los meshes se ven afectados por un unico CubeMap.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploIntegrador : TgcExample
    {
        private List<TgcMeshBumpMapping> bumpMeshes;
        private List<TgcMesh> commonMeshes;
        private CubeTexture cubeMap;
        private Effect effect;
        private List<LightData> lights;

        public override string getCategory()
        {
            return "Lights";
        }

        public override string getName()
        {
            return "Integrador";
        }

        public override string getDescription()
        {
            return "Ejemplo que muestra un escenario con BumpMapping, EnvironmentMap y varias Point Lights";
        }

        public override void init()
        {
            //Cargar textura de CubeMap para Environment Map, fijo para todos los meshes
            cubeMap = TextureLoader.FromCubeFile(D3DDevice.Instance.Device,
                GuiController.Instance.ExamplesMediaDir + "Shaders\\CubeMap.dds");

            //Cargar Shader personalizado de EnvironmentMap
            effect = TgcShaders.loadEffect(GuiController.Instance.ExamplesMediaDir + "Shaders\\EnvironmentMap.fx");

            //Cargar escenario, pero inicialmente solo hacemos el parser, para separar los objetos que son solo luces y no meshes
            var scenePath = GuiController.Instance.ExamplesDir + "Lights\\NormalMapRoom\\NormalMapRoom-TgcScene.xml";
            var mediaPath = GuiController.Instance.ExamplesDir + "Lights\\NormalMapRoom\\";
            var parser = new TgcSceneParser();
            var sceneData = parser.parseSceneFromString(File.ReadAllText(scenePath));

            //Separar modelos reales de las luces, segun layer "Lights"
            lights = new List<LightData>();
            var realMeshData = new List<TgcMeshData>();
            for (var i = 0; i < sceneData.meshesData.Length; i++)
            {
                var meshData = sceneData.meshesData[i];

                //Es una luz, no cargar mesh, solo importan sus datos
                if (meshData.layerName == "Lights")
                {
                    //Guardar datos de luz
                    var light = new LightData();
                    light.color = Color.FromArgb((int)meshData.color[0], (int)meshData.color[1],
                        (int)meshData.color[2]);
                    light.aabb = new TgcBoundingBox(TgcParserUtils.float3ArrayToVector3(meshData.pMin),
                        TgcParserUtils.float3ArrayToVector3(meshData.pMax));
                    light.pos = light.aabb.calculateBoxCenter();
                    lights.Add(light);
                }
                //Es un mesh real, agregar a array definitivo
                else
                {
                    realMeshData.Add(meshData);
                }
            }

            //Reemplazar array original de meshData de sceneData por el definitivo
            sceneData.meshesData = realMeshData.ToArray();

            //Ahora si cargar meshes reales
            var loader = new TgcSceneLoader();
            var scene = loader.loadScene(sceneData, mediaPath);

            //Separar meshes con bumpMapping de los comunes
            bumpMeshes = new List<TgcMeshBumpMapping>();
            commonMeshes = new List<TgcMesh>();
            foreach (var mesh in scene.Meshes)
            {
                //Mesh con BumpMapping
                if (mesh.Layer == "BumpMap")
                {
                    //Por convencion de este ejemplo el NormalMap se llama igual que el DiffuseMap (y cada mesh tiene una sola)
                    var path = mesh.DiffuseMaps[0].FilePath;
                    var split = path.Split('.');
                    path = split[0] + "_NormalMap.png";

                    //Convertir TgcMesh a TgcMeshBumpMapping
                    var normalMap = TgcTexture.createTexture(path);
                    TgcTexture[] normalMapArray = { normalMap };
                    var bumpMesh = TgcMeshBumpMapping.fromTgcMesh(mesh, normalMapArray);
                    bumpMesh.Effect = effect;
                    bumpMesh.Technique = "EnvironmentMapTechnique";
                    bumpMeshes.Add(bumpMesh);

                    //Liberar original
                    mesh.dispose();
                }
                //Mesh normal
                else
                {
                    commonMeshes.Add(mesh);
                }
            }

            //Camara en 1ra persona
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(0, 50, 100), new Vector3(0, 50, -1));

            //Modifiers
            GuiController.Instance.Modifiers.addBoolean("lightEnable", "lightEnable", true);
            GuiController.Instance.Modifiers.addFloat("reflection", 0, 1, 0.2f);
            GuiController.Instance.Modifiers.addFloat("bumpiness", 0, 2, 1f);
            GuiController.Instance.Modifiers.addFloat("lightIntensity", 0, 150, 20);
            GuiController.Instance.Modifiers.addFloat("lightAttenuation", 0.1f, 2, 0.3f);
            GuiController.Instance.Modifiers.addFloat("specularEx", 0, 20, 9f);

            GuiController.Instance.Modifiers.addColor("mEmissive", Color.Black);
            GuiController.Instance.Modifiers.addColor("mAmbient", Color.White);
            GuiController.Instance.Modifiers.addColor("mDiffuse", Color.White);
            GuiController.Instance.Modifiers.addColor("mSpecular", Color.White);
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
                currentTechnique = "EnvironmentMapTechnique";
            }
            else
            {
                //Sin luz: Restaurar shader default
                currentShader = TgcShaders.Instance.TgcMeshShader;
                currentTechnique = TgcShaders.Instance.getTgcMeshTechnique(TgcMesh.MeshRenderType.DIFFUSE_MAP);
            }

            //Aplicar a cada mesh el shader actual
            foreach (TgcMesh mesh in bumpMeshes)
            {
                mesh.Effect = currentShader;
                mesh.Technique = currentTechnique;
            }

            var eyePosition = GuiController.Instance.FpsCamera.getPosition();

            //Renderizar meshes con BumpMapping
            foreach (var mesh in bumpMeshes)
            {
                if (lightEnable)
                {
                    //Obtener la luz que corresponde a este mesh (buscamos la mas cercana)
                    var light = getClosestLight(mesh.BoundingBox.calculateBoxCenter());

                    mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(eyePosition));
                    mesh.Effect.SetValue("bumpiness", (float)GuiController.Instance.Modifiers["bumpiness"]);
                    mesh.Effect.SetValue("reflection", (float)GuiController.Instance.Modifiers["reflection"]);
                    mesh.Effect.SetValue("lightIntensity", (float)GuiController.Instance.Modifiers["lightIntensity"]);
                    mesh.Effect.SetValue("lightAttenuation",
                        (float)GuiController.Instance.Modifiers["lightAttenuation"]);

                    //Cargar variables de shader de la luz
                    mesh.Effect.SetValue("lightColor", ColorValue.FromColor(light.color));
                    mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(light.pos));

                    //Cargar variables de shader del Material
                    mesh.Effect.SetValue("materialEmissiveColor",
                        ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mEmissive"]));
                    mesh.Effect.SetValue("materialAmbientColor",
                        ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mAmbient"]));
                    mesh.Effect.SetValue("materialDiffuseColor",
                        ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mDiffuse"]));
                    mesh.Effect.SetValue("materialSpecularColor",
                        ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mSpecular"]));
                    mesh.Effect.SetValue("materialSpecularExp", (float)GuiController.Instance.Modifiers["specularEx"]);

                    //CubeMap
                    mesh.Effect.SetValue("texCubeMap", cubeMap);
                }

                //Renderizar modelo
                mesh.render();
            }

            //Renderizar meshes comunes
            foreach (var mesh in commonMeshes)
            {
                mesh.render();
            }
        }

        /// <summary>
        ///     Devuelve la luz mas cercana a la posicion especificada
        /// </summary>
        private LightData getClosestLight(Vector3 pos)
        {
            var minDist = float.MaxValue;
            LightData minLight = null;

            foreach (var light in lights)
            {
                var distSq = Vector3.LengthSq(pos - light.pos);
                if (distSq < minDist)
                {
                    minDist = distSq;
                    minLight = light;
                }
            }

            return minLight;
        }

        public override void close()
        {
            effect.Dispose();
            foreach (var m in bumpMeshes)
            {
                m.dispose();
            }
            foreach (var m in commonMeshes)
            {
                m.dispose();
            }
            cubeMap.Dispose();
        }

        /// <summary>
        ///     Estructura auxiliar para informacion de luces
        /// </summary>
        public class LightData
        {
            public TgcBoundingBox aabb;
            public Color color;
            public Vector3 pos;
        }
    }
}