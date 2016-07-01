using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;
using TGC.Examples.Example;

namespace TGC.Examples.Lights
{
    /// <summary>
    ///     Ejemplo EjemploEnvironmentMap:
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminacion - Iluminacion dinamica
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "Lights/EjemploSimpleEnvironmentMap"
    ///     Muestra como combinar los efectos de EnvironmentMap y BumpMapping con iluminacion dinamca con un PointLight.
    ///     Para EnvironmentMap se utiliza un CubeMap ya generado.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploEnvironmentMap : TGCExampleViewer
    {
        private CubeTexture cubeMap;
        private Effect effect;
        private TgcBox lightMesh;
        private List<TgcMeshBumpMapping> meshes;

        public EjemploEnvironmentMap(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Lights";
            Name = "Environment Map";
            Description = "Efecto de reflejo de luz con EnvironmentMap y BumpMap utilizando un CubeMap pre-calculado.";
        }

        public override void Init()
        {
            //Cargar textura de CubeMap para Environment Map
            cubeMap = TextureLoader.FromCubeFile(D3DDevice.Instance.Device, MediaDir + "CubeMap.dds");

            //Crear 3 paredes y un piso con textura comun y textura de normalMap
            var diffuseMap = TgcTexture.createTexture(MediaDir + "Texturas//BM_DiffuseMap_pared.jpg");
            var normalMap = TgcTexture.createTexture(MediaDir + "Texturas//BM_NormalMap.jpg");
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

            //Camara en 1ra persona
            Camara = new TgcFpsCamera(new Vector3(0, 50, 100), Input);

            //Cargar Shader personalizado para EnviromentMap
            effect = TgcShaders.loadEffect(ShadersDir + "EnvironmentMap.fx");

            //Cargar shader en meshes
            foreach (var m in meshes)
            {
                m.Effect = effect;
                m.Technique = "EnvironmentMapTechnique";
            }

            //Mesh para la luz
            lightMesh = TgcBox.fromSize(new Vector3(10, 10, 10), Color.Red);
            Modifiers.addFloat("reflection", 0, 1, 0.35f);
            Modifiers.addFloat("bumpiness", 0, 1, 1f);
            Modifiers.addVertex3f("lightPos", new Vector3(-200, 0, -200), new Vector3(200, 100, 200),
                new Vector3(0, 80, 0));
            Modifiers.addColor("lightColor", Color.White);
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

            //Actualzar posicion de la luz
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
                mesh.Effect.SetValue("reflection", (float)Modifiers["reflection"]);

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

                //CubeMap
                mesh.Effect.SetValue("texCubeMap", cubeMap);

                //Renderizar modelo
                mesh.render();
            }

            //Renderizar mesh de luz
            lightMesh.render();

            PostRender();
        }

        public override void Dispose()
        {
            effect.Dispose();
            foreach (var m in meshes)
            {
                m.dispose();
            }
            cubeMap.Dispose();
            lightMesh.dispose();
        }
    }
}