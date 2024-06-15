using System.Drawing;
using System.Threading;
using Microsoft.DirectX.Direct3D;
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
    ///     Clase abstracta con las herramientas basicas para poder realizar un juego e interactuar con DirectX 9.
    /// </summary>
    public abstract class TGCExample
    {
        /// <summary>
        ///     FPS minimo para que un ejemplo, pero puede causar problemas con BulletSharp, revisar StepSimulation.
        /// </summary>
        protected const int FPS_30 = 30;

        /// <summary>
        ///     FPS deseado de obtener par aun ejemplo, deberia comportarse correctamente BulletSharp.
        /// </summary>
        protected const int FPS_60 = 60;

        /// <summary>
        ///     FPS sobre lo esperado, pero puede causar problemas con BulletSharp, revisar StepSimulation.
        /// </summary>
        protected const int FPS_120 = 120;

        /// <summary>
        ///     Color por defecto que se usa para llenar la pantalla.
        /// </summary>
        private static readonly Color DefaultClearColor = Color.CornflowerBlue;

        /// <summary>
        ///     Crea un ejemplo con lo necesario para realizar un juego.
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
            TimeBetweenRenders = 1F / FPS_60;
            TimeBetweenUpdates = 1F / FPS_60;
            LastRenderTime = 0;
            LastUpdateTime = 0;
            ElapsedTime = -1;
            Camera = new TgcCamera();
            Timer = new HighResolutionTimer();
            Frustum = new TgcFrustum();
            //DirectSound = new TGCDirectSound(); Por ahora lo carga el Model
            DrawText = new TGCText2D();
            //Input = new TgcD3dInput(); Por ahora lo carga el Model
            BackgroundColor = DefaultClearColor;

            Category = "Others";
            Name = "Ejemplo en Blanco";
            Description = "Ejemplo en Blanco. Es hora de empezar a hacer tu propio ejemplo :)";
        }

        /// <summary>
        ///     Color de fondo del ejemplo.
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        ///     Tiempo en segundos transcurridos desde el ultimo frame.
        /// </summary>
        public float ElapsedTime { get; set; }

        /// <summary>
        ///     Tiempo que paso desde el ultimo render.
        /// </summary>
        public float LastRenderTime { get; set; }

        /// <summary>
        ///     Tiempo que paso desde el ultimo update.
        /// </summary>
        public float LastUpdateTime { get; set; }

        /// <summary>
        ///     Habilita/Deshabilita el render loop a intervalo constante.
        /// </summary>
        public bool FixedTickEnable { get; set; }

        /// <summary>
        ///     Activa o desactiva el contador de frames por segundo.
        /// </summary>
        public bool FPSText { get; set; }

        /// <summary>
        ///     Tiempo que va a pasar entre render = 1/fps deseados.
        /// </summary>
        public float TimeBetweenRenders { get; set; }

        /// <summary>
        ///     Tiempo que va a pasar entre updates = 1/fps deseados.
        /// </summary>
        public float TimeBetweenUpdates { get; set; }

        /// <summary>
        ///     Habilita/Deshabilita el dibujado de los ejes cartesianos.
        /// </summary>
        public bool AxisLinesEnable { get; set; }

        /// <summary>
        ///     Utilidad para visualizar los ejes cartesianos.
        /// </summary>
        public TgcAxisLines AxisLines { get; set; }

        /// <summary>
        ///     Categoria a la que pertenece el ejemplo.
        ///     Influye en donde se va a haber en el arbol de la derecha de la pantalla.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        ///     Completar nombre del grupo en formato Grupo NN.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Completar con la descripcion del TP.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Path de la carpeta Media que contiene el material de los ejemplos, como texturas, modelos 3D, sonido, etc.
        /// </summary>
        public string MediaDir { get; set; }

        /// <summary>
        ///     Path de la carpeta Shaders que contiene todo los shaders genericos.
        /// </summary>
        public string ShadersDir { get; set; }

        /// <summary>
        ///     Camara que esta utilizando el ejemplo.
        /// </summary>
        public TgcCamera Camera { get; set; }

        /// <summary>
        ///     Temporizador del juego de alta resolución.
        /// </summary>
        private HighResolutionTimer Timer { get; }

        /// <summary>
        ///     La región que puede ser vista y renderizada por una cámara.
        /// </summary>
        public TgcFrustum Frustum { get; set; }

        /// <summary>
        ///     Herramienta para manipular el Device de DirectSound.
        /// </summary>
        public TGCDirectSound DirectSound { get; set; }

        /// <summary>
        ///     Herramienta para poder dibujar texto en la pantalla.
        /// </summary>
        public TGCText2D DrawText { get; set; }

        /// <summary>
        ///     Herramienta para el manejo del Input.
        /// </summary>
        public TgcD3dInput Input { get; set; }

        /// <summary>
        ///     Se llama cuando el ejemplo es elegido para ejecutar.
        ///     Inicializar todos los recursos y configuraciones que se van a utilizar.
        /// </summary>
        public abstract void Init();

        /// <summary>
        ///     Se ejecuta una vez por ciclo del render loop, decide si corre en intervalos constantes o sin limite basado en
        ///     FixedTickEnable.
        ///     Internamente ejecuta update y render.
        /// </summary>
        public virtual void Tick()
        {
            if (FixedTickEnable)
            {
                FixedTick();
            }
            else
            {
                UnlimitedTick();
            }
        }

        /// <summary>
        ///     Tick sin limite por FPS.
        /// </summary>
        protected virtual void UnlimitedTick()
        {
            UnlimitedUpdateClock();

            PreUpdate();
            Update();
            PostUpdate();

            Render();
        }

        /// <summary>
        ///     Tick con los FPS constantes, el limite esta puesto por TimeBetweenUpdate.
        /// </summary>
        protected virtual void FixedTick()
        {
            FixedUpdateClock();

            // Tambien es posible que en ciertas maquinas sea necesario agregar:
            // double MaxSkipFrames; constant maximum of frames to skip in the update loop (important to not stall the system on slower computers)
            // while(LastFrameTime >= TimeBetweenUpdate && nLoops < maxSkipFrames)
            // Acumular un loop mas nLoops++;

            while (LastUpdateTime >= TimeBetweenUpdates)
            {
                PreUpdate();
                Update();
                PostUpdate();
                LastUpdateTime -= TimeBetweenUpdates;
            }

            if (LastRenderTime >= TimeBetweenRenders)
            {
                Render();
                LastRenderTime = 0;
            }

            // Como durante muchos ciclos no se hace nada mas que actualizar el clock, se duerme para no consumir gran cantidad de CPU.
            Thread.Sleep(1);
        }

        /// <summary>
        ///     Update de mi modelo.
        /// </summary>
        public abstract void Update();

        /// <summary>
        ///     Se llama para renderizar cada cuadro del ejemplo.
        /// </summary>
        public abstract void Render();

        /// <summary>
        ///     Metodos a ejecutar antes del update.
        ///     Se actualiza el Clock y el Input.
        /// </summary>
        protected virtual void PreUpdate()
        {
            // Para poder permitir intervalos constantes se saco el UpdateClock un nivel arriba.
            //UpdateClock();
            UpdateInput();
            UpdateSounds3D();
        }

        /// <summary>
        ///     Metodos a ejecutar despues del update, se actualizan la Matriz de View y el Frustum de la Camara.
        /// </summary>
        protected virtual void PostUpdate()
        {
            UpdateView();
            UpdateFrustum();
        }

        /// <summary>
        ///     Metodos a ejecutar antes del render.
        /// </summary>
        protected virtual void PreRender()
        {
            BeginRenderScene();
            ClearTextures(); //TODO no se si falta algo mas previo.
        }

        /// <summary>
        ///     Metodos a ejecutar cuando termino el render.
        /// </summary>
        protected virtual void PostRender()
        {
            RenderAxis();
            RenderFPS();
            EndRenderScene();
        }

        /// <summary>
        ///     Actualiza el elapsedTime, importante invocar en cada update loop. Para el loop unlimited.
        /// </summary>
        protected virtual void UnlimitedUpdateClock()
        {
            var FrameTime = Timer.FrameTime;
            Timer.Set();
            LastUpdateTime = FrameTime;
            LastRenderTime = FrameTime;
            ElapsedTime = FrameTime;
        }

        /// <summary>
        ///     Actualiza el elapsedTime, importante invocar en cada update loop. Para el loop fixed.
        /// </summary>
        protected virtual void FixedUpdateClock()
        {
            var FrameTime = Timer.FrameTime;
            Timer.Set();
            LastUpdateTime += FrameTime;
            LastRenderTime += FrameTime;
            ElapsedTime = TimeBetweenUpdates;
        }

        /// <summary>
        ///     Actualiza el Input.
        /// </summary>
        protected void UpdateInput()
        {
            Input.update();
        }

        /// <summary>
        ///     Actualiza la Camara.
        /// </summary>
        protected void UpdateView()
        {
            Camera.UpdateCamera(ElapsedTime);
            D3DDevice.Instance.Device.Transform.View = Camera.GetViewMatrix().ToMatrix();
        }

        /// <summary>
        ///     Actualiza el Frustum.
        /// </summary>
        protected void UpdateFrustum()
        {
            Frustum.updateVolume(TGCMatrix.FromMatrix(D3DDevice.Instance.Device.Transform.View),
                TGCMatrix.FromMatrix(D3DDevice.Instance.Device.Transform.Projection));
        }

        /// <summary>
        ///     Actualiza el Listener3D.
        /// </summary>
        protected void UpdateSounds3D()
        {
            DirectSound.UpdateListener3d();
        }

        /// <summary>
        ///     Limpia la pantalla y inicia la escena 3D.
        /// </summary>
        protected void BeginRenderScene()
        {
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, BackgroundColor, 1.0f, 0);
            BeginScene();
        }

        /// <summary>
        ///     Inicia la escena 3D.
        /// </summary>
        public void BeginScene()
        {
            D3DDevice.Instance.Device.BeginScene();
        }

        /// <summary>
        ///     Limpia las texturas.
        /// </summary>
        protected void ClearTextures()
        {
            TexturesManager.Instance.clearAll();
        }

        /// <summary>
        ///     Dibuja el indicador de los ejes cartesianos.
        /// </summary>
        protected void RenderAxis()
        {
            if (AxisLinesEnable)
            {
                AxisLines.render();
            }
        }

        /// <summary>
        ///     Dibuja el contador de FPS si esta activo.
        /// </summary>
        protected void RenderFPS()
        {
            if (FPSText)
            {
                DrawText.drawText(Timer.FramesPerSecondText(), 0, 0, Color.Yellow);
            }
        }

        /// <summary>
        ///     Finaliza y presenta (se debe hacer al final del render) la escena 3D.
        /// </summary>
        protected void EndRenderScene()
        {
            EndScene();
            D3DDevice.Instance.Device.Present();
        }

        /// <summary>
        ///     Finaliza una escena que se inicio con un BeginScene().
        /// </summary>
        private static void EndScene()
        {
            D3DDevice.Instance.Device.EndScene();
        }

        /// <summary>
        ///     Valores default del Direct3D Device.
        /// </summary>
        public void DeviceDefaultValues()
        {
            D3DDevice.Instance.DefaultValues();
        }

        /// <summary>
        ///     Reinicia el timer para un nuevo juego.
        /// </summary>
        public void ResetTimer()
        {
            Timer.Reset();
            ElapsedTime = -1;
            LastRenderTime = 0;
            LastUpdateTime = 0;
        }

        /// <summary>
        ///     Se llama cuando el ejemplo es cerrado.
        ///     Liberar todos los recursos utilizados.
        ///     OBLIGATORIAMENTE!!!!
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        ///     Vuelve la configuracion de render y otras cosas a la configuracion inicial.
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
