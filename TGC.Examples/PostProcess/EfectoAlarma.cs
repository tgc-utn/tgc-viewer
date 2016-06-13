using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Interpolation;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.PostProcess
{
    /// <summary>
    ///     Ejemplo EfectoAlarma:
    ///     Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "Shaders/EjemploShaderTgcMesh".
    ///     Muestra como utilizar la tenica de Render Target para lograr efectos de Post-Procesado.
    ///     Toda la escena no se dibuja a pantalla sino que se dibuja a una textura auxiliar.
    ///     Luego se crea un unico mesh (un Quad) que ocupa toda la pantalla y se le carga como textura
    ///     esta imagen generada antes.
    ///     De esta forma se pueden hacer diversos efectos 2D con pixels shaders sobre la imagen final.
    ///     En este caso, se concatena el dibujo final de la escena con una textura que genera un efecto
    ///     de alarma.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EfectoAlarma : TgcExample
    {
        private TgcTexture alarmTexture;
        private Effect effect;
        private InterpoladorVaiven intVaivenAlarm;
        private List<TgcMesh> meshes;
        private Surface pOldRT;
        private Texture renderTarget2D;
        private VertexBuffer screenQuadVB;

        public EfectoAlarma(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "PostProcess";
            Name = "Efecto Alarma";
            Description = "Graba la escena a un Render Target y luego la combina con una textura de efecto de alarma.";
        }

        public override void Init()
        {
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
            effect = TgcShaders.loadEffect(ShadersDir + "PostProcess.fx");

            //Configurar Technique dentro del shader
            effect.Technique = "AlarmaTechnique";

            //Cargar textura que se va a dibujar arriba de la escena del Render Target
            alarmTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\efecto_alarma.png");

            //Interpolador para efecto de variar la intensidad de la textura de alarma
            intVaivenAlarm = new InterpoladorVaiven();
            intVaivenAlarm.Min = 0;
            intVaivenAlarm.Max = 1;
            intVaivenAlarm.Speed = 5;
            intVaivenAlarm.reset();

            //Cargamos un escenario
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");
            meshes = scene.Meshes;

            //Camara en primera personas
            Camara = new TgcFpsCamera(new Vector3(-182.3816f, 82.3252f, -811.9061f));

            //Modifier para activar/desactivar efecto de alarma
            Modifiers.addBoolean("activar_efecto", "Activar efecto", true);
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            base.Render();

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
            //TextureLoader.Save(this.ShadersDir + "render_target.bmp", ImageFileFormat.Bmp, renderTarget2D);

            //Ahora volvemos a restaurar el Render Target original (osea dibujar a la pantalla)
            D3DDevice.Instance.Device.SetRenderTarget(0, pOldRT);

            //Luego tomamos lo dibujado antes y lo combinamos con una textura con efecto de alarma
            drawPostProcess(D3DDevice.Instance.Device, ElapsedTime);
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
        ///     Se toma todo lo dibujado antes, que se guardo en una textura, y se combina con otra textura, que en este ejemplo
        ///     es para generar un efecto de alarma.
        ///     Se usa un shader para combinar ambas texturas y lograr el efecto de alarma.
        /// </summary>
        private void drawPostProcess(Device d3dDevice, float elapsedTime)
        {
            //Arrancamos la escena
            d3dDevice.BeginScene();

            //Cargamos para renderizar el unico modelo que tenemos, un Quad que ocupa toda la pantalla, con la textura de todo lo dibujado antes
            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, screenQuadVB, 0);

            //Ver si el efecto de alarma esta activado, configurar Technique del shader segun corresponda
            var activar_efecto = (bool)Modifiers["activar_efecto"];
            if (activar_efecto)
            {
                effect.Technique = "AlarmaTechnique";
            }
            else
            {
                effect.Technique = "DefaultTechnique";
            }

            //Cargamos parametros en el shader de Post-Procesado
            effect.SetValue("render_target2D", renderTarget2D);
            effect.SetValue("textura_alarma", alarmTexture.D3dTexture);
            effect.SetValue("alarmaScaleFactor", intVaivenAlarm.update(elapsedTime));

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

        public override void Close()
        {
            base.Close();

            foreach (var m in meshes)
            {
                m.dispose();
            }
            effect.Dispose();
            alarmTexture.dispose();
            screenQuadVB.Dispose();
            renderTarget2D.Dispose();
        }
    }
}