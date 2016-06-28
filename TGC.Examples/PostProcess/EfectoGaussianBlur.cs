using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;
using TGC.Examples.Example;

namespace TGC.Examples.PostProcess
{
    /// <summary>
    ///     Ejemplo EfectoGaussianBlur:
    ///     Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "PostProcess/EfectoBlur"
    ///     Muestra como utilizar la tenica de Render Target para lograr efectos de Post-Procesado.
    ///     Toda la escena no se dibuja a pantalla sino que se dibuja a una textura auxiliar.
    ///     Luego esa textura es renderizada con una pasada de Gaussian blur horizontal.
    ///     Y por ultimo se hace otra pasada mas de Gaussian blur pero vertical.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EfectoGaussianBlur : TGCExampleViewer
    {
        private Texture blurTempRT;
        private Effect effect;
        private List<TgcMesh> meshes;
        private Surface pOldRT;
        private Texture sceneRT;
        private TgcScreenQuad screenQuad;

        public EfectoGaussianBlur(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "PostProcess";
            Name = "Efecto Gaussian Blur";
            Description =
                "Graba la escena a un Render Target y luego con un pixel shader se borronea la imagen con Gaussian Blur.";
        }

        public override void Init()
        {
            //Creamos un FullScreen Quad
            screenQuad = new TgcScreenQuad();

            //Creamos un Render Targer sobre el cual se va a dibujar toda la escena original
            var backBufferWidth = D3DDevice.Instance.Device.PresentationParameters.BackBufferWidth;
            var backBufferHeight = D3DDevice.Instance.Device.PresentationParameters.BackBufferHeight;
            sceneRT = new Texture(D3DDevice.Instance.Device, backBufferWidth, backBufferHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8,
                Pool.Default);

            //Definimos el tamano de una textura que sea de 1/4 x 1/4 de la original, y que sean divisibles por 8 para facilitar los calculos de sampleo
            var cropWidth = (backBufferWidth - backBufferWidth % 8) / 4;
            var cropHeight = (backBufferHeight - backBufferHeight % 8) / 4;

            //Creamos un Render Target para auxiliar para almacenar la pasada horizontal de blur
            blurTempRT = new Texture(D3DDevice.Instance.Device, cropWidth, cropHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8,
                Pool.Default);

            //Cargar shader con efectos de Post-Procesado
            effect = TgcShaders.loadEffect(ShadersDir + "GaussianBlur.fx");
            //Configurar Technique dentro del shader
            effect.Technique = "GaussianBlurPass";

            //Cargamos un escenario
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");
            meshes = scene.Meshes;

            //Camara en primera personas
            Camara = new TgcFpsCamera(new Vector3(-182.3816f, 82.3252f, -811.9061f));

            //Modifier para activar/desactivar efecto
            Modifiers.addBoolean("activar_efecto", "Activar efecto", true);
            Modifiers.addFloat("deviation", 1, 5, 1);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            ClearTextures();

            //Cargamos el Render Targer al cual se va a dibujar la escena 3D. Antes nos guardamos el surface original
            //En vez de dibujar a la pantalla, dibujamos a un buffer auxiliar, nuestro Render Target.
            pOldRT = D3DDevice.Instance.Device.GetRenderTarget(0);
            var pSurf = sceneRT.GetSurfaceLevel(0);
            D3DDevice.Instance.Device.SetRenderTarget(0, pSurf);
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            //Dibujamos la escena comun, pero en vez de a la pantalla al Render Target
            drawSceneToRenderTarget(D3DDevice.Instance.Device);

            //Liberar memoria de surface de Render Target
            pSurf.Dispose();

            //Si quisieramos ver que se dibujo, podemos guardar el resultado a una textura en un archivo para debugear su resultado (ojo, es lento)
            //TextureLoader.Save(this.ShadersDir + "render_target.bmp", ImageFileFormat.Bmp, renderTarget2D);

            //Luego tomamos lo dibujado antes y lo combinamos con una textura con efecto de alarma
            drawPostProcess(D3DDevice.Instance.Device);
        }

        /// <summary>
        ///     Dibujamos toda la escena pero en vez de a la pantalla, la dibujamos al Render Target que se cargo antes.
        ///     Es como si dibujaramos a una textura auxiliar, que luego podemos utilizar.
        /// </summary>
        private void drawSceneToRenderTarget(Device d3dDevice)
        {
            //Arrancamos el renderizado. Esto lo tenemos que hacer nosotros a mano porque estamos en modo CustomRenderEnabled = true
            d3dDevice.BeginScene();

            //Dibujamos todos los meshes del escenario
            foreach (var m in meshes)
            {
                m.render();
            }

            //Terminamos manualmente el renderizado de esta escena. Esto manda todo a dibujar al GPU al Render Target que cargamos antes
            d3dDevice.EndScene();
        }

        /// <summary>
        ///     Se toma todo lo dibujado antes, que se guardo en una textura, y se le aplica un shader para borronear la imagen
        /// </summary>
        private void drawPostProcess(Device d3dDevice)
        {
            //Arrancamos la escena
            d3dDevice.BeginScene();

            //Ver si el efecto de oscurecer esta activado, configurar Technique del shader segun corresponda
            var activar_efecto = (bool)Modifiers["activar_efecto"];

            //Hacer blur
            if (activar_efecto)
            {
                var deviation = (float)Modifiers["deviation"];
                var blurTempS = blurTempRT.GetSurfaceLevel(0);

                //Gaussian blur horizontal
                Vector2[] texCoordOffsets;
                float[] colorWeights;
                TgcPostProcessingUtils.computeGaussianBlurSampleOffsets15(blurTempS.Description.Width, deviation, 1,
                    true, out texCoordOffsets, out colorWeights);
                effect.Technique = "GaussianBlurPass";
                effect.SetValue("texSceneRT", sceneRT);
                effect.SetValue("gauss_offsets", TgcParserUtils.vector2ArrayToFloat2Array(texCoordOffsets));
                effect.SetValue("gauss_weights", colorWeights);
                d3dDevice.SetRenderTarget(0, blurTempS);
                d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                screenQuad.render(effect);

                //Gaussian blur vertical
                TgcPostProcessingUtils.computeGaussianBlurSampleOffsets15(blurTempS.Description.Height, deviation, 1,
                    false, out texCoordOffsets, out colorWeights);
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

            //Terminamos el renderizado de la escena
            RenderFPS();
            RenderAxis();
            d3dDevice.EndScene();
            d3dDevice.Present();
        }

        public override void Dispose()
        {
            foreach (var m in meshes)
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