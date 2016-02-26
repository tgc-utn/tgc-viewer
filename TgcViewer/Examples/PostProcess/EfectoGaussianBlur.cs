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
using TGC.Core.Utils;

namespace Examples.PostProcess
{

    /// <summary>
    /// Ejemplo EfectoGaussianBlur:
    /// Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    /// 
    /// Ejemplo avanzado. Ver primero ejemplo "PostProcess/EfectoBlur"
    /// Muestra como utilizar la tenica de Render Target para lograr efectos de Post-Procesado.
    /// Toda la escena no se dibuja a pantalla sino que se dibuja a una textura auxiliar.
    /// Luego esa textura es renderizada con una pasada de Gaussian blur horizontal.
    /// Y por ultimo se hace otra pasada mas de Gaussian blur pero vertical.
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EfectoGaussianBlur : TgcExample
    {

        TgcScreenQuad screenQuad;
        Texture sceneRT;
        Texture blurTempRT;
        Surface pOldRT;
        Effect effect;
        List<TgcMesh> meshes;


        public override string getCategory()
        {
            return "PostProcess";
        }

        public override string getName()
        {
            return "Efecto Gaussian Blur";
        }

        public override string getDescription()
        {
            return "Graba la escena a un Render Target y luego con un pixel shader se borronea la imagen con Gaussian Blur.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Activamos el renderizado customizado. De esta forma el framework nos delega control total sobre como dibujar en pantalla
            //La responsabilidad cae toda de nuestro lado
            GuiController.Instance.CustomRenderEnabled = true;

            //Creamos un FullScreen Quad
            screenQuad = new TgcScreenQuad();
            

            //Creamos un Render Targer sobre el cual se va a dibujar toda la escena original
            int backBufferWidth = d3dDevice.PresentationParameters.BackBufferWidth;
            int backBufferHeight = d3dDevice.PresentationParameters.BackBufferHeight;
            sceneRT = new Texture(d3dDevice, backBufferWidth, backBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

            //Definimos el tamaño de una textura que sea de 1/4 x 1/4 de la original, y que sean divisibles por 8 para facilitar los calculos de sampleo
            int cropWidth = (backBufferWidth - backBufferWidth % 8) / 4;
            int cropHeight = (backBufferHeight - backBufferHeight % 8) / 4;

            //Creamos un Render Target para auxiliar para almacenar la pasada horizontal de blur
            blurTempRT = new Texture(d3dDevice, cropWidth, cropHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);


            //Cargar shader con efectos de Post-Procesado
            effect = TgcShaders.loadEffect(GuiController.Instance.ExamplesMediaDir + "Shaders\\GaussianBlur.fx");
            //Configurar Technique dentro del shader
            effect.Technique = "GaussianBlurPass";


            //Cargamos un escenario
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");
            meshes = scene.Meshes;


            //Camara en primera personas
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.MovementSpeed *= 2;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-182.3816f, 82.3252f, -811.9061f), new Vector3(-182.0957f, 82.3147f, -810.9479f));
            

            //Modifier para activar/desactivar efecto
            GuiController.Instance.Modifiers.addBoolean("activar_efecto", "Activar efecto", true);
            GuiController.Instance.Modifiers.addFloat("deviation", 1, 5, 1);
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Cargamos el Render Targer al cual se va a dibujar la escena 3D. Antes nos guardamos el surface original
            //En vez de dibujar a la pantalla, dibujamos a un buffer auxiliar, nuestro Render Target.
            pOldRT = d3dDevice.GetRenderTarget(0);
            Surface pSurf = sceneRT.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, pSurf);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);


            //Dibujamos la escena comun, pero en vez de a la pantalla al Render Target
            drawSceneToRenderTarget(d3dDevice);

            //Liberar memoria de surface de Render Target
            pSurf.Dispose();

            //Si quisieramos ver que se dibujo, podemos guardar el resultado a una textura en un archivo para debugear su resultado (ojo, es lento)
            //TextureLoader.Save(GuiController.Instance.ExamplesMediaDir + "Shaders\\render_target.bmp", ImageFileFormat.Bmp, renderTarget2D);


            //Luego tomamos lo dibujado antes y lo combinamos con una textura con efecto de alarma
            drawPostProcess(d3dDevice);
        }

        

        /// <summary>
        /// Dibujamos toda la escena pero en vez de a la pantalla, la dibujamos al Render Target que se cargo antes.
        /// Es como si dibujaramos a una textura auxiliar, que luego podemos utilizar.
        /// </summary>
        private void drawSceneToRenderTarget(Device d3dDevice)
        {
            //Arrancamos el renderizado. Esto lo tenemos que hacer nosotros a mano porque estamos en modo CustomRenderEnabled = true
            d3dDevice.BeginScene();

            //Dibujamos todos los meshes del escenario
            foreach (TgcMesh m in meshes)
            {
                m.render();
            }


            //Terminamos manualmente el renderizado de esta escena. Esto manda todo a dibujar al GPU al Render Target que cargamos antes
            d3dDevice.EndScene();
        }


        /// <summary>
        /// Se toma todo lo dibujado antes, que se guardo en una textura, y se le aplica un shader para borronear la imagen
        /// </summary>
        private void drawPostProcess(Device d3dDevice)
        {
            //Arrancamos la escena
            d3dDevice.BeginScene();

            //Ver si el efecto de oscurecer esta activado, configurar Technique del shader segun corresponda
            bool activar_efecto = (bool)GuiController.Instance.Modifiers["activar_efecto"];

            //Hacer blur
            if (activar_efecto)
            {
                float deviation = (float)GuiController.Instance.Modifiers["deviation"];
                Surface blurTempS = blurTempRT.GetSurfaceLevel(0);

                //Gaussian blur horizontal
                Vector2[] texCoordOffsets;
                float[] colorWeights;
                TgcPostProcessingUtils.computeGaussianBlurSampleOffsets15(blurTempS.Description.Width, deviation, 1, true, out texCoordOffsets, out colorWeights);
                effect.Technique = "GaussianBlurPass";
                effect.SetValue("texSceneRT", sceneRT);
                effect.SetValue("gauss_offsets", TgcParserUtils.vector2ArrayToFloat2Array(texCoordOffsets));
                effect.SetValue("gauss_weights", colorWeights);
                d3dDevice.SetRenderTarget(0, blurTempS);
                d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                screenQuad.render(effect);
                
                //Gaussian blur vertical
                TgcPostProcessingUtils.computeGaussianBlurSampleOffsets15(blurTempS.Description.Height, deviation, 1, false, out texCoordOffsets, out colorWeights);
                effect.Technique = "GaussianBlurPass";
                effect.SetValue("texSceneRT", blurTempRT);
                effect.SetValue("gauss_offsets", TgcParserUtils.vector2ArrayToFloat2Array(texCoordOffsets));
                effect.SetValue("gauss_weights", colorWeights);
                d3dDevice.SetRenderTarget(0, pOldRT);
                d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                screenQuad.render(effect);

                blurTempS.Dispose();
            }
            //No hacer blur
            else
            {
                effect.Technique = "DefaultTechnique";
                effect.SetValue("texSceneRT", sceneRT);
                d3dDevice.SetRenderTarget(0, pOldRT);
                d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                screenQuad.render(effect);
            }


            //Como estamos en modo CustomRenderEnabled, tenemos que dibujar todo nosotros, incluso el contador de FPS
            GuiController.Instance.Text3d.drawText("FPS: " + HighResolutionTimer.Instance.FramesPerSecond, 0, 0, Color.Yellow);

            //Tambien hay que dibujar el indicador de los ejes cartesianos
            GuiController.Instance.AxisLines.render();


            //Terminamos el renderizado de la escena
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
            blurTempRT.Dispose();
            pOldRT.Dispose();
        }

    }
}
