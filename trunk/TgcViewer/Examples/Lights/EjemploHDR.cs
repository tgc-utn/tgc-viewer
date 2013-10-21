using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Interpolation;
using TgcViewer.Utils;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.Lights
{

    /// <summary>
    /// Ejemplo EjemploHDR:
    /// Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminaci�n - Iluminaci�n din�mica
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    /// 
    /// Ejemplo avanzado. Ver primero ejemplo "Lights/EjemploPointLight" y "PostProcess/EfectoGaussianBlur"
    /// 
    /// 
    /// 
    /// 
    /// 
    /// Autor: Mat�as Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EfectoGaussianBlur : TgcExample
    {

        const int NUM_LUMINANCE_TEXTURES = 4;


        TgcScreenQuad screenQuad;
        Texture sceneRT;
        Texture scaledSceneRT;
        Texture brightPassRT;
        Texture bloomRT;
        Texture bloomTempRT;
        Texture[] luminanceRTs;
        Surface pOldRT;
        Effect effect;
        List<TgcMesh> meshes;
        TgcBox lightMesh;

        public override string getCategory()
        {
            return "Lights";
        }

        public override string getName()
        {
            return "HDR";
        }

        public override string getDescription()
        {
            return "HDR";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Activamos el renderizado customizado. De esta forma el framework nos delega control total sobre como dibujar en pantalla
            //La responsabilidad cae toda de nuestro lado
            GuiController.Instance.CustomRenderEnabled = true;

            //Creamos un FullScreen Quad
            screenQuad = new TgcScreenQuad();
            

            //Crear RT con formato de floating point en vez del tipico formato de enteros
            int backBufferWidth = d3dDevice.PresentationParameters.BackBufferWidth;
            int backBufferHeight = d3dDevice.PresentationParameters.BackBufferHeight;
            sceneRT = new Texture(d3dDevice, backBufferWidth, backBufferHeight, 1, Usage.RenderTarget, Format.A16B16G16R16F, Pool.Default);

            //Crear RT para escalar sceneRT a un tama�o menor, de 1/4 x 1/4 (y divisible por 8 para facilitar los calculos de sampleo)
            int cropWidth = (backBufferWidth - backBufferWidth % 8) / 4;
            int cropHeight = (backBufferHeight - backBufferHeight % 8) / 4;
            scaledSceneRT = new Texture(d3dDevice, cropWidth, cropHeight, 1, Usage.RenderTarget, Format.A16B16G16R16F, Pool.Default);

            //Crear RT para el bright-pass filter, de igual tama�o que scaledSceneRT (formato comun)
            brightPassRT = new Texture(d3dDevice, cropWidth, cropHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);

            //Crear RT para el efecto bloom (formato comun). Son dos por la doble pasada del filtro gaussiano
            bloomRT = new Texture(d3dDevice, cropWidth, cropHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            bloomTempRT = new Texture(d3dDevice, cropWidth, cropHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);

            //Crear un RT por cada paso de downsampling necesario para obtener el average luminance (un solo canal de 16 bits)
            luminanceRTs = new Texture[NUM_LUMINANCE_TEXTURES];
            for (int i = 0; i < luminanceRTs.Length; i++)
            {
                int iSampleLen = 1 << ( 2 * i );
                luminanceRTs[i] = new Texture(d3dDevice, iSampleLen, iSampleLen, 1, Usage.RenderTarget, Format.R16F, Pool.Default);
            }



            //Cargar shader con efectos de Post-Procesado
            effect = TgcShaders.loadEffect(GuiController.Instance.ExamplesMediaDir + "Shaders\\HDR.fx");


            //Cargamos un escenario
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");
            meshes = scene.Meshes;

            //Aplicar a cada mesh el shader de luz custom
            foreach (TgcMesh mesh in scene.Meshes)
            {
                mesh.Effect = effect;
                mesh.Technique = "LightPass";
            }

            //Mesh para la luz
            lightMesh = TgcBox.fromSize(new Vector3(10, 10, 10), Color.Red);
            lightMesh.Effect = effect;
            lightMesh.Technique = "DrawLightSource";

            //Camara en 1ra persona
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.MovementSpeed = 400f;
            GuiController.Instance.FpsCamera.JumpSpeed = 300f;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-20, 80, 450), new Vector3(0, 80, 1));

            //Modifiers de la luz
            GuiController.Instance.Modifiers.addBoolean("toneMapping", "toneMapping", true);
            GuiController.Instance.Modifiers.addFloat("lightIntensity", 0, 100, 5);
            GuiController.Instance.Modifiers.addFloat("middleGray", 0.1f, 1, 0.72f);
            GuiController.Instance.Modifiers.addVertex3f("lightPos", new Vector3(-400, -200, -400), new Vector3(400, 300, 500), new Vector3(60, 35, 250));
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Guardar RT original
            pOldRT = d3dDevice.GetRenderTarget(0);
            
            //Dibujamos la escena al RT en floating point
            drawSceneToRenderTarget(d3dDevice);

            //Generar version reducida de la imagen original de la escena
            scaleScene(d3dDevice);

            //Buscar luminance promedio de la escena
            findAverageLuminance(d3dDevice);

            //Hacer bright-pass para quedarse con los pixels mas luminosos
            brightPass(d3dDevice);

            //Hacer blur de bright-pass para generar efecto de bloom
            bloomPass(d3dDevice);

            //Final render
            finalRender(d3dDevice);
        }

        

        

        

        /// <summary>
        /// Dibujamos toda la escena pero en vez de a la pantalla, la dibujamos al Render Target con floating point.
        /// De esa forma todos los calculos de iluminacion que superen 1.0 no son clampeados
        /// </summary>
        private void drawSceneToRenderTarget(Device d3dDevice)
        {
            Surface pSurf = sceneRT.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, pSurf);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            //Arrancamos el renderizado
            d3dDevice.BeginScene();

            //Actualzar posici�n de la luz
            Vector3 lightPos = (Vector3)GuiController.Instance.Modifiers["lightPos"];
            lightMesh.Position = lightPos;

            //Dibujar mesh de fuente de luz
            lightMesh.Effect.Technique = "DrawLightSource";
            lightMesh.render();

            //Renderizar meshes
            foreach (TgcMesh mesh in meshes)
            {
                mesh.Effect.Technique = "LightPass";

                //Cargar variables shader de la luz
                mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(GuiController.Instance.FpsCamera.getPosition()));
                mesh.Effect.SetValue("lightIntensity", (float)GuiController.Instance.Modifiers["lightIntensity"]);

                //Cargar variables de shader de Material
                mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.DarkGray));
                mesh.Effect.SetValue("materialSpecularExp", 9f);

                //Renderizar modelo
                mesh.render();
            }


            //Renderizar mesh de luz
            lightMesh.render();


            d3dDevice.EndScene();
            pSurf.Dispose();
        }

        /// <summary>
        /// Generar imagen reducida del RT de HDR original de la escena.
        /// Lo reducimos a una textura de 1/4 x 1/4 de la original.
        /// El shader lee 4x4 texels y los promedia
        /// </summary>
        private void scaleScene(Device d3dDevice)
        {
            d3dDevice.BeginScene();

            int backBufferWidth = d3dDevice.PresentationParameters.BackBufferWidth;
            int backBufferHeight = d3dDevice.PresentationParameters.BackBufferHeight;
            Vector2[] sampleOffsets = TgcPostProcessingUtils.computeDownScaleOffsets4x4(backBufferWidth, backBufferHeight);

            effect.Technique = "DownScale4x4";
            effect.SetValue("texSceneRT", sceneRT);
            effect.SetValue("sampleOffsets", TgcParserUtils.vector2ArrayToFloat2Array(sampleOffsets));
            Surface scaledSceneS = scaledSceneRT.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, scaledSceneS);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            screenQuad.render(effect);


            d3dDevice.EndScene();
            scaledSceneS.Dispose();
        }

        /// <summary>
        /// Recorremos toda la imagen de scaledSceneRT y buscamos el pixel con el maximo nivel de luminosidad.
        /// Lo hacemos en GPU, con varias pasadas de downsampling, hasta llegar a una textura de 1x1.
        /// </summary>
        private void findAverageLuminance(Device d3dDevice)
        {
            d3dDevice.BeginScene();

            //Obtener surfaces de todos los luminanceRTs
            Surface[] luminanceSurfaces = new Surface[luminanceRTs.Length];
            for (int i = 0; i < luminanceRTs.Length; i++)
            {
                luminanceSurfaces[i] = luminanceRTs[i].GetSurfaceLevel(0);
            }

            int curTexture = luminanceRTs.Length - 1;
            int surfWidth = luminanceSurfaces[curTexture].Description.Width;
            int surfHeight = luminanceSurfaces[curTexture].Description.Height;

            //Calcular los offsets de la primera pasada de downsampling de luminance
            float tU, tV;
            tU = 1.0f / (3.0f * surfWidth);
            tV = 1.0f / (3.0f * surfHeight);
            int x, y;
            int index = 0;
            Vector2[] sampleOffsets = new Vector2[16];
            for (x = -1; x <= 1; x++)
            {
                for (y = -1; y <= 1; y++)
                {
                    sampleOffsets[index] = new Vector2(x * tU, y * tV);
                    index++;
                }
            }

            //Primera pasada de luminance: calculamos el promedio de log(luminance) de 4x4
            effect.Technique = "SampleAvgLuminance_Init";
            effect.SetValue("texSceneRT", scaledSceneRT);
            effect.SetValue("sampleOffsets", TgcParserUtils.vector2ArrayToFloat2Array(sampleOffsets));
            d3dDevice.SetRenderTarget(0, luminanceSurfaces[curTexture]);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            screenQuad.render(effect);
            curTexture--;

            //DEBUG
            //TextureLoader.Save(GuiController.Instance.ExamplesMediaDir + "Shaders\\luminance_" + (curTexture+1) + ".bmp", ImageFileFormat.Bmp, luminanceRTs[curTexture + 1]);


            //Hacer pasadas de downsampling intermedio
            while (curTexture > 0)
            {
                //Calcular offsets de esta pasada de downsampling
                surfWidth = luminanceSurfaces[curTexture].Description.Width;
                surfHeight = luminanceSurfaces[curTexture].Description.Height;
                sampleOffsets = TgcPostProcessingUtils.computeDownScaleOffsets4x4(surfWidth, surfHeight);

                //Hacer downsampling
                effect.Technique = "SampleAvgLuminance_Intermediate";
                effect.SetValue("texSceneRT", luminanceRTs[curTexture + 1]);
                effect.SetValue("sampleOffsets", TgcParserUtils.vector2ArrayToFloat2Array(sampleOffsets));
                d3dDevice.SetRenderTarget(0, luminanceSurfaces[curTexture]);
                d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                screenQuad.render(effect);
                curTexture--;

                //DEBUG
                //TextureLoader.Save(GuiController.Instance.ExamplesMediaDir + "Shaders\\luminance_" + (curTexture+1) + ".bmp", ImageFileFormat.Bmp, luminanceRTs[curTexture + 1]);
            }

            //Hacer downsampling final de 1x1
            surfWidth = luminanceSurfaces[1].Description.Width;
            surfHeight = luminanceSurfaces[1].Description.Height;
            sampleOffsets = TgcPostProcessingUtils.computeDownScaleOffsets4x4(surfWidth, surfHeight);

            //Esta ultima pasada hace un promedio de 4x4 y luego hace exp para quitar el log y obtener el promedio de luminance de toda la escena
            effect.Technique = "SampleAvgLuminance_End";
            effect.SetValue("texSceneRT", luminanceRTs[1]);
            effect.SetValue("sampleOffsets", TgcParserUtils.vector2ArrayToFloat2Array(sampleOffsets));
            d3dDevice.SetRenderTarget(0, luminanceSurfaces[0]);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            screenQuad.render(effect);

            //DEBUG
            //TextureLoader.Save(GuiController.Instance.ExamplesMediaDir + "Shaders\\luminance_" + 0 + ".bmp", ImageFileFormat.Bmp, luminanceRTs[0]);



            //Liberar todos los surfaces usados
            for (int i = 0; i < luminanceSurfaces.Length; i++)
            {
                luminanceSurfaces[i].Dispose();
            }

            d3dDevice.EndScene();
        }

        /// <summary>
        /// Quedarse con los pixels de scaledSceneRT que superan un cierto nivel de brillo 
        /// </summary>
        private void brightPass(Device d3dDevice)
        {
            d3dDevice.BeginScene();

            float middleGray = (float)GuiController.Instance.Modifiers["middleGray"];

            effect.Technique = "BrightPass";
            effect.SetValue("texSceneRT", scaledSceneRT);
            effect.SetValue("texLuminanceRT", luminanceRTs[0]);
            effect.SetValue("middleGray", middleGray);
            Surface brightPassS = brightPassRT.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, brightPassS);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            screenQuad.render(effect);

            //DEBUG
            //TextureLoader.Save(GuiController.Instance.ExamplesMediaDir + "Shaders\\brightPass.bmp", ImageFileFormat.Bmp, brightPassRT);

            brightPassS.Dispose();
            d3dDevice.EndScene();
        }


        /// <summary>
        /// Hacer blur de bright-pass para generar efecto de bloom
        /// </summary>
        /// <param name="d3dDevice"></param>
        private void bloomPass(Device d3dDevice)
        {
            d3dDevice.BeginScene();

            //Gaussian blur horizontal
            Vector2[] texCoordOffsets;
            float[] colorWeights;
            Surface bloomTempS = bloomTempRT.GetSurfaceLevel(0);
            TgcPostProcessingUtils.computeGaussianBlurSampleOffsets15(bloomTempS.Description.Width, 1, 1, true, out texCoordOffsets, out colorWeights);
            effect.Technique = "BloomPass";
            effect.SetValue("texBloomRT", brightPassRT);
            d3dDevice.SetRenderTarget(0, bloomTempS);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            screenQuad.render(effect);

            //Gaussian blur vertical
            TgcPostProcessingUtils.computeGaussianBlurSampleOffsets15(bloomTempS.Description.Height, 1, 1, false, out texCoordOffsets, out colorWeights);
            effect.Technique = "BloomPass";
            effect.SetValue("texBloomRT", bloomTempRT);
            effect.SetValue("gauss_offsets", TgcParserUtils.vector2ArrayToFloat2Array(texCoordOffsets));
            effect.SetValue("gauss_weights", colorWeights);
            Surface bloomS = bloomRT.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, bloomS);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            screenQuad.render(effect);

            bloomTempS.Dispose();
            bloomS.Dispose();
            d3dDevice.EndScene();
        }


        /// <summary>
        /// Render final donde une todo
        /// </summary>
        private void finalRender(Device d3dDevice)
        {
            d3dDevice.BeginScene();

            bool toneMapping = (bool)GuiController.Instance.Modifiers["toneMapping"];
            effect.Technique = toneMapping ? "FinalRender" : "FinalRenderNoToneMapping";
            effect.SetValue("texSceneRT", sceneRT);
            effect.SetValue("texLuminanceRT", luminanceRTs[0]);
            effect.SetValue("texBloomRT", bloomRT);
            effect.SetValue("middleGray", (float)GuiController.Instance.Modifiers["middleGray"]);
            d3dDevice.SetRenderTarget(0, pOldRT);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            screenQuad.render(effect);

            //Mostrar FPS
            GuiController.Instance.Text3d.drawText("FPS: " + HighResolutionTimer.Instance.FramesPerSecond, 0, 0, Color.Yellow);

            d3dDevice.EndScene();
        }



        public override void close()
        {
            foreach (TgcMesh m in meshes)
            {
                m.dispose();
            }
            effect.Dispose();
            screenQuad.dispose();
            sceneRT.Dispose();
            scaledSceneRT.Dispose();
            brightPassRT.Dispose();
            bloomRT.Dispose();
            bloomTempRT.Dispose();
            for (int i = 0; i < luminanceRTs.Length; i++)
            {
                luminanceRTs[i].Dispose();
            }
            pOldRT.Dispose();
        }

    }
}
