using Microsoft.DirectX;
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
        ///     Tiempo en segundos transcurridos desde el ultimo frame.
        /// </summary>
        public float ElapsedTime { get; set; }

        /// <summary>
        ///     Activa o desactiva el contador de frames por segundo.
        /// </summary>
        public bool FPS { get; set; }

        /// <summary>
        ///     Utilidad para visualizar los ejes cartesianos
        /// </summary>
        public TgcAxisLines AxisLines { get; set; }

        /// <summary>
        ///     Categoría a la que pertenece el ejemplo.
        ///     Influye en donde se va a haber en el árbol de la derecha de la pantalla.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        ///     Completar nombre del grupo en formato Grupo NN
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Completar con la descripción del TP
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Path de la carpeta Media que contiene todo el contenido visual de los ejemplos, como texturas, modelos 3D, etc.
        /// </summary>
        public string MediaDir { get; set; }

        /// <summary>
        ///     Path de la carpeta Shaders que contiene todo los shaders genericos
        /// </summary>
        public string ShadersDir { get; set; }

        /// <summary>
        ///     Cámara que esta utilizando el ejemplo
        /// </summary>
        public TgcCamera Camara { get; set; }

        /// <summary>
        ///     Utilidad para administrar las variables de usuario visibles en el panel derecho de la aplicacion.
        /// </summary>
        public TgcUserVars UserVars { get; set; }

        /// <summary>
        ///     Utilidad para crear modificadores de variables de usuario, que son mostradas en el panel derecho de la aplicacion.
        /// </summary>
        public TgcModifiers Modifiers { get; set; }

        /// <summary>
        ///     Se llama cuando el ejemplo es elegido para ejecutar.
        ///     Inicializar todos los recursos y configuraciones que se van a utilizar.
        /// </summary>
        public abstract void Init();

        /// <summary>
        ///     Update de mi modelo
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public abstract void Update();

        /// <summary>
        ///     Se llama para renderizar cada cuadro del ejemplo.
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public virtual void Render()
        {
            ElapsedTime = HighResolutionTimer.Instance.FrameTime;
            D3DDevice.Instance.Clear();
            HighResolutionTimer.Instance.Set();

            //Acutalizar input
            TgcD3dInput.Instance.update();

            //Actualizar la camara
            Camara.updateCamera(ElapsedTime); //FIXME esto deberia hacerce en el update.
            D3DDevice.Instance.Device.Transform.View = Camara.getViewMatrix();

            //actualizar el Frustum
            TgcFrustum.Instance.updateVolume(D3DDevice.Instance.Device.Transform.View,
                D3DDevice.Instance.Device.Transform.Projection);

            //limpiar texturas
            TexturesManager.Instance.clearAll();

            //actualizar Listener3D
            TgcDirectSound.Instance.updateListener3d();

            //Actualizar contador de FPS si esta activo
            if (FPS)
            {
                DrawFPS();
            }

            //Hay que dibujar el indicador de los ejes cartesianos
            if (AxisLines.Enable)
            {
                AxisLines.render();
            }
        }

        private void DrawFPS()
        {
            TgcDrawText.Instance.drawText("FPS: " + HighResolutionTimer.Instance.FramesPerSecond, 0, 0, Color.Yellow);
        }

        /// <summary>
        ///     Se llama cuando el ejemplo es cerrado.
        ///     Liberar todos los recursos utilizados.
        /// </summary>
        public virtual void Close()
        {
            D3DDevice.Instance.Device.Transform.World = Matrix.Identity;
            UserVars.ClearVars();
            Modifiers.Clear();
            ElapsedTime = -1;
        }

        /// <summary>
        ///     Iniciar escena 3D
        /// </summary>
        public void IniciarEscena()
        {
            D3DDevice.Instance.Device.BeginScene();
        }

        /// <summary>
        ///     Finalizar escena 3D
        /// </summary>
        public void FinalizarEscena()
        {
            D3DDevice.Instance.Device.EndScene();
            D3DDevice.Instance.Device.Present();
        }

        /// <summary>
        ///     Vuelve la configuracion de Render y otras cosas a la configuracion inicial
        /// </summary>
        public void ResetDefaultConfig()
        {
            D3DDevice.Instance.DefaultValues();
        }
    }
}