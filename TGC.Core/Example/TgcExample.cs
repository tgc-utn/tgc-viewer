using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core._2D;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Sound;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Core.Example
{
    public abstract class TgcExample
    {
        private static readonly Color DEFAULT_CLEAR_COLOR = Color.FromArgb(255, 78, 129, 179);

        public TgcExample(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
        {
            MediaDir = mediaDir;
            ShadersDir = shadersDir;
            UserVars = userVars;
            Modifiers = modifiers;
            AxisLines = axisLines;
            AxisLines.Enable = true;
            Camara = camara;
            FPS = true;
            ElapsedTime = -1;

            Category = "Others";
            Name = "Ejemplo en Blanco";
            Description = "Ejemplo en Blanco. Es hora de empezar a hacer tu propio ejemplo :)";
        }

        /// <summary>
        /// Tiempo en segundos transcurridos desde el ultimo frame.
        /// </summary>
        public float ElapsedTime { get; set; }

        /// <summary>
        /// Activa o desactiva el contador de frames por segundo.
        /// </summary>
        public bool FPS { get; set; }

        /// <summary>
        /// Utilidad para visualizar los ejes cartesianos
        /// </summary>
        public TgcAxisLines AxisLines { get; set; }

        /// <summary>
        /// Categoria a la que pertenece el ejemplo.
        /// Influye en donde se va a haber en el arbol de la derecha de la pantalla.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Completar nombre del grupo en formato Grupo NN
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Completar con la descripcion del TP
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Path de la carpeta Media que contiene todo el contenido visual de los ejemplos, como texturas, modelos 3D, etc.
        /// </summary>
        public string MediaDir { get; set; }

        /// <summary>
        /// Path de la carpeta Shaders que contiene todo los shaders genericos
        /// </summary>
        public string ShadersDir { get; set; }

        /// <summary>
        /// Camara que esta utilizando el ejemplo
        /// </summary>
        public TgcCamera Camara { get; set; }

        /// <summary>
        /// Utilidad para administrar las variables de usuario visibles en el panel derecho de la aplicacion.
        /// </summary>
        public TgcUserVars UserVars { get; set; }

        /// <summary>
        /// Utilidad para crear modificadores de variables de usuario, que son mostradas en el panel derecho de la aplicacion.
        /// </summary>
        public TgcModifiers Modifiers { get; set; }

        /// <summary>
        /// Se llama cuando el ejemplo es elegido para ejecutar.
        /// Inicializar todos los recursos y configuraciones que se van a utilizar.
        /// </summary>
        public abstract void Init();

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
		/// </summary>
		protected virtual void PreUpdate()
        {
            UpdateClock();
            UpdateInput();
            UpdateView();
            UpdateFrustum();
            UpdateSounds3D();
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
            ElapsedTime = HighResolutionTimer.Instance.FrameTime;
            HighResolutionTimer.Instance.Set();
        }

        /// <summary>
        /// Acutaliza el input
        /// </summary>
		protected void UpdateInput()
        {
            TgcD3dInput.Instance.update();
        }

        /// <summary>
        /// Actualiza la Camara
        /// </summary>
		protected void UpdateView()
        {
            Camara.updateCamera(ElapsedTime); //FIXME esto deberia hacerce en el update.
            D3DDevice.Instance.Device.Transform.View = Camara.getViewMatrix();
        }

        /// <summary>
        /// Acutaliza el Frustum
        /// </summary>
		protected void UpdateFrustum()
        {
            TgcFrustum.Instance.updateVolume(D3DDevice.Instance.Device.Transform.View,
                     D3DDevice.Instance.Device.Transform.Projection);
        }

        /// <summary>
        /// Actualiza el Listener3D
        /// </summary>
		protected void UpdateSounds3D()
        {
            TgcDirectSound.Instance.updateListener3d();
        }

        /// <summary>
        /// Inicia la escena 3D
        /// </summary>
		protected void BeginRenderScene()
        {
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, DEFAULT_CLEAR_COLOR, 1.0f, 0);
            D3DDevice.Instance.Device.BeginScene();
        }

        /// <summary>
		/// Limpia las texturas
        /// </summary>
        protected void ClearTextures()
        {
            TexturesManager.Instance.clearAll();
        }

        /// <summary>
        ///  Dibuja el indicador de los ejes cartesianos
        /// </summary>
        protected void RenderAxis()
        {
            if (AxisLines.Enable)
            {
                AxisLines.render();
            }
        }

        /// <summary>
        /// Dibuja el contador de FPS si esta activo
        /// </summary>
        protected void RenderFPS()
        {
            if (FPS)
            {
                TgcDrawText.Instance.drawText("FPS: " + HighResolutionTimer.Instance.FramesPerSecond, 0, 0, Color.Yellow);
            }
        }

        /// <summary>
        /// Finaliza la escena 3D
        /// </summary>
		protected void EndRenderScene()
        {
            D3DDevice.Instance.Device.EndScene();
            D3DDevice.Instance.Device.Present();
        }

        /// <summary>
        /// Se llama cuando el ejemplo es cerrado.
        /// Liberar todos los recursos utilizados. OBLIGATORIAMENTE!!!!
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Vuelve la configuracion de Render y otras cosas a la configuracion inicial
        /// </summary>
        public void ResetDefaultConfig()
        {
            D3DDevice.Instance.DefaultValues();
            D3DDevice.Instance.Device.Transform.World = Matrix.Identity;
            UserVars.ClearVars();
            Modifiers.Clear();
            ElapsedTime = -1;
        }
    }
}