using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

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
        private TGCFloatModifier reflectionModifier;
        private TGCFloatModifier bumpinessModifier;
        private TGCVertex3fModifier lightPosModifier;
        private TGCColorModifier lightColorModifier;
        private TGCFloatModifier lightIntensityModifier;
        private TGCFloatModifier lightAttenuationModifier;
        private TGCFloatModifier specularExModifier;
        private TGCColorModifier mEmissiveModifier;
        private TGCColorModifier mAmbientModifier;
        private TGCColorModifier mDiffuseModifier;
        private TGCColorModifier mSpecularModifier;

        private CubeTexture cubeMap;
        private Effect effect;
        private TGCBox lightMesh;
        private List<TgcMeshBumpMapping> meshes;

        public EjemploEnvironmentMap(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
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

            var paredSur = TGCBox.fromExtremes(new TGCVector3(-200, 0, -210), new TGCVector3(200, 100, -200), diffuseMap);
            paredSur.Transform = TGCMatrix.Translation(paredSur.Position);

            var paredOeste = TGCBox.fromExtremes(new TGCVector3(-210, 0, -200), new TGCVector3(-200, 100, 200), diffuseMap);
            paredOeste.Transform = TGCMatrix.Translation(paredOeste.Position);

            var paredEste = TGCBox.fromExtremes(new TGCVector3(200, 0, -200), new TGCVector3(210, 100, 200), diffuseMap);
            paredEste.Transform = TGCMatrix.Translation(paredEste.Position);

            var piso = TGCBox.fromExtremes(new TGCVector3(-200, -1, -200), new TGCVector3(200, 0, 200), diffuseMap);
            piso.Transform = TGCMatrix.Translation(piso.Position);

            //Convertir TgcBox a TgcMesh
            var m1 = paredSur.ToMesh("paredSur");
            var m2 = paredOeste.ToMesh("paredOeste");
            var m3 = paredEste.ToMesh("paredEste");
            var m4 = piso.ToMesh("piso");

            //Convertir TgcMesh a TgcMeshBumpMapping
            meshes = new List<TgcMeshBumpMapping>();
            meshes.Add(TgcMeshBumpMapping.fromTgcMesh(m1, normalMapArray));
            meshes.Add(TgcMeshBumpMapping.fromTgcMesh(m2, normalMapArray));
            meshes.Add(TgcMeshBumpMapping.fromTgcMesh(m3, normalMapArray));
            meshes.Add(TgcMeshBumpMapping.fromTgcMesh(m4, normalMapArray));

            //Borrar TgcMesh y TgcBox, ya no se usan
            paredSur.Dispose();
            paredOeste.Dispose();
            paredEste.Dispose();
            piso.Dispose();
            m1.Dispose();
            m2.Dispose();
            m3.Dispose();
            m4.Dispose();

            //Camara en 1ra persona
            Camara = new TgcFpsCamera(new TGCVector3(200, 60, 50), Input);

            //Cargar Shader personalizado para EnviromentMap
            effect = TgcShaders.loadEffect(ShadersDir + "EnvironmentMap.fx");

            //Cargar shader en meshes
            foreach (var m in meshes)
            {
                m.Effect = effect;
                m.Technique = "EnvironmentMapTechnique";
            }

            //Mesh para la luz
            lightMesh = TGCBox.fromSize(new TGCVector3(10, 10, 10), Color.Red);

            reflectionModifier = AddFloat("reflection", 0, 1, 0.35f);
            bumpinessModifier = AddFloat("bumpiness", 0, 1, 1f);
            lightPosModifier = AddVertex3f("lightPos", new TGCVector3(-200, 0, -200), new TGCVector3(200, 100, 200), new TGCVector3(0, 80, 0));
            lightColorModifier = AddColor("lightColor", Color.White);
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

            //Actualzar posicion de la luz
            var lightPos = lightPosModifier.Value;
            lightMesh.Position = lightPos;
            var eyePosition = Camara.Position;

            //Renderizar meshes
            foreach (var mesh in meshes)
            {
                //Cargar variables shader de la luz
                mesh.Effect.SetValue("lightColor", ColorValue.FromColor(lightColorModifier.Value));
                mesh.Effect.SetValue("lightPosition", TGCVector3.Vector3ToFloat4Array(lightPos));
                mesh.Effect.SetValue("eyePosition", TGCVector3.Vector3ToFloat4Array(eyePosition));
                mesh.Effect.SetValue("lightIntensity", lightIntensityModifier.Value);
                mesh.Effect.SetValue("lightAttenuation", lightAttenuationModifier.Value);
                mesh.Effect.SetValue("bumpiness", bumpinessModifier.Value);
                mesh.Effect.SetValue("reflection", reflectionModifier.Value);

                //Material
                mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(mEmissiveModifier.Value));
                mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(mAmbientModifier.Value));
                mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(mDiffuseModifier.Value));
                mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(mSpecularModifier.Value));
                mesh.Effect.SetValue("materialSpecularExp", specularExModifier.Value);

                //CubeMap
                mesh.Effect.SetValue("texCubeMap", cubeMap);

                //Renderizar modelo
                mesh.render();
            }

            //Renderizar mesh de luz
            lightMesh.Render();

            PostRender();
        }

        public override void Dispose()
        {
            effect.Dispose();
            foreach (var m in meshes)
            {
                m.Dispose();
            }
            cubeMap.Dispose();
            lightMesh.Dispose();
        }
    }
}