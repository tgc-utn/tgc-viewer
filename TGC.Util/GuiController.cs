using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core;
using TGC.Core._2D;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Fog;
using TGC.Core.Geometries;
using TGC.Core.Input;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Core.Utils;
using TGC.Util.Example;
using TGC.Util.Input;
using TGC.Util.Modifiers;
using TGC.Util.Sound;
using TGC.Util.Ui;

namespace TGC.Util
{
    /// <summary>
    ///     Controlador principal de la aplicación
    /// </summary>
    public class GuiController
    {
        private TgcExample currentExample;
        private ExampleLoader exampleLoader;
        private TgcD3dDevice tgcD3dDevice;

        #region Singleton

        /// <summary>
        ///     Permite acceder a una instancia de la clase GuiController desde cualquier parte del codigo.
        /// </summary>
        public static GuiController Instance { get; } = new GuiController();

        /// <summary> Constructor privado para poder hacer el singleton</summary>
        private GuiController()
        {
        }

        #endregion Singleton

        #region Internal Methods

        /// <summary>
        ///     Crea todos los modulos necesarios de la aplicacion
        /// </summary>
        public void initGraphics(Form mainForm, Panel panel3d, RichTextBox logConsole, DataGridView dataGridUserVars, Panel modifiersPanel, TreeView treeViewExamples)
        {
            Panel3d = panel3d;
            FullScreenPanel = new FullScreenPanel();
            panel3d.Focus();

            //Iniciar graficos
            //FIXME hay que quitar la dependencia de TgcD3dDevice, necesita un control que es un winform para el device... por ahora se crea aca.
            tgcD3dDevice = new TgcD3dDevice(panel3d);
            tgcD3dDevice.OnResetDevice(D3DDevice.Instance.Device, null);

            //Iniciar otras herramientas
            Logger = new Logger(logConsole);
            D3dInput = new TgcD3dInput(mainForm, panel3d);
            FpsCamera = new TgcFpsCamera();
            RotCamera = new TgcRotationalCamera();
            ThirdPersonCamera = new TgcThirdPersonCamera();
            AxisLines = new TgcAxisLines(D3DDevice.Instance.Device);
            UserVars = new TgcUserVars(dataGridUserVars);
            Modifiers = new TgcModifiers(modifiersPanel);
            ElapsedTime = -1;
            Mp3Player = new TgcMp3Player();
            DirectSound = new TgcDirectSound(mainForm);
            Fog = new TgcFog();
            CamaraManager.Instance.CurrentCamera = RotCamera;
            CustomRenderEnabled = false;

            //toogles
            RotCamera.Enable = true;
            FpsCamera.Enable = false;
            ThirdPersonCamera.Enable = false;
            FpsCounterEnable = true;
            AxisLines.Enable = true;

            //Cargar algoritmos
            exampleLoader = new ExampleLoader();
            ExamplesMediaDir = "Media\\";
            AlumnoMediaDir = "AlumnoMedia\\";
            exampleLoader.loadExamplesInGui(treeViewExamples);

            //Cargar shaders del framework
            TgcShaders.Instance.loadCommonShaders(ExamplesMediaDir + "Shaders\\TgcViewer\\");
        }

        /// <summary>
        ///     Hacer render del ejemplo
        /// </summary>
        public void render()
        {
            var d3dDevice = D3DDevice.Instance.Device;
            ElapsedTime = HighResolutionTimer.Instance.FrameTime;

            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, D3DDevice.Instance.ClearColor, 1.0f, 0);
            HighResolutionTimer.Instance.Set();

            //Acutalizar input
            D3dInput.update();

            //Actualizar camaras (solo una va a estar activada a la vez)
            if (CamaraManager.Instance.CurrentCamera.Enable)
            {
                CamaraManager.Instance.CurrentCamera.updateCamera(ElapsedTime);
                CamaraManager.Instance.CurrentCamera.updateViewMatrix(d3dDevice);
            }

