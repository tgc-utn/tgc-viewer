using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TGC.Core.BoundingVolumes;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.Lights
{
    /// <summary>
    ///     Ejemplo EjemploIntegrador:
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminacion - Iluminacion dinamica
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
    ///     Todos los meshes del layer BumpMap tienen una textura de NormalMap. Por convencion se llama:
    ///     NOMBRE_ORIGINAL_NormalMap.png
    ///     Todos los meshes se ven afectados por un unico CubeMap.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploIntegrador : TGCExampleViewer
    {
        private TGCFloatModifier reflectionModifier;
        private TGCFloatModifier bumpinessModifier;
        private TGCFloatModifier lightIntensityModifier;
        private TGCFloatModifier lightAttenuationModifier;
        private TGCFloatModifier specularExModifier;
        private TGCColorModifier mEmissiveModifier;
        private TGCColorModifier mAmbientModifier;
        private TGCColorModifier mDiffuseModifier;
        private TGCColorModifier mSpecularModifier;

        private List<TgcMeshBumpMapping> bumpMeshes;
        private List<TgcMesh> commonMeshes;
        private CubeTexture cubeMap;
        private Effect effect;
        private List<LightData> lights;

        public EjemploIntegrador(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Pixel y Vertex Shaders";
            Name = "BumpMap + EnvMap + 1 Point Light por Proximidad";
            Description = "Ejemplo que muestra un escenario con BumpMapping, EnvironmentMap y varias Point Lights.";
        }

        public override void Init()
        {
            //Cargar textura de CubeMap para Environment Map, fijo para todos los meshes
            cubeMap = TextureLoader.FromCubeFile(D3DDevice.Instance.Device, MediaDir + "CubeMap.dds");

            //Cargar Shader personalizado de EnvironmentMap
            effect = TGCShaders.Instance.LoadEffect(ShadersDir + "EnvironmentMap.fx");

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
                    light.color = Color.FromArgb((int)meshData.color[0], (int)meshData.color[1], (int)meshData.color[2]);
                    light.aabb = new TgcBoundingAxisAlignBox(TGCVector3.Float3ArrayToTGCVector3(meshData.pMin), TGCVector3.Float3ArrayToTGCVector3(meshData.pMax));
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
                    mesh.Dispose();
                }
                //Mesh normal
                else
                {
                    commonMeshes.Add(mesh);
                }
            }

            //Camara en 1ra persona
            Camara = new TgcFpsCamera(new TGCVector3(0, 50, 100), Input);

            //Modifiers
            reflectionModifier = AddFloat("reflection", 0, 1, 0.2f);
            bumpinessModifier = AddFloat("bumpiness", 0, 2, 1f);
            lightIntensityModifier = AddFloat("lightIntensity", 0, 150, 20);
            lightAttenuationModifier = AddFloat("lightAttenuation", 0.1f, 2, 0.3f);
            specularExModifier = AddFloat("specularEx", 0, 20, 9f);

            mEmissiveModifier = AddColor("mEmissive", Color.Black);
            mAmbientModifier = AddColor("mAmbient", Color.White);
            mDiffuseModifier = AddColor("mDiffuse", Color.White);
            mSpecularModifier = AddColor("mSpecular", Color.White);
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            Effect currentShader;
            string currentTechnique;

            //Shader personalizado de iluminacion
            currentShader = effect;
            currentTechnique = "EnvironmentMapTechnique";

            //Aplicar a cada mesh el shader actual
            foreach (TgcMesh mesh in bumpMeshes)
            {
                mesh.Effect = currentShader;
                mesh.Technique = currentTechnique;
            }

            var eyePosition = Camara.Position;

            //Renderizar meshes con BumpMapping
            foreach (var mesh in bumpMeshes)
            {
                if (true) //FIXME da error cuando se desabilitan las luces.) (lightEnable)
                {
                    //Obtener la luz que corresponde a este mesh (buscamos la mas cercana)
                    var light = getClosestLight(mesh.BoundingBox.calculateBoxCenter());

                    mesh.Effect.SetValue("eyePosition", TGCVector3.TGCVector3ToFloat4Array(eyePosition));
                    mesh.Effect.SetValue("bumpiness", bumpinessModifier.Value);
                    mesh.Effect.SetValue("reflection", reflectionModifier.Value);
                    mesh.Effect.SetValue("lightIntensity", lightIntensityModifier.Value);
                    mesh.Effect.SetValue("lightAttenuation", lightAttenuationModifier.Value);

                    //Cargar variables de shader de la luz
                    mesh.Effect.SetValue("lightColor", ColorValue.FromColor(light.color));
                    mesh.Effect.SetValue("lightPosition", TGCVector3.TGCVector3ToFloat4Array(light.pos));

                    //Cargar variables de shader del Material
                    mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(mEmissiveModifier.Value));
                    mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(mAmbientModifier.Value));
                    mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(mDiffuseModifier.Value));
                    mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(mSpecularModifier.Value));
                    mesh.Effect.SetValue("materialSpecularExp", specularExModifier.Value);

                    //CubeMap
                    mesh.Effect.SetValue("texCubeMap", cubeMap);
                }

                mesh.UpdateMeshTransform();
                //Renderizar modelo
                mesh.Render();
            }

            //Renderizar meshes comunes
            foreach (var mesh in commonMeshes)
            {
                mesh.UpdateMeshTransform();
                mesh.Render();
            }

            PostRender();
        }

        /// <summary>
        ///     Devuelve la luz mas cercana a la posicion especificada
        /// </summary>
        private LightData getClosestLight(TGCVector3 pos)
        {
            var minDist = float.MaxValue;
            LightData minLight = null;

            foreach (var light in lights)
            {
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
            foreach (var m in bumpMeshes)
            {
                m.Dispose();
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
    }
}