using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TGC.Core.BoundingVolumes;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.Lights
{
    /// <summary>
    ///     Ejemplo EjemploIntegrador2:
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminacion - Iluminacion dinamica
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "Lights/EjemploIntegrador"
    ///     Similar al Ejemplo Integrador 1 pero se usan 3 luces para cada mesh.
    ///     Se pre-calcula que luz le corresponde a cada mesh segun su cercania.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploIntegrador2 : TGCExampleViewer
    {
        private List<TgcMesh> commonMeshes;
        private CubeTexture cubeMap;
        private Effect effect;
        private List<LightData> lights;
        private List<MeshLightData> meshesWithLight;

        public EjemploIntegrador2(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Pixel y Vertex Shaders";
            Name = "BumpMap + EnvMap + 3 Point Light por Proximidad";
            Description = "Ejemplo que muestra un escenario con BumpMapping, EnvironmentMap y varias Point Lights.";
        }

        public override void Init()
        {
            //Cargar textura de CubeMap para Environment Map, fijo para todos los meshes
            cubeMap = TextureLoader.FromCubeFile(D3DDevice.Instance.Device, MediaDir + "CubeMap.dds");

            //Cargar Shader personalizado de EnvironmentMap
            effect = TgcShaders.loadEffect(ShadersDir + "EnvironmentMap_Integrador2.fx");

            //Cargar escenario, pero inicialmente solo hacemos el parser, para separar los objetos que son solo luces y no meshes
            var scenePath = MediaDir + "NormalMapRoom\\NormalMapRoom-TgcScene.xml";
            var mediaPath = MediaDir + "NormalMapRoom\\";
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
                    light.aabb = new TgcBoundingAxisAlignBox(TGCVector3.Float3ArrayToVector3(meshData.pMin), TGCVector3.Float3ArrayToVector3(meshData.pMax));
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
            var bumpMeshes = new List<TgcMeshBumpMapping>();
            commonMeshes = new List<TgcMesh>();
            foreach (var mesh in scene.Meshes)
            {
                //Mesh con BumpMapping
                if (mesh.Layer == "BumpMap")
                {
                    //Por convencion de este ejemplo el NormalMap se llama igual que el DiffuseMap (y cada mesh tiene una sola)
                    var path = mesh.DiffuseMaps[0].FilePath;
                    var split = path.Split('.');
                    path = split[0] + "." + split[1] + "_NormalMap.png";

                    //Convertir TgcMesh a TgcMeshBumpMapping
                    var normalMap = TgcTexture.createTexture(path);
                    TgcTexture[] normalMapArray = { normalMap };
                    var bumpMesh = TgcMeshBumpMapping.fromTgcMesh(mesh, normalMapArray);
                    bumpMesh.Effect = effect;
                    bumpMesh.Technique = "ThreeLightsTechnique";
                    bumpMeshes.Add(bumpMesh);

                    //Liberar original
                    mesh.Dispose();
                }
                //Mesh normal
                else
                {
                    commonMeshes.Add(mesh);
                }
            }

            //Pre-calculamos las 3 luces mas cercanas de cada mesh
            meshesWithLight = new List<MeshLightData>();
            foreach (var mesh in bumpMeshes)
            {
                var meshData = new MeshLightData();
                meshData.mesh = mesh;
                var meshCeter = mesh.BoundingBox.calculateBoxCenter();
                meshData.lights[0] = getClosestLight(meshCeter, null, null);
                meshData.lights[1] = getClosestLight(meshCeter, meshData.lights[0], null);
                meshData.lights[2] = getClosestLight(meshCeter, meshData.lights[0], meshData.lights[1]);
                meshesWithLight.Add(meshData);
            }

            //Camara en 1ra persona
            Camara = new TgcFpsCamera(new TGCVector3(0, 50, 100), Input);

            //Modifiers
            Modifiers.addBoolean("lightEnable", "lightEnable", true);
            Modifiers.addFloat("reflection", 0, 1, 0.2f);
            Modifiers.addFloat("bumpiness", 0, 2, 1f);
            Modifiers.addFloat("lightIntensity", 0, 150, 20);
            Modifiers.addFloat("lightAttenuation", 0.1f, 2, 0.3f);
            Modifiers.addFloat("specularEx", 0, 20, 9f);

            Modifiers.addColor("mEmissive", Color.Black);
            Modifiers.addColor("mAmbient", Color.White);
            Modifiers.addColor("mDiffuse", Color.White);
            Modifiers.addColor("mSpecular", Color.White);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Habilitar luz
            var lightEnable = (bool)Modifiers["lightEnable"];
            Effect currentShader;
            string currentTechnique;
            if (lightEnable)
            {
                //Shader personalizado de iluminacion
                currentShader = effect;
                currentTechnique = "ThreeLightsTechnique";
            }
            else
            {
                //Sin luz: Restaurar shader default
                currentShader = TgcShaders.Instance.TgcMeshShader;
                currentTechnique = TgcShaders.Instance.getTgcMeshTechnique(TgcMesh.MeshRenderType.DIFFUSE_MAP);
            }

            //Aplicar a cada mesh el shader actual
            foreach (var meshData in meshesWithLight)
            {
                meshData.mesh.Effect = currentShader;
                meshData.mesh.Technique = currentTechnique;
            }

            var eyePosition = Camara.Position;

            //Renderizar meshes con BumpMapping
            foreach (var meshData in meshesWithLight)
            {
                var mesh = meshData.mesh;

                if (true) //FIXME da error cuando se desabilitan las luces.) (lightEnable)
                {
                    mesh.Effect.SetValue("eyePosition", TGCVector3.Vector3ToFloat4Array(eyePosition));
                    mesh.Effect.SetValue("bumpiness", (float)Modifiers["bumpiness"]);
                    mesh.Effect.SetValue("reflection", (float)Modifiers["reflection"]);

                    //Cargar variables de shader del Material
                    mesh.Effect.SetValue("materialEmissiveColor",
                        ColorValue.FromColor((Color)Modifiers["mEmissive"]));
                    mesh.Effect.SetValue("materialAmbientColor",
                        ColorValue.FromColor((Color)Modifiers["mAmbient"]));
                    mesh.Effect.SetValue("materialDiffuseColor",
                        ColorValue.FromColor((Color)Modifiers["mDiffuse"]));
                    mesh.Effect.SetValue("materialSpecularColor",
                        ColorValue.FromColor((Color)Modifiers["mSpecular"]));
                    mesh.Effect.SetValue("materialSpecularExp", (float)Modifiers["specularEx"]);

                    //CubeMap
                    mesh.Effect.SetValue("texCubeMap", cubeMap);

                    //Cargar variables de shader de las 3 luces
                    //Intensidad y atenuacion deberian ser atributos propios de cada luz
                    var lightIntensity = (float)Modifiers["lightIntensity"];
                    var lightAttenuation = (float)Modifiers["lightAttenuation"];
                    mesh.Effect.SetValue("lightIntensity", new[] { lightIntensity, lightIntensity, lightIntensity });
                    mesh.Effect.SetValue("lightAttenuation",
                        new[] { lightAttenuation, lightAttenuation, lightAttenuation });

                    mesh.Effect.SetValue("lightColor",
                        new[]
                        {
                            ColorValue.FromColor(meshData.lights[0].color),
                            ColorValue.FromColor(meshData.lights[1].color),
                            ColorValue.FromColor(meshData.lights[2].color)
                        });
                    mesh.Effect.SetValue("lightPosition",
                        new[]
                        {
                            TGCVector3.Vector3ToVector4(meshData.lights[0].pos),
                            TGCVector3.Vector3ToVector4(meshData.lights[1].pos),
                            TGCVector3.Vector3ToVector4(meshData.lights[2].pos)
                        });
                }

                //Renderizar modelo
                mesh.render();
            }

            //Renderizar meshes comunes
            foreach (var mesh in commonMeshes)
            {
                mesh.Render();
            }

            PostRender();
        }

        /// <summary>
        ///     Devuelve la luz mas cercana a la posicion especificada
        /// </summary>
        private LightData getClosestLight(TGCVector3 pos, LightData ignore1, LightData ignore2)
        {
            var minDist = float.MaxValue;
            LightData minLight = null;

            foreach (var light in lights)
            {
                //Ignorar las luces indicadas
                if (ignore1 != null && light.Equals(ignore1))
                    continue;
                if (ignore2 != null && light.Equals(ignore2))
                    continue;

                var distSq = TGCVector3.LengthSq(pos - light.pos);
                if (distSq < minDist)
                {
                    minDist = distSq;
                    minLight = light;
                }
            }

            return minLight;
        }

        public override void Dispose()
        {
            effect.Dispose();
            foreach (var meshData in meshesWithLight)
            {
                meshData.mesh.Dispose();
            }
            foreach (var m in commonMeshes)
            {
                m.Dispose();
            }
            cubeMap.Dispose();
        }

        /// <summary>
        ///     Estructura auxiliar para informacion de luces
        /// </summary>
        public class LightData
        {
            public TgcBoundingAxisAlignBox aabb;
            public Color color;
            public TGCVector3 pos;
        }

        /// <summary>
        ///     Estructura auxiliar para guardar un mesh y sus tres luces mas cercanas
        /// </summary>
        public class MeshLightData
        {
            public LightData[] lights = new LightData[3];
            public TgcMeshBumpMapping mesh;
        }
    }
}