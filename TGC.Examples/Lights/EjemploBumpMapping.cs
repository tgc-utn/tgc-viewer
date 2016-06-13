using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;

namespace TGC.Examples.Lights
{
    /// <summary>
    ///     Ejemplo EjemploBumpMapping:
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminación - Iluminación dinámica
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "Lights/EjemploPointLight"
    ///     Muestra como dibujar un mesh con efecto de BumpMapping, utilizando iluminación dinámica con una luz PointLight
    ///     BumpMapping no usa la normal de cada vértice para iluminar, sino que utiliza una textura auxiliar denominada
    ///     NormalMap.
    ///     El NormalMap es una textura que cada pixel RGB corresponde a un vector normal (XYZ)
    ///     Crear un buen NormalMap para un mesh es una tarea de diseño. Existen herramientas para generar automáticamente un
    ///     NormalMap a partir de una textura. Ejemplo: http://sourceforge.net/projects/ssbumpgenerator/
    ///     Pero no se logra el mismo resultado que con una textura hecha por un artista.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploBumpMapping : TgcExample
    {
        private List<TgcArrow> binormals;
        private Effect effect;
        private TgcBox lightMesh;
        private List<TgcMeshBumpMapping> meshes;
        private List<TgcArrow> normals;
        private List<TgcArrow> tangents;

        public EjemploBumpMapping(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Lights";
            Name = "Bump Mapping";
            Description = "Efecto de Bump Mapping utilizando una textura de NormalMap en TangentSpace.";
        }

        public override void Init()
        {
            //DEBUG: para probar codigo que genera un NormalMap automaticamente. Queda bastante peor que el NormalMap que ya viene hecho
            //createNormalMap(this.ShadersDir+"BumpMapping_DiffuseMap.jpg", this.ShadersDir+"NormalMap_Prueba.jpg");
            //TgcTexture normalMap = TgcTexture.createTexture(this.ShadersDir+"NormalMap_Prueba2.jpg");

            //Crear 3 paredes y un piso con textura comun y textura de normalMap
            var diffuseMap =
                TgcTexture.createTexture(MediaDir + "Texturas//BM_DiffuseMap_pared.jpg");
            var normalMap =
                TgcTexture.createTexture(MediaDir + "Texturas//BM_NormalMap.jpg");
            TgcTexture[] normalMapArray = { normalMap };

            var paredSur = TgcBox.fromExtremes(new Vector3(-200, 0, -210), new Vector3(200, 100, -200), diffuseMap);
            var paredOeste = TgcBox.fromExtremes(new Vector3(-210, 0, -200), new Vector3(-200, 100, 200), diffuseMap);
            var paredEste = TgcBox.fromExtremes(new Vector3(200, 0, -200), new Vector3(210, 100, 200), diffuseMap);
            var piso = TgcBox.fromExtremes(new Vector3(-200, -1, -200), new Vector3(200, 0, 200), diffuseMap);

            //Convertir TgcBox a TgcMesh
            var m1 = paredSur.toMesh("paredSur");
            var m2 = paredOeste.toMesh("paredOeste");
            var m3 = paredEste.toMesh("paredEste");
            var m4 = piso.toMesh("piso");

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
            foreach (var mesh in meshes)
            {
                loadDebugArrows(mesh);
            }

            //Camara en 1ra persona
            Camara = new TgcFpsCamera(new Vector3(0, 50, 100));

            //Cargar Shader de personalizado de BumpMapping. Solo soporta meshes de tipo DiffuseMap
            effect = TgcShaders.loadEffect(ShadersDir + "BumpMapping.fx");

            //Cargar shader en meshes
            foreach (var m in meshes)
            {
                m.Effect = effect;
                m.Technique = "BumpMappingTechnique";
            }

            //Mesh para la luz
            lightMesh = TgcBox.fromSize(new Vector3(10, 10, 10), Color.Red);
            Modifiers.addFloat("bumpiness", 0, 1, 1f);
            Modifiers.addVertex3f("lightPos", new Vector3(-200, 0, -200),
                new Vector3(200, 100, 200), new Vector3(0, 80, 0));
            Modifiers.addColor("lightColor", Color.White);
            Modifiers.addFloat("lightIntensity", 0, 150, 20);
            Modifiers.addFloat("lightAttenuation", 0.1f, 2, 0.3f);
            Modifiers.addFloat("specularEx", 0, 20, 9f);
            Modifiers.addBoolean("showNormals", "showNormals", false);
            Modifiers.addBoolean("showTangents", "showTangents", false);
            Modifiers.addBoolean("showBinormals", "showBinormals", false);

            Modifiers.addColor("mEmissive", Color.Black);
            Modifiers.addColor("mAmbient", Color.White);
            Modifiers.addColor("mDiffuse", Color.White);
            Modifiers.addColor("mSpecular", Color.White);
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Crear flechas de debug para normales, tangentes y binormales
        /// </summary>
        private void loadDebugArrows(TgcMeshBumpMapping mesh)
        {
            //Obtener vertexBuffer
            var vertexBuffer = (TgcMeshBumpMapping.BumpMappingVertex[])mesh.D3dMesh.LockVertexBuffer(
                typeof(TgcMeshBumpMapping.BumpMappingVertex), LockFlags.ReadOnly, mesh.D3dMesh.NumberVertices);
            mesh.D3dMesh.UnlockVertexBuffer();

            for (var i = 0; i < vertexBuffer.Length; i++)
            {
                var v = vertexBuffer[i];
                normals.Add(TgcArrow.fromDirection(v.Position, v.Normal * 50, Color.Blue, Color.Yellow, 0.5f,
                    new Vector2(2f, 4f)));
                tangents.Add(TgcArrow.fromDirection(v.Position, v.Tangent * 50, Color.Red, Color.Yellow, 0.5f,
                    new Vector2(2f, 4f)));
                binormals.Add(TgcArrow.fromDirection(v.Position, v.Binormal * 50, Color.Green, Color.Yellow, 0.5f,
                    new Vector2(2f, 4f)));
            }
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            //Actualzar posición de la luz
            var lightPos = (Vector3)Modifiers["lightPos"];
            lightMesh.Position = lightPos;
            var eyePosition = Camara.Position;

            //Renderizar meshes
            foreach (var mesh in meshes)
            {
                //Cargar variables shader de la luz
                mesh.Effect.SetValue("lightColor",
                    ColorValue.FromColor((Color)Modifiers["lightColor"]));
                mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(eyePosition));
                mesh.Effect.SetValue("lightIntensity", (float)Modifiers["lightIntensity"]);
                mesh.Effect.SetValue("lightAttenuation", (float)Modifiers["lightAttenuation"]);
                mesh.Effect.SetValue("bumpiness", (float)Modifiers["bumpiness"]);

                //Material
                mesh.Effect.SetValue("materialEmissiveColor",
                    ColorValue.FromColor((Color)Modifiers["mEmissive"]));
                mesh.Effect.SetValue("materialAmbientColor",
                    ColorValue.FromColor((Color)Modifiers["mAmbient"]));
                mesh.Effect.SetValue("materialDiffuseColor",
                    ColorValue.FromColor((Color)Modifiers["mDiffuse"]));
                mesh.Effect.SetValue("materialSpecularColor",
                    ColorValue.FromColor((Color)Modifiers["mSpecular"]));
                mesh.Effect.SetValue("materialSpecularExp", (float)Modifiers["specularEx"]);

                //Renderizar modelo
                mesh.render();
            }

            //Renderizar mesh de luz
            lightMesh.render();

            //Dibujar flechas de debug
            var showNormals = (bool)Modifiers["showNormals"];
            var showTangents = (bool)Modifiers["showTangents"];
            var showBinormals = (bool)Modifiers["showBinormals"];
            for (var i = 0; i < normals.Count; i++)
            {
                if (showNormals) normals[i].render();
                if (showTangents) tangents[i].render();
                if (showBinormals) binormals[i].render();
            }

            FinalizarEscena();
        }

        /// <summary>
        ///     Crea un bitmap de NormalMap en base a un DiffuseMap.
        ///     Es experimental.
        ///     El resultado aun deja mucho que desear.
        /// </summary>
        /// <param name="diffuseMapPath">Path del diffuseMap</param>
        /// <param name="outputFileName">Path del normalMap a generar</param>
        public void createNormalMap(string diffuseMapPath, string outputFileName)
        {
            //Cargar diffuseMap
            var bitmap = (Bitmap)Image.FromFile(diffuseMapPath);

            //Convertir a escala de grises
            var heightScale = 100f;
            var heightmap = new float[bitmap.Width, bitmap.Height];
            for (var i = 0; i < bitmap.Width; i++)
            {
                for (var j = 0; j < bitmap.Height; j++)
                {
                    var c = bitmap.GetPixel(i, j);
                    //Greyscale
                    heightmap[i, j] = (0.299f * c.R + 0.587f * c.G + 0.114f * c.B) * heightScale / 255f;
                }
            }

            /*
            //Aplicar Bilateral Filtering para quitar irregularidades innecesarias
            BilateralFiltering filter = new BilateralFiltering(heightmap, 4, 4);
            heightmap = filter.runFilter();
            */

            //Cargar datos para normalMap
            var normalMap = new Bitmap(bitmap.Width, bitmap.Height);
            for (var i = 0; i < bitmap.Width; i++)
            {
                for (var j = 0; j < bitmap.Height; j++)
                {
                    //Tomar pixel actual y el vecino derecho y abajo
                    var current = heightmap[i, j];
                    var nextX = i + 1 < bitmap.Width ? i + 1 : i;
                    var nextY = j - 1 >= 0 ? j - 1 : 0;
                    var right = heightmap[nextX, j];
                    var up = heightmap[i, nextY];

                    //Calcular 2 vectores entre los 3 puntos
                    var e1 = new Vector3(1, 0, right - current);
                    var e2 = new Vector3(0, 1, up - current);

                    //Obtener normal con cross product
                    var normal = Vector3.Cross(e1, e2);
                    normal.Normalize();

                    //Pasar a rango [0, 1]
                    normal = new Vector3(0.5f, 0.5f, 0.5f) + normal * 0.5f;

                    //Convertir a color ARGB
                    var c = Color.FromArgb(255, (byte)(normal.X * 255), (byte)(normal.Y * 255), (byte)(normal.Z * 255));
                    normalMap.SetPixel(i, j, c);
                }
            }
            normalMap.Save(outputFileName);

            bitmap.Dispose();
            normalMap.Dispose();
        }

        public override void Close()
        {
            base.Close();

            effect.Dispose();
            foreach (var m in meshes)
            {
                m.dispose();
            }
            for (var i = 0; i < normals.Count; i++)
            {
                normals[i].dispose();
                tangents[i].dispose();
                binormals[i].dispose();
            }
            lightMesh.dispose();
        }
    }
}