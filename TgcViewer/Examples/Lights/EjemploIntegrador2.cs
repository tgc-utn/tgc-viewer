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
using Examples.Shaders;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils.Interpolation;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace Examples.Lights
{
    /// <summary>
    /// Ejemplo EjemploIntegrador2:
    /// Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminación - Iluminación dinámica
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    /// 
    /// Ejemplo avanzado. Ver primero ejemplo "Lights/EjemploIntegrador"
    /// 
    /// xxxxxxxxxxxxxxxxx
    /// 
    /// 
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploIntegrador2 : TgcExample
    {
        Effect effect;
        List<TgcMesh> commonMeshes;
        CubeTexture cubeMap;
        List<LightData> lights;
        List<MeshLightData> meshesWithLight;


        public override string getCategory()
        {
            return "Lights";
        }

        public override string getName()
        {
            return "Integrador 2";
        }

        public override string getDescription()
        {
            return "Ejemplo que muestra un escenario con BumpMapping, EnvironmentMap y varias Point Lights";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;


            //Cargar textura de CubeMap para Environment Map, fijo para todos los meshes
            cubeMap = TextureLoader.FromCubeFile(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Shaders\\CubeMap.dds");

            //Cargar Shader personalizado de EnvironmentMap
            effect = TgcShaders.loadEffect(GuiController.Instance.ExamplesMediaDir + "Shaders\\EnvironmentMap_Integrador2.fx");


            //Cargar escenario, pero inicialmente solo hacemos el parser, para separar los objetos que son solo luces y no meshes
            string scenePath = GuiController.Instance.ExamplesDir + "Lights\\NormalMapRoom\\NormalMapRoom-TgcScene.xml";
            string mediaPath = GuiController.Instance.ExamplesDir + "Lights\\NormalMapRoom\\";
            TgcSceneParser parser = new TgcSceneParser();
            TgcSceneData sceneData = parser.parseSceneFromString(File.ReadAllText(scenePath));

            //Separar modelos reales de las luces, segun layer "Lights"
            lights = new List<LightData>();
            List<TgcMeshData> realMeshData = new List<TgcMeshData>();
            for (int i = 0; i < sceneData.meshesData.Length; i++)
            {
                TgcMeshData meshData = sceneData.meshesData[i];

                //Es una luz, no cargar mesh, solo importan sus datos
                if (meshData.layerName == "Lights")
                {
                    //Guardar datos de luz
                    LightData light = new LightData();
                    light.color = Color.FromArgb((int)meshData.color[0], (int)meshData.color[1], (int)meshData.color[2]);
                    light.aabb = new TgcBoundingBox(TgcParserUtils.float3ArrayToVector3(meshData.pMin), TgcParserUtils.float3ArrayToVector3(meshData.pMax));
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
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadScene(sceneData, mediaPath);

            //Separar meshes con bumpMapping de los comunes
            List<TgcMeshBumpMapping> bumpMeshes = new List<TgcMeshBumpMapping>();
            commonMeshes = new List<TgcMesh>();
            foreach (TgcMesh mesh in scene.Meshes)
            {
                //Mesh con BumpMapping
                if (mesh.Layer == "BumpMap")
                {
                    //Por convencion de este ejemplo el NormalMap se llama igual que el DiffuseMap (y cada mesh tiene una sola)
                    string path = mesh.DiffuseMaps[0].FilePath;
                    string[] split = path.Split('.');
                    path = split[0] + "_NormalMap.png";

                    //Convertir TgcMesh a TgcMeshBumpMapping
                    TgcTexture normalMap = TgcTexture.createTexture(path);
                    TgcTexture[] normalMapArray = new TgcTexture[] { normalMap };
                    TgcMeshBumpMapping bumpMesh = TgcMeshBumpMapping.fromTgcMesh(mesh, normalMapArray);
                    bumpMesh.Effect = effect;
                    bumpMesh.Technique = "ThreeLightsTechnique";
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


            //Pre-calculamos las 3 luces mas cercanas de cada mesh
            meshesWithLight = new List<MeshLightData>();
            foreach (TgcMeshBumpMapping mesh in bumpMeshes)
            {
                MeshLightData meshData = new MeshLightData();
                meshData.mesh = mesh;
                Vector3 meshCeter = mesh.BoundingBox.calculateBoxCenter();
                meshData.lights[0] = getClosestLight(meshCeter, null, null);
                meshData.lights[1] = getClosestLight(meshCeter, meshData.lights[0], null);
                meshData.lights[2] = getClosestLight(meshCeter, meshData.lights[0], meshData.lights[1]);
                meshesWithLight.Add(meshData);
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
            Device device = GuiController.Instance.D3dDevice;

            //Habilitar luz
            bool lightEnable = (bool)GuiController.Instance.Modifiers["lightEnable"];
            Effect currentShader;
            String currentTechnique;
            if (lightEnable)
            {
                //Shader personalizado de iluminacion
                currentShader = this.effect;
                currentTechnique = "ThreeLightsTechnique";
            }
            else
            {
                //Sin luz: Restaurar shader default
                currentShader = GuiController.Instance.Shaders.TgcMeshShader;
                currentTechnique = GuiController.Instance.Shaders.getTgcMeshTechnique(TgcMesh.MeshRenderType.DIFFUSE_MAP);
            }

            //Aplicar a cada mesh el shader actual
            foreach (MeshLightData meshData in meshesWithLight)
            {
                meshData.mesh.Effect = currentShader;
                meshData.mesh.Technique = currentTechnique;
            }


            Vector3 eyePosition = GuiController.Instance.FpsCamera.getPosition();
            
            //Renderizar meshes con BumpMapping
            foreach (MeshLightData meshData in meshesWithLight)
            {
                TgcMeshBumpMapping mesh = meshData.mesh;

                if (lightEnable)
                {
                    mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(eyePosition));
                    mesh.Effect.SetValue("bumpiness", (float)GuiController.Instance.Modifiers["bumpiness"]);
                    mesh.Effect.SetValue("reflection", (float)GuiController.Instance.Modifiers["reflection"]);

                    //Cargar variables de shader del Material
                    mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mEmissive"]));
                    mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mAmbient"]));
                    mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mDiffuse"]));
                    mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mSpecular"]));
                    mesh.Effect.SetValue("materialSpecularExp", (float)GuiController.Instance.Modifiers["specularEx"]);

                    //CubeMap
                    mesh.Effect.SetValue("texCubeMap", cubeMap);

                    //Cargar variables de shader de las 3 luces
                    //Intensidad y atenuacion deberian ser atributos propios de cada luz
                    float lightIntensity = (float)GuiController.Instance.Modifiers["lightIntensity"];
                    float lightAttenuation = (float)GuiController.Instance.Modifiers["lightAttenuation"];
                    mesh.Effect.SetValue("lightIntensity", new float[]{lightIntensity, lightIntensity, lightIntensity});
                    mesh.Effect.SetValue("lightAttenuation", new float[] { lightAttenuation, lightAttenuation, lightAttenuation});

                    mesh.Effect.SetValue("lightColor", new ColorValue[] { ColorValue.FromColor(meshData.lights[0].color), ColorValue.FromColor(meshData.lights[1].color), ColorValue.FromColor(meshData.lights[2].color) });
                    mesh.Effect.SetValue("lightPosition", new Vector4[] { TgcParserUtils.vector3ToVector4(meshData.lights[0].pos), TgcParserUtils.vector3ToVector4(meshData.lights[1].pos), TgcParserUtils.vector3ToVector4(meshData.lights[2].pos)});
                }
                

                //Renderizar modelo
                mesh.render();
            }

            //Renderizar meshes comunes
            foreach (TgcMesh mesh in commonMeshes)
            {
                mesh.render();
            }


        }


        /// <summary>
        /// Devuelve la luz mas cercana a la posicion especificada
        /// </summary>
        private LightData getClosestLight(Vector3 pos, LightData ignore1, LightData ignore2)
        {
            float minDist = float.MaxValue;
            LightData minLight = null;

            foreach (LightData light in lights)
            {
                //Ignorar las luces indicadas
                if (ignore1 != null && light.Equals(ignore1))
                    continue;
                if (ignore2 != null && light.Equals(ignore2))
                    continue;

                float distSq = Vector3.LengthSq(pos - light.pos);
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
            foreach (MeshLightData meshData in meshesWithLight)
            {
                meshData.mesh.dispose();
            }
            foreach (TgcMesh m in commonMeshes)
            {
                m.dispose();
            }
            cubeMap.Dispose();
        }

        /// <summary>
        /// Estructura auxiliar para informacion de luces
        /// </summary>
        public class LightData
        {
            public Vector3 pos;
            public TgcBoundingBox aabb;
            public Color color;
        }


        /// <summary>
        /// Estructura auxiliar para guardar un mesh y sus tres luces mas cercanas
        /// </summary>
        public class MeshLightData
        {
            public TgcMeshBumpMapping mesh;
            public LightData[] lights = new LightData[3];
        }


    }

    

}
