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

namespace Examples.Lights
{
    /// <summary>
    /// Ejemplo EjemploMultipleLights:
    /// Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminación - Iluminación dinámica
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    /// 
    /// Ejemplo avanzado. Ver primero ejemplo "Lights/EjemploPointLight"
    /// 
    /// Muestra como aplicar iluminación dinámica con PhongShading por pixel en un Pixel Shader, para un tipo
    /// de luz "Point Light".
    /// Permite una única luz por objeto.
    /// El escenario cuenta con muchas luces.
    /// Cada objeto solo se ve influenciado por la luz mas cercana.
    /// Calcula todo el modelo de iluminación completo (Ambient, Diffuse, Specular)
    /// Las luces poseen atenuación por la distancia.
    /// 
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploMultipleLights : TgcExample
    {
        TgcScene scene;
        List<LightData> lights;


        public override string getCategory()
        {
            return "Lights";
        }

        public override string getName()
        {
            return "Multiple Lights";
        }

        public override string getDescription()
        {
            return "Escenario con muchas luces dinamicas";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Cargar escenario, pero inicialmente solo hacemos el parser, para separar los objetos que son solo luces y no meshes
            string scenePath = GuiController.Instance.ExamplesDir + "Lights\\Escenario\\EscenarioLuces-TgcScene.xml";
            string mediaPath = GuiController.Instance.ExamplesDir + "Lights\\Escenario\\";
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
            scene = loader.loadScene(sceneData, mediaPath);



            //Camara en 1ra persona
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.MovementSpeed = 400f;
            GuiController.Instance.FpsCamera.JumpSpeed = 300f;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-20, 80, 450), new Vector3(0, 80, 1));


            //Modifiers para variables de luz
            GuiController.Instance.Modifiers.addBoolean("lightEnable", "lightEnable", true);
            GuiController.Instance.Modifiers.addFloat("lightIntensity", 0, 150, 20);
            GuiController.Instance.Modifiers.addFloat("lightAttenuation", 0.1f, 2, 0.3f);
            GuiController.Instance.Modifiers.addFloat("specularEx", 0, 20, 9f);

            //Modifiers para material
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
            if (lightEnable)
            {
                //Con luz: Cambiar el shader actual por el shader default que trae el framework para iluminacion dinamica con PointLight
                currentShader = GuiController.Instance.Shaders.TgcMeshPointLightShader;
            }
            else
            {
                //Sin luz: Restaurar shader default
                currentShader = GuiController.Instance.Shaders.TgcMeshShader;
            }

            //Aplicar a cada mesh el shader actual
            foreach (TgcMesh mesh in scene.Meshes)
            {
                mesh.Effect = currentShader;
                //El Technique depende del tipo RenderType del mesh
                mesh.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(mesh.RenderType);
            }


            //Renderizar meshes
            foreach (TgcMesh mesh in scene.Meshes)
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
                    LightData light = getClosestLight(mesh.BoundingBox.calculateBoxCenter());

                    //Cargar variables shader de la luz
                    mesh.Effect.SetValue("lightColor", ColorValue.FromColor(light.color));
                    mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(light.pos));
                    mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(GuiController.Instance.FpsCamera.getPosition()));
                    mesh.Effect.SetValue("lightIntensity", (float)GuiController.Instance.Modifiers["lightIntensity"]);
                    mesh.Effect.SetValue("lightAttenuation", (float)GuiController.Instance.Modifiers["lightAttenuation"]);

                    //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
                    mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mEmissive"]));
                    mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mAmbient"]));
                    mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mDiffuse"]));
                    mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mSpecular"]));
                    mesh.Effect.SetValue("materialSpecularExp", (float)GuiController.Instance.Modifiers["specularEx"]);
                }
                


                //Renderizar modelo
                mesh.render();
            }

        }

        /// <summary>
        /// Devuelve la luz mas cercana a la posicion especificada
        /// </summary>
        private LightData getClosestLight(Vector3 pos)
        {
            float minDist = float.MaxValue;
            LightData minLight = null;

            foreach (LightData light in lights)
            {
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
            scene.disposeAll();
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


    }

    

}
