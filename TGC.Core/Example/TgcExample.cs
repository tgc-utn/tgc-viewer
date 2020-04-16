using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.BoundingVolumes;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.Sound;
using TGC.Core.Text;
using TGC.Core.Textures;

namespace TGC.Core.Example
{
    /// <summary>
    /// Clase abstracta con las herramientas basicas para poder realizar un juego e interactuar con DirectX 9.
    /// </summary>
    public abstract class TGCExample
    {
        /// <summary>
        /// Color por defecto que se usa para llenar la pantalla.
        /// </summary>
        private static readonly Color DefaultClearColor = Color.CornflowerBlue;

        /// <summary>
        /// FPS minimo para que un ejemplo, pero puede causar problemas con BulletSharp, revisar StepSimulation.
        /// </summary>
        protected const int FPS_30 = 30;

        /// <summary>
        /// FPS deseado de obtener par aun ejemplo, deberia comportarse correctamente BulletSharp.
        /// </summary>
        protected const int FPS_60 = 60;

        /// <summary>
        /// FPS sobre lo esperado, pero puede causar problemas con BulletSharp, revisar StepSimulation.
        /// </summary>
        protected const int FPS_120 = 120;

        /// <summary>
        /// Crea un ejemplo con lo necesario para realizar un juego.
        /// </summary>
        /// <param name="mediaDir">Ruta donde estan los Media.</param>
        /// <param name="shadersDir">Ruta donde estan los Shaders.</param>
        public TGCExample(string mediaDir, string shadersDir)
        {
            MediaDir = mediaDir;
            ShadersDir = shadersDir;
            AxisLines = new TgcAxisLines();
            AxisLinesEnable = true;
            FPSText = true;
            TimeBetweenFrames = 1F / FPS_60;
            LastUpdateTime = 0;
            ElapsedTime = -1;
            Camera = new TgcCamera();
            Timer = new HighResolutionTimer();
            Frustum = new TgcFrustum();
            //DirectSound = new TgcDirectSound(); Por ahora lo carga el Model
            DrawText = new TgcText2D();
            //Input = new TgcD3dInput(); Por ahora lo carga el Model
            BackgroundColor = DefaultClearColor;

            Category = "Others";
            Name = "Ejemplo en Blanco";
            Description = "Ejemplo en Blanco. Es hora de empezar a hacer tu propio ejemplo :)";
        }

        /// <summary>
        /// Color de fondo del ejemplo.
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Tiempo en segundos transcurridos desde el ultimo frame.
        /// </summary>
        public float ElapsedTime { get; set; }

        /// <summary>
        /// Tiempo que paso desde el ultimo frame.
        /// </summary>
        public float LastUpdateTime { get; set; }

        /// <summary>
        /// Habilita/Deshabilita el update a intervalo constante.
        /// </summary>
        public bool FixedUpdateEnable { get; set; }

        /// <summary>
        /// Activa o desactiva el contador de frames por segundo.
        /// </summary>
        public bool FPSText { get; set; }

        /// <summary>
        /// Tiempo que va a pasar entre frames = 1/fps deseados.
        /// </summary>
        public float TimeBetweenFrames { get; set; }

        /// <summary>
        /// Habilita/Deshabilita el dibujado de los ejes cartesianos.
        /// </summary>
        public bool AxisLinesEnable { get; set; }

        /// <summary>
        /// Utilidad para visualizar los ejes cartesianos.
        /// </summary>
        public TgcAxisLines AxisLines { get; set; }

        /// <summary>
        /// Categoria a la que pertenece el ejemplo.
        /// Influye en donde se va a haber en el arbol de la derecha de la pantalla.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Completar nombre del grupo en formato Grupo NN.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Completar con la descripcion del TP.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Path de la carpeta Media que contiene el material de los ejemplos, como texturas, modelos 3D, sonido, etc.
        /// </summary>
        public string MediaDir { get; set; }

        /// <summary>
        /// Path de la carpeta Shaders que contiene todo los shaders genericos.
        /// </summary>
        public string ShadersDir { get; set; }

        /// <summary>
        /// Camara que esta utilizando el ejemplo.
        /// </summary>
        public TgcCamera Camera { get; set; }

        /// <summary>
        /// Temporizador del juego de alta resolución.
        /// </summary>
        private HighResolutionTimer Timer { get; }

        /// <summary>
        /// La región que puede ser vista y renderizada por una cámara.
        /// </summary>
        public TgcFrustum Frustum { get; set; }

        /// <summary>
        /// Herramienta para manipular el Device de DirectSound.
        /// </summary>
        public TgcDirectSound DirectSound { get; set; }

        /// <summary>
        /// Herramienta para poder dibujar texto en la pantalla.
        /// </summary>
        public TgcText2D DrawText { get; set; }

        /// <summary>
        /// Herramienta para el manejo del Input.
        /// </summary>
        public TgcD3dInput Input { get; set; }

        /// <summary>
        /// Se llama cuando el ejemplo es elegido para ejecutar.
        /// Inicializar todos los recursos y configuraciones que se van a utilizar.
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// Se ejecuta una vez por ciclo del render loop, decide si corre en intervalos constantes o sin limite basado en FixedUpdateEnable.
        /// Internamente ejecuta update y render.
        /// </summary>
        public void Tick()
        {
            if (FixedUpdateEnable)
            {
                FixedTick();
            }
            else
            {
                UnlimitedTick();
            }
        }

        /// <summary>
        /// Tick sin limite por FPS.
        /// </summary>
        protected virtual void UnlimitedTick()
        {
            PreUpdate();
            LastUpdateTime += ElapsedTime;
            UpdateInput();
            Update();
            PostUpdate();

            Render();
        }