            //actualizar el Frustum
            TgcFrustum.Instance.updateVolume(d3dDevice.Transform.View, d3dDevice.Transform.Projection);

            //limpiar texturas
            TexturesManager.Instance.clearAll();

            //actualizar Listener3D
            DirectSound.updateListener3d();

            //Hacer render delegando control total al ejemplo
            if (CustomRenderEnabled)
            {
                //Ejecutar render del ejemplo
                if (currentExample != null)
                {
                    currentExample.render(ElapsedTime);
                }
            }

            //Hacer render asistido (mas sencillo para el ejemplo)
            else
            {
                //Iniciar escena 3D
                d3dDevice.BeginScene();

                //Actualizar contador de FPS si esta activo
                if (FpsCounterEnable)
                {
                    TgcDrawText.Instance.drawText("FPS: " + HighResolutionTimer.Instance.FramesPerSecond, 0, 0,
                        Color.Yellow);
                }

                //Ejecutar render del ejemplo
                if (currentExample != null)
                {
                    currentExample.render(ElapsedTime);
                }

                //Ejes cartesianos
                if (AxisLines.Enable)
                {
                    AxisLines.render();
                }

                //Finalizar escena 3D
                d3dDevice.EndScene();
            }

            d3dDevice.Present();
            //this.Invalidate();
        }

        /// <summary>
        ///     Finaliza la ejecución de la aplicacion
        /// </summary>
        internal void shutDown()
        {
            if (currentExample != null)
            {
                currentExample.close();
            }
            //Liberar Device al finalizar la aplicacion
            D3DDevice.Instance.Device.Dispose();
            TexturesPool.Instance.clearAll();
        }

        /// <summary>
        ///     Vuelve la configuracion de render y otras cosas a la configuracion inicial
        /// </summary>
        public void resetDefaultConfig()
        {
            AxisLines.Enable = true;
            FpsCamera.Enable = false;
            RotCamera.Enable = true;
            CamaraManager.Instance.CurrentCamera = RotCamera;
            ThirdPersonCamera.Enable = false;
            FpsCounterEnable = true;
            D3DDevice.Instance.setDefaultValues();
            Mp3Player.closeFile();
            Fog.resetValues();
            CustomRenderEnabled = false;
        }

        /// <summary>
        ///     Imprime por consola la posicion actual de la pantalla.
        ///     Ideal para copiar y pegar esos valores
        /// </summary>
        internal void printCurrentPosition()
        {
            Logger.log(FpsCamera.getPositionCode());
        }

        /// <summary>
        ///     Hace foco en el panel 3D de la aplicacion.
        ///     Es util para evitar que el foco quede en otro contro, por ej. un boton,
        ///     y que los controles de navegacion respondan mal
        /// </summary>
        internal void focus3dPanel()
        {
            Panel3d.Focus();
        }

        #endregion Internal Methods

        #region Getters and Setters and Public Methods

        /// <summary>
        ///     Herramienta para loggear mensajes en la consola inferior de la pantalla
        /// </summary>
        public Logger Logger { get; private set; }

        /// <summary>
        ///     Utilidad de camara en Primera Persona
        /// </summary>
        public TgcFpsCamera FpsCamera { get; private set; }

        /// <summary>
        ///     Utilidad de camara que rota alrededor de un objetivo
        /// </summary>
        public TgcRotationalCamera RotCamera { get; private set; }

        /// <summary>
        ///     Utilidad de camara en tercera persona que sigue a un objeto que se mueve desde atrás
        /// </summary>
        public TgcThirdPersonCamera ThirdPersonCamera { get; private set; }

        /// <summary>
        ///     Path absoluto del directorio en donde se encuentran las DLL de los ejemplos
        /// </summary>
        public string ExamplesDir { get; private set; }

        /// <summary>
        ///     Path absoluto de la carpeta Media que contiene todo el contenido visual de los
        ///     ejemplos, como texturas, modelos 3D, etc.
        /// </summary>
        public string ExamplesMediaDir { get; private set; }

