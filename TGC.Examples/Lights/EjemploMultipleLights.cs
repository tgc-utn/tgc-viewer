using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;

namespace TGC.Examples.Lights
{
    /// <summary>
    ///     Ejemplo EjemploMultipleLights:
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminación - Iluminación dinámica
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "Lights/EjemploPointLight"
    ///     Muestra como aplicar iluminación dinámica con PhongShading por pixel en un Pixel Shader, para un tipo
    ///     de luz "Point Light".
    ///     Permite una única luz por objeto.
    ///     El escenario cuenta con muchas luces.
    ///     Cada objeto solo se ve influenciado por la luz mas cercana.
    ///     Calcula todo el modelo de iluminación completo (Ambient, Diffuse, Specular)
    ///     Las luces poseen atenuación por la distancia.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploMultipleLights : TgcExample
    {
        private List<LightData> lights;
        private TgcScene scene;

        public EjemploMultipleLights(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Lights";
            Name = "Multiple Lights";
            Description = "Escenario con muchas luces dinamicas.";
        }

        public override void Init()
        {
            //Cargar escenario, pero inicialmente solo hacemos el parser, para separar los objetos que son solo luces y no meshes
            var scenePath = MediaDir + "Escenario\\EscenarioLuces-TgcScene.xml";
            var mediaPath = MediaDir + "Escenario\\";
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
            scene = loader.loadScene(sceneData, mediaPath);

            //Camara en 1ra persona
            Camara = new TgcFpsCamera(new Vector3(-20, 80, 450), 400f, 300f);

            //Modifiers para variables de luz
            Modifiers.addBoolean("lightEnable", "lightEnable", true);
            Modifiers.addFloat("lightIntensity", 0, 150, 20);
            Modifiers.addFloat("lightAttenuation", 0.1f, 2, 0.3f);
            Modifiers.addFloat("specularEx", 0, 20, 9f);

            //Modifiers para material
            Modifiers.addColor("mEmissive", Color.Black);
            Modifiers.addColor("mAmbient", Color.White);
            Modifiers.addColor("mDiffuse", Color.White);
            Modifiers.addColor("mSpecular", Color.White);
        }

        public override void Update()
        {
            base.helperPreUpdate();
        }

        public override void Render()
        {
            base.helperPreRender();
            
            //Habilitar luz
            var lightEnable = (bool)Modifiers["lightEnable"];
            Effect currentShader;
            if (lightEnable)
            {
                //Con luz: Cambiar el shader actual por el shader default que trae el framework para iluminacion dinamica con PointLight
                currentShader = TgcShaders.Instance.TgcMeshPointLightShader;
            }
            else
            {
                //Sin luz: Restaurar shader default
                currentShader = TgcShaders.Instance.TgcMeshShader;
            }

            //Aplicar a cada mesh el shader actual
            foreach (var mesh in scene.Meshes)
            {
                mesh.Effect = currentShader;
                //El Technique depende del tipo RenderType del mesh
                mesh.Technique = TgcShaders.Instance.getTgcMeshTechnique(mesh.RenderType);
            }

            //Renderizar meshes
            foreach (var mesh in scene.Meshes)
            {
                if (lightEnable)
                {
                    /* Obtener la luz que corresponde a este mesh.
                 * En este escenario a un mesh lo pueden estar iluminando varias luces a la vez.
                 * Cuando se tienen varias luces en un escenario surge el problema de "Many-Light shader"
                 * No existe una única forma de resolverlo. Algunas alternativas son:
                 *  1) Cada mesh solo se ilumina por una unica luz. Aunque haya varias
                 *  2) El shader se programa para que soporte 2 o mas luces.
                 *  3) Se hacen varias pasadas, renderizando una vez con cada luz
                 *  4) Se utiliza la tecnica de Deferred Shading. Ver: http://ogldev.atspace.co.uk/www/tutorial35/tutorial35.html
                 *
                 * En este ejemplo se utiliza la tecnica 1, por ser la mas simple. Para cada objeto se usa solo la luz mas cercana.
                 * El problema es que un mesh solo se ve influenciado por una única luz.
                 * Para un mesh chico no es muy problemático. Pero para un mesh largo, como una pared, se nota bastante.
                 * Si la pared tiene 3 luces, solo una va a contribuir en la iluminación, las otras dos no.
                 * Se podria usar otro criterio distinto que cercania. Cada mesh podria tener asignada una luz segun el cuarto en el que se encuentra o
                 * segun algun otro criterio de diseño.
                 * Si es por cercania y hay muchas luces lo mejor armar una grilla o octree offline que indique que luz corresponde a cada celda.
                 * En este ejemplo en cambio por cada mesh se sale a recorrer todas las luces que hay para ver las mas cercana (no es para nada optimo)
                 * Como posicion del mesh se toma el centro del AABB. En este ejemplo el centro se calcula en cada cuadro. Deberia estar precalculado para
                 * todos los meshes.
                 *
                 * La tecnica 2 requiere hacer todas la cuentas de iluminacion N veces en el shader (N dot L, N dot H, etc, una vez por cada luz).
                 * El problema de esto es que los calculos del shader se pueden volver bastante costosos.
                 * Muchas aplicaciones definen una cantidad X de luces, por ejemplo hasta 3. Y el shader hace los calculos 3 veces.
                 * Otros aplicaciones permiten una luz principal con todo el modelo de iluminacion (ambiente + diffuse + specular) y otras dos luces
                 * mas sencillas (por ejemplo solo diffuse). Para asi reducer los calculos.
                 * Ver presentacion de "Physically-based lighting in Call of Duty: Black Ops" en http://advances.realtimerendering.com/s2011/index.html
                 *
                 * La tecnica 3 permite usar el mismo shader sin hacerlo mas complejo, pero requiere renderizar varias veces el mismo mesh.
                 * Si el mesh tiene muchos triangulos puede ser bastante costoso dibujarlo varias veces.
                 *
                 * La tecnica 4 es a la que muchas aplicaciones están tendiendo hoy en dia. Pero es compleja, requiere que la GPU soporte un modelo
                 * de Pixel Shader bastante nuevo, y consume bastante memoria de GPU.
                 *
                 */
                    var light = getClosestLight(mesh.BoundingBox.calculateBoxCenter());

                    //Cargar variables shader de la luz
                    mesh.Effect.SetValue("lightColor", ColorValue.FromColor(light.color));
                    mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(light.pos));
                    mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(Camara.Position));
                    mesh.Effect.SetValue("lightIntensity", (float)Modifiers["lightIntensity"]);
                    mesh.Effect.SetValue("lightAttenuation", (float)Modifiers["lightAttenuation"]);

                    //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
                    mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor((Color)Modifiers["mEmissive"]));
                    mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor((Color)Modifiers["mAmbient"]));
                    mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor((Color)Modifiers["mDiffuse"]));
                    mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor((Color)Modifiers["mSpecular"]));
                    mesh.Effect.SetValue("materialSpecularExp", (float)Modifiers["specularEx"]);
                }

                //Renderizar modelo
                mesh.render();

            }
            helperPostRender();
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

        public override void Dispose()
        {
            

            scene.disposeAll();
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