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
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils;

namespace Examples.DirectX
{
    /// <summary>
    /// Ejemplo EjemploGetZBuffer:
    /// Unidades Involucradas:
    ///      # Unidad 4 - Texturas e Iluminaci�n - Z Buffer
    ///      # Unidad 8 - Adaptadores de Video - Shaders
    /// 
    /// 
    /// Muestra como hacer un primer render de un escenario a una textura guardando ahi el valor de Z.
    /// Seria como Z Buffer pero propio de la aplicacion.
    /// Y luego usar ese Z Buffer como una textura de entrada para una siguiente pasada de shader.
    /// El ejemplo usa el valor de Z para oscurecer los puntos lejos y aclarar los puntos cercanos.
    /// 
    /// 
    /// Autor: Mat�as Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploGetZBuffer : TgcExample
    {

        List<TgcMesh> meshes;
        Texture zBufferTexture;
        Effect effect;
        Surface pOldRT;


        public override string getCategory()
        {
            return "DirectX";
        }

        public override string getName()
        {
            return "Get Z Buffer";
        }

        public override string getDescription()
        {
            return "Muestra como hacer un primer render de un escenario a una textura guardando ahi el valor de Z.";
        }


        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Activamos el renderizado customizado. De esta forma el framework nos delega control total sobre como dibujar en pantalla
            //La responsabilidad cae toda de nuestro lado
            GuiController.Instance.CustomRenderEnabled = true;



            //Cargar shader de este ejemplo
            effect = TgcShaders.loadEffect(GuiController.Instance.ExamplesMediaDir + "Shaders\\EjemploGetZBuffer.fx");


            //Cargamos un escenario
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");
            meshes = scene.Meshes;
            foreach (TgcMesh mesh in meshes)
            {
                mesh.Effect = effect;
            }


            //Crear textura para almacenar el zBuffer. Es una textura que se usa como RenderTarget y que tiene un formato de 1 solo float de 32 bits.
            //En cada pixel no vamos a guardar un color sino el valor de Z de la escena
            //La creamos con un solo nivel de mipmap (el original)
            zBufferTexture = new Texture(d3dDevice, d3dDevice.Viewport.Width, d3dDevice.Viewport.Height, 1, Usage.RenderTarget, Format.R32F, Pool.Default);



            //Camara en 1ra persona
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.MovementSpeed = 400f;
            GuiController.Instance.FpsCamera.JumpSpeed = 300f;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-20, 80, 450), new Vector3(0, 80, 1));
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Guardar render target original
            pOldRT = d3dDevice.GetRenderTarget(0);




            // 1) Mandar a dibujar todos los mesh para que se genere la textura de ZBuffer
            d3dDevice.BeginScene();

            //Seteamos la textura de zBuffer como render  target (en lugar de dibujar a la pantalla)
            Surface zBufferSurface = zBufferTexture.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, zBufferSurface);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            //Render de cada mesh
            foreach (TgcMesh mesh in meshes)
            {
                mesh.Technique = "GenerateZBuffer";
                mesh.render();
            }

            zBufferSurface.Dispose();
            d3dDevice.EndScene();



            // 2) Volvemos a dibujar la escena y pasamos el ZBuffer al shader como una textura.
            // Para este ejemplo particular utilizamos el valor de Z para alterar el color del pixel
            d3dDevice.BeginScene();

            //Restaurar render target original
            d3dDevice.SetRenderTarget(0, pOldRT);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            //Cargar textura de zBuffer al shader
            effect.SetValue("texZBuffer", zBufferTexture);
            effect.SetValue("screenDimensions", new float[]{d3dDevice.Viewport.Width, d3dDevice.Viewport.Height});

            //Render de cada mesh
            foreach (TgcMesh mesh in meshes)
            {
                mesh.Technique = "AlterColorByDepth";
                mesh.render();
            }

            d3dDevice.EndScene();





            //Mostrar FPS
            d3dDevice.BeginScene();
            GuiController.Instance.Text3d.drawText("FPS: " + HighResolutionTimer.Instance.FramesPerSecond, 0, 0, Color.Yellow);
            d3dDevice.EndScene();
            
        }

        public override void close()
        {
            pOldRT.Dispose();
            zBufferTexture.Dispose();
            foreach (TgcMesh mesh in meshes)
            {
                mesh.dispose();
            }
        }

    }
}
