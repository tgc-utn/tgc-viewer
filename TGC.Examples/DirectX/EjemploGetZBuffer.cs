using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.DirectX
{
    /// <summary>
    ///     Ejemplo EjemploGetZBuffer:
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminación - Z Buffer
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Muestra como hacer un primer render de un escenario a una textura guardando ahi el valor de Z.
    ///     Seria como Z Buffer pero propio de la aplicacion.
    ///     Y luego usar ese Z Buffer como una textura de entrada para una siguiente pasada de shader.
    ///     El ejemplo usa el valor de Z para oscurecer los puntos lejos y aclarar los puntos cercanos.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploGetZBuffer : TgcExample
    {
        private Effect effect;
        private List<TgcMesh> meshes;
        private Surface pOldRT;
        private Texture zBufferTexture;

        public EjemploGetZBuffer(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "DirectX";
            Name = "Get Z Buffer";
            Description =
                "Muestra como hacer un primer render de un escenario a una textura guardando ahi el valor de Z.";
        }

        public override void Init()
        {
            //Cargar shader de este ejemplo
            effect = TgcShaders.loadEffect(ShadersDir + "EjemploGetZBuffer.fx");

            //Cargamos un escenario
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");
            meshes = scene.Meshes;
            foreach (var mesh in meshes)
            {
                mesh.Effect = effect;
            }

            //Crear textura para almacenar el zBuffer. Es una textura que se usa como RenderTarget y que tiene un formato de 1 solo float de 32 bits.
            //En cada pixel no vamos a guardar un color sino el valor de Z de la escena
            //La creamos con un solo nivel de mipmap (el original)
            zBufferTexture = new Texture(D3DDevice.Instance.Device, D3DDevice.Instance.Device.Viewport.Width,
                D3DDevice.Instance.Device.Viewport.Height, 1,
                Usage.RenderTarget, Format.R32F, Pool.Default);

            //Camara en 1ra persona
            Camara = new TgcFpsCamera(new Vector3(-20, 80, 450), 400f, 300f);
        }

        public override void Update()
        {
            base.helperPreUpdate();
        }

        public override void Render()
        {
            base.helperRenderClearTextures();
            //Guardar render target original
            pOldRT = D3DDevice.Instance.Device.GetRenderTarget(0);

            // 1) Mandar a dibujar todos los mesh para que se genere la textura de ZBuffer
            D3DDevice.Instance.Device.BeginScene();

            //Seteamos la textura de zBuffer como render  target (en lugar de dibujar a la pantalla)
            var zBufferSurface = zBufferTexture.GetSurfaceLevel(0);
            D3DDevice.Instance.Device.SetRenderTarget(0, zBufferSurface);
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            //Render de cada mesh
            foreach (var mesh in meshes)
            {
                mesh.Technique = "GenerateZBuffer";
                mesh.render();
            }

            zBufferSurface.Dispose();
            D3DDevice.Instance.Device.EndScene();

            // 2) Volvemos a dibujar la escena y pasamos el ZBuffer al shader como una textura.
            // Para este ejemplo particular utilizamos el valor de Z para alterar el color del pixel
            D3DDevice.Instance.Device.BeginScene();

            //Restaurar render target original
            D3DDevice.Instance.Device.SetRenderTarget(0, pOldRT);
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            //Cargar textura de zBuffer al shader
            effect.SetValue("texZBuffer", zBufferTexture);
            effect.SetValue("screenDimensions",
                new float[] { D3DDevice.Instance.Device.Viewport.Width, D3DDevice.Instance.Device.Viewport.Height });

            //Render de cada mesh
            foreach (var mesh in meshes)
            {
                mesh.Technique = "AlterColorByDepth";
                mesh.render();
            }

            base.helperRenderFPS();
            base.helperRenderAxis();
            D3DDevice.Instance.Device.EndScene();
            D3DDevice.Instance.Device.Present();
        }

        public override void Close()
        {
            base.Close();

            pOldRT.Dispose();
            zBufferTexture.Dispose();
            foreach (var mesh in meshes)
            {
                mesh.dispose();
            }
        }
    }
}