        /// <summary>
        ///     Path absoluto del directorio en donde se encuentran las DLL de los ejemplos del alumno
        /// </summary>
        public string AlumnoEjemplosDir { get; private set; }

        /// <summary>
        ///     Path absoluto de la carpeta AlumnoMedia que contiene todo el contenido visual de los
        ///     ejemplos del alumno
        /// </summary>
        public string AlumnoMediaDir { get; private set; }

        /// <summary>
        ///     Habilita o desactiva el contador de FPS
        /// </summary>
        public bool FpsCounterEnable { get; set; }

        /// <summary>
        ///     Utilidad para administrar las variables de usuario visibles en el panel derecho de la aplicación.
        /// </summary>
        public TgcUserVars UserVars { get; private set; }

        /// <summary>
        ///     Utilidad para crear modificadores de variables de usuario, que son mostradas en el panel derecho de la aplicación.
        /// </summary>
        public TgcModifiers Modifiers { get; private set; }

        /// <summary>
        ///     Utilidad para visualizar los ejes cartesianos
        /// </summary>
        public TgcAxisLines AxisLines { get; private set; }

        /// <summary>
        ///     Utilidad para acceder al Input de Teclado y Mouse
        /// </summary>
        public TgcD3dInput D3dInput { get; private set; }

        /// <summary>
        ///     Tiempo en segundos transcurridos desde el último frame.
        ///     Solo puede ser invocado cuando se esta ejecutando un bloque de render() de un TgcExample
        /// </summary>
        public float ElapsedTime { get; set; }

        /// <summary>
        ///     Ventana principal de la aplicacion
        /// </summary>
        public Form MainForm { get; private set; }

        /// <summary>
        ///     Control gráfico de .NET utilizado para el panel3D sobre el cual renderiza el
        ///     Device de Direct3D
        /// </summary>
        public Control Panel3d { get; private set; }

        /// <summary>
        ///     Configura la posicion de la cámara
        /// </summary>
        /// <param name="pos">Posición de la cámara</param>
        /// <param name="lookAt">Punto hacia el cuál se quiere ver</param>
        public void setCamera(Vector3 pos, Vector3 lookAt)
        {
            D3DDevice.Instance.Device.Transform.View = Matrix.LookAtLH(pos, lookAt, new Vector3(0, 1, 0));
        }

        /// <summary>
        ///     Herramienta para reproducir archivos MP3s
        /// </summary>
        public TgcMp3Player Mp3Player { get; private set; }

        /// <summary>
        ///     Herramienta para manipular DirectSound
        /// </summary>
        public TgcDirectSound DirectSound { get; private set; }

        /// <summary>
        ///     Panel que se utiliza para ejecutar un ejemplo en modo FullScreen
        /// </summary>
        public FullScreenPanel FullScreenPanel { get; private set; }

        /// <summary>
        ///     Herramienta para manipular el efecto de niebla
        /// </summary>
        public TgcFog Fog { get; private set; }

        /// <summary>
        ///     Color con el que se limpia la pantalla
        /// </summary>
        public Color BackgroundColor
        {
            get { return D3DDevice.Instance.ClearColor; }
            set { D3DDevice.Instance.ClearColor = value; }
        }

        /// <summary>
        ///     Activa o desactiva el renderizado personalizado.
        ///     Si esta activo, el ejemplo tiene la responsabilidad de hacer
        ///     el BeginScene y EndScene, y tampoco se dibuja el contador de FPS y Axis.
        /// </summary>
        public bool CustomRenderEnabled { get; set; }

        public ExampleLoader ExampleLoader
        {
            get { return exampleLoader; }
            set { exampleLoader = value; }
        }

        public TgcExample CurrentExample
        {
            get { return currentExample; }
            set { currentExample = value; }
        }

        public TgcD3dDevice TgcD3DDevice
        {
            get { return tgcD3dDevice; }
            set { tgcD3dDevice = value; }
        }

        #endregion Getters and Setters and Public Methods
    }
}