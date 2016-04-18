using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core;
using TGC.Core._2D;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Interpolation;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Util;

namespace TGC.Examples.PostProcess
{
    /// <summary>
    ///     Ejemplo EfectoOscurecer:
    ///     Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "Shaders/EjemploShaderTgcMesh".
    ///     Muestra como utilizar la tenica de Render Target para lograr efectos de Post-Procesado.
    ///     Toda la escena no se dibuja a pantalla sino que se dibuja a una textura auxiliar.
    ///     Luego se crea un unico mesh (un Quad) que ocupa toda la pantalla y se le carga como textura
    ///     esta imagen generada antes.
    ///     De esta forma se pueden hacer diversos efectos 2D con pixels shaders sobre la imagen final.
    ///     En este caso, la imagen final se oscurece de forma intermitente.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EfectoOscurecer : TgcExample
    {
        private Effect effect;
        private InterpoladorVaiven intVaivenOscurecer;
        private List<TgcMesh> meshes;
        private Surface pOldRT;
        private Texture renderTarget2D;
        private VertexBuffer screenQuadVB;

        public override string getCategory()
        {
            return "PostProcess";
        }

        public override string getName()
        {
            return "Efecto Oscurecer";
        }

        public override string getDescription()
        {
            return "Graba la escena a un Render Target y luego con un pixel shader la oscurece en forma intermitente.";
        }

        public override void init()
        {
            //Activamos el renderizado customizado. De esta forma el framework nos delega control total sobre como dibujar en pantalla
            //La responsabilidad cae toda de nuestro lado
            GuiController.Instance.CustomRenderEnabled = true;

            //Se crean 2 triangulos (o Quad) con las dimensiones de la pantalla con sus posiciones ya transformadas
            // x = -1 es el extremo izquiedo de la pantalla, x = 1 es el extremo derecho
            // Lo mismo para la Y con arriba y abajo
            // la Z en 1 simpre
            CustomVertex.PositionTextured[] screenQuadVertices =
            {
                new CustomVertex.PositionTextured(-1, 1, 1, 0, 0),
                new CustomVertex.PositionTextured(1, 1, 1, 1, 0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0, 1),
                new CustomVertex.PositionTextured(1, -1, 1, 1, 1)
            };
            //vertex buffer de los triangulos
            screenQuadVB = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                4, D3DDevice.Instance.Device, Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionTextured.Format, Pool.Default);
            screenQuadVB.SetData(screenQuadVertices, 0, LockFlags.None);

            //Creamos un Render Targer sobre el cual se va a dibujar la pantalla
            renderTarget2D = new Texture(D3DDevice.Instance.Device,
                D3DDevice.Instance.Device.PresentationParameters.BackBufferWidth
                , D3DDevice.Instance.Device.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);

            //Cargar shader con efectos de Post-Procesado
            effect = TgcShaders.loadEffect(GuiController.Instance.ShadersDir + "PostProcess.fx");

            //Configurar Technique dentro del shader
            effect.Technique = "OscurecerTechnique";

            //Interpolador para efecto de variar la intensidad de la textura de alarma
            intVaivenOscurecer = new InterpoladorVaiven();
            intVaivenOscurecer.Min = 0;
            intVaivenOscurecer.Max = 1;
            intVaivenOscurecer.Speed = 0.4f;
            intVaivenOscurecer.reset();

            //Cargamos un escenario
            var loader = new TgcSceneLoader();
            var scene =
                loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir +
                                         "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");
            meshes = scene.Meshes;

