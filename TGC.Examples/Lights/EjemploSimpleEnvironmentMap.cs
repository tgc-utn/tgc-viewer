using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Core.Utils;
using TGC.Util;

namespace TGC.Examples.Lights
{
    /// <summary>
    ///     Ejemplo EjemploEnvironmentMap:
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminación - Iluminación dinámica
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "Lights/EjemploPointLight"
    ///     Muestra utilizar el efecto de EnvironmentMap.
    ///     Se utiliza un CubeMap ya generado.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploSimpleEnvironmentMap : TgcExample
    {
        private CubeTexture cubeMap;
        private Effect effect;
        private TgcBox lightMesh;
        private List<TgcMeshBumpMapping> meshes;

        public override string getCategory()
        {
            return "Lights";
        }

        public override string getName()
        {
            return "Simple Environment Map";
        }

        public override string getDescription()
        {
            return "Efecto de reflejo con Environment Map utilizando un CubeMap pre-calculado";
        }

        public override void init()
        {
            //Cargar textura de CubeMap para Environment Map
            cubeMap = TextureLoader.FromCubeFile(D3DDevice.Instance.Device,
                GuiController.Instance.ShadersDir + "CubeMap.dds");

            //Crear 3 paredes y un piso con textura comun y textura de normalMap
            var diffuseMap =
                 TgcTexture.createTexture(GuiController.Instance.ExamplesMediaDir + "Texturas//BM_DiffuseMap_pared.jpg");
            var normalMap =
                TgcTexture.createTexture(GuiController.Instance.ExamplesMediaDir + "Texturas//BM_NormalMap.jpg");
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

            //Convertir TgcMesh a TgcMeshBumpMapping (se usa solo por conveniencia, pero el NormalMap de TgcMeshBumpMapping es innecesario para este ejemplo)
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
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(0, 50, 100), new Vector3(0, 50, -1));

            //Cargar Shader de DynamicLights
            effect = TgcShaders.loadEffect(GuiController.Instance.ShadersDir + "EnvironmentMap.fx");
            effect.Technique = "SimpleEnvironmentMapTechnique";

            //Cargar shader en meshes
            foreach (var m in meshes)
            {
                m.Effect = effect;
                m.Technique = "SimpleEnvironmentMapTechnique";
            }

            //Mesh para la luz
            lightMesh = TgcBox.fromSize(new Vector3(10, 10, 10), Color.Red);
            GuiController.Instance.Modifiers.addFloat("reflection", 0, 1, 0.35f);
            GuiController.Instance.Modifiers.addVertex3f("lightPos", new Vector3(-200, 0, -200),
                new Vector3(200, 100, 200), new Vector3(0, 80, 0));
            GuiController.Instance.Modifiers.addColor("lightColor", Color.White);
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
            //Actualzar posición de la luz
            var lightPos = (Vector3)GuiController.Instance.Modifiers["lightPos"];
            lightMesh.Position = lightPos;
            var eyePosition = GuiController.Instance.FpsCamera.getPosition();

            //Renderizar meshes
            foreach (var mesh in meshes)
            {
                //Cargar variables shader de la luz
                mesh.Effect.SetValue("lightColor",
                    ColorValue.FromColor((Color)GuiController.Instance.Modifiers["lightColor"]));
                mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(eyePosition));
                mesh.Effect.SetValue("lightIntensity", (float)GuiController.Instance.Modifiers["lightIntensity"]);
                mesh.Effect.SetValue("lightAttenuation", (float)GuiController.Instance.Modifiers["lightAttenuation"]);
                mesh.Effect.SetValue("reflection", (float)GuiController.Instance.Modifiers["reflection"]);

                //Material
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

                //Renderizar modelo
                mesh.render();
            }

            //Renderizar mesh de luz
            lightMesh.render();
        }

        public override void close()
        {
            effect.Dispose();
            foreach (var m in meshes)
            {
                m.dispose();
            }
            lightMesh.dispose();
        }
    }
}