        /// <summary>
        /// Tick con los FPS constantes, el limite esta puesto por TimeBetweenFrames.
        /// </summary>
        protected virtual void FixedTick()
        {
            PreUpdate();
            LastUpdateTime += ElapsedTime;

            // Tambien es posible que en ciertas maquinas sea necesario agregar:
            // double MaxSkipFrames; constant maximum of frames to skip in the update loop (important to not stall the system on slower computers)
            // while(LastFrameTime >= TimeBetweenFrames && nLoops < maxSkipFrames)
            // Acumular un loop mas nLoops++;

            while (LastUpdateTime >= TimeBetweenFrames)
            {
                UpdateInput();
                Update();
                LastUpdateTime -= TimeBetweenFrames;
            }

            PostUpdate();
            Render();
        }

        /// <summary>
        /// Update de mi modelo.
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Se llama para renderizar cada cuadro del ejemplo.
        /// </summary>
        public abstract void Render();

        /// <summary>
        /// Metodos a ejecutar antes del update.
        /// Se actualiza el Clock y el Input.
        /// </summary>
        protected virtual void PreUpdate()
        {
            UpdateClock();
            //Por como esta implementado el manejo de Inputs, el mismo tiene estados y para no tener problemas en el update a intervalos constantes se quita de aca y se pone dentro de la condicion.
            //UpdateInput();
            UpdateSounds3D();
        }

        /// <summary>
        /// Metodos a ejecutar despues del update, se acutalizan la Matriz de View y el Frustum de la Camara.
        /// </summary>
        protected virtual void PostUpdate()
        {
            UpdateView();
            UpdateFrustum();
        }

        /// <summary>
        /// Metodos a ejecutar antes del render.
        /// </summary>
        protected virtual void PreRender()
        {
            BeginRenderScene();
            ClearTextures(); //TODO no se si falta algo mas previo.
        }

        /// <summary>
        /// Metodos a ejecutar cuando termino el render.
        /// </summary>
        protected virtual void PostRender()
        {
            RenderAxis();
            RenderFPS();
            EndRenderScene();
        }

        /// <summary>
        /// Actualiza el elapsedTime, importante invocar en cada update loop.
        /// </summary>
        protected void UpdateClock()
        {
            ElapsedTime = Timer.FrameTime;
            Timer.Set();
        }

        /// <summary>
        /// Acutaliza el Input.
        /// </summary>
        protected void UpdateInput()
        {
            Input.update();
        }

        /// <summary>
        /// Actualiza la Camara.
        /// </summary>
        protected void UpdateView()
        {
            Camera.UpdateCamera(ElapsedTime);
            D3DDevice.Instance.Device.Transform.View = Camera.GetViewMatrix().ToMatrix();
        }

        /// <summary>
        /// Acutaliza el Frustum.
        /// </summary>
        protected void UpdateFrustum()
        {
            Frustum.updateVolume(TGCMatrix.FromMatrix(D3DDevice.Instance.Device.Transform.View), TGCMatrix.FromMatrix(D3DDevice.Instance.Device.Transform.Projection));
        }

        /// <summary>
        /// Actualiza el Listener3D.
        /// </summary>
        protected void UpdateSounds3D()
        {
            DirectSound.UpdateListener3d();
        }

        /// <summary>
        /// Limpia la pantalla y inicia la escena 3D.
        /// </summary>
        protected void BeginRenderScene()
        {
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, BackgroundColor, 1.0f, 0);
            BeginScene();
        }

        /// <summary>
        /// Inicia la escena 3D.
        /// </summary>
        public void BeginScene()
        {
            D3DDevice.Instance.Device.BeginScene();
        }

        /// <summary>
        /// Limpia las texturas.
        /// </summary>
        protected void ClearTextures()
        {
            TexturesManager.Instance.clearAll();
        }

        /// <summary>
        /// Dibuja el indicador de los ejes cartesianos.
        /// </summary>
        protected void RenderAxis()
        {
            if (AxisLinesEnable)
            {
                AxisLines.render();
            }
        }

        /// <summary>
        /// Dibuja el contador de FPS si esta activo.
        /// </summary>
        protected void RenderFPS()
        {
            if (FPSText)
            {
                DrawText.drawText(Timer.FramesPerSecondText(), 0, 0, Color.Yellow);
            }
        }

        /// <summary>
        /// Finaliza y presenta (se debe hacer al final del render) la escena 3D.
        /// </summary>
        protected void EndRenderScene()
        {
            EndScene();
            D3DDevice.Instance.Device.Present();
        }

        /// <summary>
        /// Finaliza una escena que se inicio con un BeginScene().
        /// </summary>
        private static void EndScene()
        {
            D3DDevice.Instance.Device.EndScene();
        }

        /// <summary>
        /// Valores default del Direct3D Device.
        /// </summary>
        public void DeviceDefaultValues()
        {
            D3DDevice.Instance.DefaultValues();
        }

        /// <summary>
        /// Reinicia el timer para un nuevo juego.
        /// </summary>
        public void ResetTimer()
        {
            Timer.Reset();
            ElapsedTime = -1;
            LastUpdateTime = 0;
        }

        /// <summary>
        /// Se llama cuando el ejemplo es cerrado.
        /// Liberar todos los recursos utilizados.
        /// OBLIGATORIAMENTE!!!!
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Vuelve la configuracion de Render y otras cosas a la configuracion inicial.
        /// </summary>
        public virtual void ResetDefaultConfig()
        {
            DeviceDefaultValues();
            ResetTimer();
            D3DDevice.Instance.Device.Transform.World = TGCMatrix.Identity.ToMatrix();
            BackgroundColor = DefaultClearColor;
        }
    }
}