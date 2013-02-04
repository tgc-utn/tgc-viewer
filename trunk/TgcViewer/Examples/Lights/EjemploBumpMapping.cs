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
    /// Ejemplo EjemploBumpMapping:
    /// Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminaci�n - Iluminaci�n din�mica
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    /// 
    /// Ejemplo avanzado. Ver primero ejemplo "Lights/EjemploPointLight"
    /// 
    /// Muestra como dibujar un mesh con efecto de BumpMapping, utilizando iluminaci�n din�mica con una luz PointLight
    /// BumpMapping no usa la normal de cada v�rtice para iluminar, sino que utiliza una textura auxiliar denominada NormalMap.
    /// El NormalMap es una textura que cada pixel RGB corresponde a un vector normal (XYZ)
    /// Crear un buen NormalMap para un mesh es una tarea de dise�o. Existen herramientas para generar autom�ticamente un NormalMap
    /// a partir de una textura. Ejemplo: http://sourceforge.net/projects/ssbumpgenerator/
    /// Pero no se logra el mismo resultado que con una textura hecha por un artista.
    /// 
    /// 
    /// Autor: Mat�as Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploBumpMapping : TgcExample
    {
        Effect effect;
        TgcBox lightMesh;
        List<TgcMeshBumpMapping> meshes;
        List<TgcArrow> tangents;
        List<TgcArrow> normals;
        List<TgcArrow> binormals;


        public override string getCategory()
        {
            return "Lights";
        }

        public override string getName()
        {
            return "Bump Mapping";
        }

        public override string getDescription()
        {
            return "Efecto de Bump Mapping utilizando una textura de NormalMap en TangentSpace";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;


            //DEBUG: para probar codigo que genera un NormalMap automaticamente. Queda bastante peor que el NormalMap que ya viene hecho
            //createNormalMap(GuiController.Instance.ExamplesMediaDir + "Shaders\\BumpMapping_DiffuseMap.jpg", GuiController.Instance.ExamplesMediaDir + "Shaders\\NormalMap_Prueba.jpg");
            //TgcTexture normalMap = TgcTexture.createTexture(GuiController.Instance.ExamplesMediaDir + "Shaders\\NormalMap_Prueba2.jpg");





            //Crear 3 paredes y un piso con textura comun y textura de normalMap
            TgcTexture diffuseMap = TgcTexture.createTexture(GuiController.Instance.ExamplesMediaDir + "Shaders\\BumpMapping_DiffuseMap.jpg");
            TgcTexture normalMap = TgcTexture.createTexture(GuiController.Instance.ExamplesMediaDir + "Shaders\\BumpMapping_NormalMap.jpg");
            TgcTexture[] normalMapArray = new TgcTexture[] { normalMap };

            TgcBox paredSur = TgcBox.fromExtremes(new Vector3(-200, 0, -210), new Vector3(200, 100, -200), diffuseMap);
            TgcBox paredOeste = TgcBox.fromExtremes(new Vector3(-210, 0, -200), new Vector3(-200, 100, 200), diffuseMap);
            TgcBox paredEste = TgcBox.fromExtremes(new Vector3(200, 0, -200), new Vector3(210, 100, 200), diffuseMap);
            TgcBox piso = TgcBox.fromExtremes(new Vector3(-200, -1, -200), new Vector3(200, 0, 200), diffuseMap);

            //Convertir TgcBox a TgcMesh
            TgcMesh m1 = paredSur.toMesh("paredSur");
            TgcMesh m2 = paredOeste.toMesh("paredOeste");
            TgcMesh m3 = paredEste.toMesh("paredEste");
            TgcMesh m4 = piso.toMesh("piso");

            //Convertir TgcMesh a TgcMeshBumpMapping
            meshes = new List<TgcMeshBumpMapping>();
            meshes.Add(TgcMeshBumpMapping.fromTgcMesh(m1, normalMapArray));
            meshes.Add(TgcMeshBumpMapping.fromTgcMesh(m2, normalMapArray));
            meshes.Add(TgcMeshBumpMapping.fromTgcMesh(m3, normalMapArray));
            meshes.Add(TgcMeshBumpMapping.fromTgcMesh(m4, normalMapArray));

            
            //Borrar TgcMesh y TgcBox, ya no se usan
            paredSur.dispose();
            paredOeste.dispose();
            paredEste.dispose();
            piso.dispose();
            m1.dispose();
            m2.dispose();
            m3.dispose();
            m4.dispose();
            

            //Crear flechas de debug
            tangents = new List<TgcArrow>();
            normals = new List<TgcArrow>();
            binormals = new List<TgcArrow>();
            foreach (TgcMeshBumpMapping mesh in meshes)
            {
                loadDebugArrows(mesh);
            }
 

            
            //Camara en 1ra persona
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(0, 50, 100), new Vector3(0, 50, -1));

            //Cargar Shader de personalizado de BumpMapping. Solo soporta meshes de tipo DiffuseMap
            effect = TgcShaders.loadEffect(GuiController.Instance.ExamplesMediaDir + "Shaders\\BumpMapping.fx");

            //Cargar shader en meshes
            foreach (TgcMeshBumpMapping m in meshes)
            {
                m.Effect = effect;
                m.Technique = "BumpMappingTechnique";
            }
            
            //Mesh para la luz
            lightMesh = TgcBox.fromSize(new Vector3(10, 10, 10), Color.Red);
            GuiController.Instance.Modifiers.addFloat("bumpiness", 0, 1, 1f);
            GuiController.Instance.Modifiers.addVertex3f("lightPos", new Vector3(-200, 0, -200), new Vector3(200, 100, 200), new Vector3(0, 80, 0));
            GuiController.Instance.Modifiers.addColor("lightColor", Color.White);
            GuiController.Instance.Modifiers.addFloat("lightIntensity", 0, 150, 20);
            GuiController.Instance.Modifiers.addFloat("lightAttenuation", 0.1f, 2, 0.3f);
            GuiController.Instance.Modifiers.addFloat("specularEx", 0, 20, 9f);
            GuiController.Instance.Modifiers.addBoolean("showNormals", "showNormals", false);
            GuiController.Instance.Modifiers.addBoolean("showTangents", "showTangents", false);
            GuiController.Instance.Modifiers.addBoolean("showBinormals", "showBinormals", false);
            

            GuiController.Instance.Modifiers.addColor("mEmissive", Color.Black);
            GuiController.Instance.Modifiers.addColor("mAmbient", Color.White);
            GuiController.Instance.Modifiers.addColor("mDiffuse", Color.White);
            GuiController.Instance.Modifiers.addColor("mSpecular", Color.White);
        }

        /// <summary>
        /// Crear flechas de debug para normales, tangentes y binormales
        /// </summary>
        private void loadDebugArrows(TgcMeshBumpMapping mesh)
        {
            //Obtener vertexBuffer
            TgcMeshBumpMapping.BumpMappingVertex[] vertexBuffer = (TgcMeshBumpMapping.BumpMappingVertex[])mesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcMeshBumpMapping.BumpMappingVertex), LockFlags.ReadOnly, mesh.D3dMesh.NumberVertices);
            mesh.D3dMesh.UnlockVertexBuffer();

            for (int i = 0; i < vertexBuffer.Length; i++)
            {
                TgcMeshBumpMapping.BumpMappingVertex v = vertexBuffer[i];
                normals.Add(TgcArrow.fromDirection(v.Position, v.Normal * 50, Color.Blue, Color.Yellow, 0.5f, new Vector2(2f, 4f)));
                tangents.Add(TgcArrow.fromDirection(v.Position, v.Tangent * 50, Color.Red, Color.Yellow, 0.5f, new Vector2(2f, 4f)));
                binormals.Add(TgcArrow.fromDirection(v.Position, v.Binormal * 50, Color.Green, Color.Yellow, 0.5f, new Vector2(2f, 4f)));
            }

        }

        


        public override void render(float elapsedTime)
        {
            Device device = GuiController.Instance.D3dDevice;

            //Actualzar posici�n de la luz
            Vector3 lightPos = (Vector3)GuiController.Instance.Modifiers["lightPos"];
            lightMesh.Position = lightPos;
            Vector3 eyePosition = GuiController.Instance.FpsCamera.getPosition();

            //Renderizar meshes
            foreach (TgcMeshBumpMapping mesh in meshes)
            {
                //Cargar variables shader de la luz
                mesh.Effect.SetValue("lightColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["lightColor"]));
                mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(eyePosition));
                mesh.Effect.SetValue("lightIntensity", (float)GuiController.Instance.Modifiers["lightIntensity"]);
                mesh.Effect.SetValue("lightAttenuation", (float)GuiController.Instance.Modifiers["lightAttenuation"]);
                mesh.Effect.SetValue("bumpiness", (float)GuiController.Instance.Modifiers["bumpiness"]);
            
                //Material
                mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mEmissive"]));
                mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mAmbient"]));
                mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mDiffuse"]));
                mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mSpecular"]));
                mesh.Effect.SetValue("materialSpecularExp", (float)GuiController.Instance.Modifiers["specularEx"]);

                //Renderizar modelo
                mesh.render();
            }


            //Renderizar mesh de luz
            lightMesh.render();


            //Dibujar flechas de debug
            bool showNormals = (bool)GuiController.Instance.Modifiers["showNormals"];
            bool showTangents = (bool)GuiController.Instance.Modifiers["showTangents"];
            bool showBinormals = (bool)GuiController.Instance.Modifiers["showBinormals"];
            for (int i = 0; i < normals.Count; i++)
            {
                if (showNormals) normals[i].render();
                if (showTangents) tangents[i].render();
                if (showBinormals) binormals[i].render();
            }

        }


        /// <summary>
        /// Crea un bitmap de NormalMap en base a un DiffuseMap.
        /// Es experimental.
        /// El resultado aun deja mucho que desear.
        /// </summary>
        /// <param name="diffuseMapPath">Path del diffuseMap</param>
        /// <param name="outputFileName">Path del normalMap a generar</param>
        public void createNormalMap(string diffuseMapPath, string outputFileName)
        {
            //Cargar diffuseMap
            Bitmap bitmap = (Bitmap)Bitmap.FromFile(diffuseMapPath);

            //Convertir a escala de grises
            float heightScale = 100f;
            float[,] heightmap = new float[bitmap.Width, bitmap.Height];
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    Color c = bitmap.GetPixel(i, j);
                    //Greyscale
                    heightmap[i, j] = ((0.299f * c.R + 0.587f * c.G + 0.114f * c.B) * heightScale) / 255f;
                }
            }

            /*
            //Aplicar Bilateral Filtering para quitar irregularidades innecesarias
            BilateralFiltering filter = new BilateralFiltering(heightmap, 4, 4);
            heightmap = filter.runFilter();
            */

            //Cargar datos para normalMap
            Bitmap normalMap = new Bitmap(bitmap.Width, bitmap.Height);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    //Tomar pixel actual y el vecino derecho y abajo
                    float current = heightmap[i, j];
                    int nextX = i + 1 < bitmap.Width ? i + 1 : i;
                    int nextY = j - 1 >= 0 ? j - 1 : 0;
                    float right = heightmap[nextX, j];
                    float up = heightmap[i, nextY];

                    //Calcular 2 vectores entre los 3 puntos
                    Vector3 e1 = new Vector3(1, 0, right - current);
                    Vector3 e2 = new Vector3(0, 1, up - current);

                    //Obtener normal con cross product
                    Vector3 normal = Vector3.Cross(e1, e2);
                    normal.Normalize();

                    //Pasar a rango [0, 1]
                    normal = new Vector3(0.5f, 0.5f, 0.5f) + normal * 0.5f;

                    //Convertir a color ARGB
                    Color c = Color.FromArgb(255, (byte)(normal.X * 255), (byte)(normal.Y * 255), (byte)(normal.Z * 255));
                    normalMap.SetPixel(i, j, c);
                }
            }
            normalMap.Save(outputFileName);

            bitmap.Dispose();
            normalMap.Dispose();
        }



        public override void close()
        {
            effect.Dispose();
            foreach (TgcMeshBumpMapping m in meshes)
            {
                m.dispose();
            }
            for (int i = 0; i < normals.Count; i++)
            {
                normals[i].dispose();
                tangents[i].dispose();
                binormals[i].dispose();
            }
            lightMesh.dispose();
        }



    }

    

}