            //Camara en primera personas
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-182.3816f, 82.3252f, -811.9061f),
                new Vector3(-182.0957f, 82.3147f, -810.9479f));

            //Modifier para activar/desactivar efecto de oscurecer
            GuiController.Instance.Modifiers.addBoolean("activar_efecto", "Activar efecto", true);
        }

        public override void render(float elapsedTime)
        {
            //Cargamos el Render Targer al cual se va a dibujar la escena 3D. Antes nos guardamos el surface original
            //En vez de dibujar a la pantalla, dibujamos a un buffer auxiliar, nuestro Render Target.
            pOldRT = D3DDevice.Instance.Device.GetRenderTarget(0);
            var pSurf = renderTarget2D.GetSurfaceLevel(0);
            D3DDevice.Instance.Device.SetRenderTarget(0, pSurf);
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            //Dibujamos la escena comun, pero en vez de a la pantalla al Render Target
            drawSceneToRenderTarget(D3DDevice.Instance.Device);

            //Liberar memoria de surface de Render Target
            pSurf.Dispose();

            //Si quisieramos ver que se dibujo, podemos guardar el resultado a una textura en un archivo para debugear su resultado (ojo, es lento)
            //TextureLoader.Save(GuiController.Instance.ShadersDir + "render_target.bmp", ImageFileFormat.Bmp, renderTarget2D);

            //Ahora volvemos a restaurar el Render Target original (osea dibujar a la pantalla)
            D3DDevice.Instance.Device.SetRenderTarget(0, pOldRT);

            //Luego tomamos lo dibujado antes y lo combinamos con una textura con efecto de alarma
            drawPostProcess(D3DDevice.Instance.Device, elapsedTime);
        }

        /// <summary>
        ///     Dibujamos toda la escena pero en vez de a la pantalla, la dibujamos al Render Target que se cargo antes.
        ///     Es como si dibujaramos a una textura auxiliar, que luego podemos utilizar.
        /// </summary>
        private void drawSceneToRenderTarget(Device d3dDevice)
        {
            //Arrancamos el renderizado. Esto lo tenemos que hacer nosotros a mano porque estamos en modo CustomRenderEnabled = true
            d3dDevice.BeginScene();

            //Como estamos en modo CustomRenderEnabled, tenemos que dibujar todo nosotros, incluso el contador de FPS
            TgcDrawText.Instance.drawText("FPS: " + HighResolutionTimer.Instance.FramesPerSecond, 0, 0,
                Color.Yellow);

            //Tambien hay que dibujar el indicador de los ejes cartesianos
            GuiController.Instance.AxisLines.render();

            //Dibujamos todos los meshes del escenario
            foreach (var m in meshes)
            {
                m.render();
            }

            //Terminamos manualmente el renderizado de esta escena. Esto manda todo a dibujar al GPU al Render Target que cargamos antes
            d3dDevice.EndScene();
        }

        /// <summary>
        ///     Se toma todo lo dibujado antes, que se guardo en una textura, y se le aplica un shader para oscurecer la imagen
        /// </summary>
        private void drawPostProcess(Device d3dDevice, float elapsedTime)
        {
            //Arrancamos la escena
            d3dDevice.BeginScene();

            //Cargamos para renderizar el unico modelo que tenemos, un Quad que ocupa toda la pantalla, con la textura de todo lo dibujado antes
            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, screenQuadVB, 0);

            //Ver si el efecto de oscurecer esta activado, configurar Technique del shader segun corresponda
            var activar_efecto = (bool)GuiController.Instance.Modifiers["activar_efecto"];
            if (activar_efecto)
            {
                effect.Technique = "OscurecerTechnique";
            }
            else
            {
                effect.Technique = "DefaultTechnique";
            }

            //Cargamos parametros en el shader de Post-Procesado
            effect.SetValue("render_target2D", renderTarget2D);
            effect.SetValue("scaleFactor", intVaivenOscurecer.update(elapsedTime));

            //Limiamos la pantalla y ejecutamos el render del shader
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();

            //Terminamos el renderizado de la escena
            d3dDevice.EndScene();
        }

        public override void close()
        {
            foreach (var m in meshes)
            {
                m.dispose();
            }
            effect.Dispose();
            screenQuadVB.Dispose();
            renderTarget2D.Dispose();
        }
    }
}