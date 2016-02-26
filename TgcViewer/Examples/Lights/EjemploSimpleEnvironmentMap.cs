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
using TGC.Core.Utils;

namespace Examples.Lights
{
    /// <summary>
    /// Ejemplo EjemploEnvironmentMap:
    /// Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminación - Iluminación dinámica
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    /// 
    /// Ejemplo avanzado. Ver primero ejemplo "Lights/EjemploPointLight"
    /// 
    /// Muestra utilizar el efecto de EnvironmentMap.
    /// Se utiliza un CubeMap ya generado.
    /// 
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploSimpleEnvironmentMap : TgcExample
    {
        Effect effect;
        TgcBox lightMesh;
        List<TgcMeshBumpMapping> meshes;
        CubeTexture cubeMap;


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
            Device d3dDevice = GuiController.Instance.D3dDevice;


            //Cargar textura de CubeMap para Environment Map
            cubeMap = TextureLoader.FromCubeFile(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Shaders\\CubeMap.dds");


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
            effect = TgcShaders.loadEffect(GuiController.Instance.ExamplesMediaDir + "Shaders\\EnvironmentMap.fx");
            effect.Technique = "SimpleEnvironmentMapTechnique";

            //Cargar shader en meshes
            foreach (TgcMeshBumpMapping m in meshes)
            {
                m.Effect = effect;
                m.Technique = "SimpleEnvironmentMapTechnique";
            }
            
            //Mesh para la luz
            lightMesh = TgcBox.fromSize(new Vector3(10, 10, 10), Color.Red);
            GuiController.Instance.Modifiers.addFloat("reflection", 0, 1, 0.35f);
            GuiController.Instance.Modifiers.addVertex3f("lightPos", new Vector3(-200, 0, -200), new Vector3(200, 100, 200), new Vector3(0, 80, 0));
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
            Device device = GuiController.Instance.D3dDevice;

            //Actualzar posición de la luz
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
                mesh.Effect.SetValue("reflection", (float)GuiController.Instance.Modifiers["reflection"]);

                //Material
                mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mEmissive"]));
                mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mAmbient"]));
                mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mDiffuse"]));
                mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mSpecular"]));
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
            foreach (TgcMeshBumpMapping m in meshes)
            {
                m.dispose();
            }
            lightMesh.dispose();
        }



    }

    

}
