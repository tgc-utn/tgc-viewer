using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils;
using System.Windows.Forms;
using TgcViewer.Example;
using System.Reflection;
using System.Drawing;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.Input;
using System.Globalization;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX;
using TgcViewer.Utils.Sound;
using TgcViewer.Utils.Ui;
using TgcViewer.Utils.Fog;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.Shaders;

namespace TgcViewer
{
    /// <summary>
    /// Controlador principal de la aplicación
    /// </summary>
    public class GuiController
    {
        #region Singleton
        private static volatile GuiController instance;

        /// <summary>
        /// Permite acceder a una instancia de la clase GuiController desde cualquier parte del codigo.
        /// </summary>
        public static GuiController Instance
        {
            get
            { return instance; }
        }

        /// <summary>
        /// Crear nueva instancia. Solo el MainForm lo hace
        /// </summary>
        internal static void newInstance()
        {
            instance = new GuiController();
        }

        #endregion

        MainForm mainForm;
        Control panel3d;
        TgcD3dDevice tgcD3dDevice;
        Logger logger;
        string examplesDir;
        string examplesMediaDir;
        string alumnoEjemplosDir;
        string alumnoEjemplosMediaDir;
        ExampleLoader exampleLoader;
        TgcExample currentExample;
        TgcFpsCamera fpsCamera;
        TgcRotationalCamera rotCamera;
        TgcThirdPersonCamera thirdPersonCamera;
        TgcAxisLines axisLines;
        TgcDrawText text3d;
        TgcUserVars userVars;
        TgcModifiers modifiers;
        bool fpsCounterEnable;
        TgcD3dInput tgcD3dInput;
        float elapsedTime;
        TgcFrustum frustum;
        TgcTexture.Pool texturesPool;
        TgcTexture.Manager texturesManager;
        TgcMp3Player mp3Player;
        TgcDirectSound directSound;
        FullScreenPanel fullScreenPanel;
        TgcFog fog;
        TgcCamera currentCamera;
        bool customRenderEnabled;
        TgcDrawer2D drawer2D;
        TgcShaders shaders;



        #region Internal Methods

        private GuiController()
        {
        }

        /// <summary>
        /// Crea todos los modulos necesarios de la aplicacion
        /// </summary>
        internal void initGraphics(MainForm mainForm, Control panel3d)
        {
            this.mainForm = mainForm;
            this.panel3d = panel3d;
            this.fullScreenPanel = new FullScreenPanel();
            panel3d.Focus();

            //Iniciar graficos
            this.tgcD3dDevice = new TgcD3dDevice(panel3d);
            this.texturesManager = new TgcTexture.Manager();
            this.tgcD3dDevice.OnResetDevice(tgcD3dDevice.D3dDevice, null);

            //Iniciar otras herramientas
            this.texturesPool = new TgcTexture.Pool();
            this.logger = new Logger(mainForm.LogConsole);
            this.text3d = new TgcDrawText(tgcD3dDevice.D3dDevice);
            this.tgcD3dInput = new TgcD3dInput(mainForm, panel3d);
            this.fpsCamera = new TgcFpsCamera();
            this.rotCamera = new TgcRotationalCamera();
            this.thirdPersonCamera = new TgcThirdPersonCamera();
            this.axisLines = new TgcAxisLines(tgcD3dDevice.D3dDevice);
            this.userVars = new TgcUserVars(mainForm.getDataGridUserVars());
            this.modifiers = new TgcModifiers(mainForm.getModifiersPanel());
            this.elapsedTime = -1;
            this.frustum = new TgcFrustum();
            this.mp3Player = new TgcMp3Player();
            this.directSound = new TgcDirectSound();
            this.fog = new TgcFog();
            this.currentCamera = this.rotCamera;
            this.customRenderEnabled = false;
            this.drawer2D = new TgcDrawer2D();
            this.shaders = new TgcShaders();

            //toogles
            this.rotCamera.Enable = true;
            this.fpsCamera.Enable = false;
            this.thirdPersonCamera.Enable = false;
            this.fpsCounterEnable = true;
            this.axisLines.Enable = true;

            //Cargar algoritmos
            exampleLoader = new ExampleLoader();
            examplesDir = System.Environment.CurrentDirectory + "\\" + ExampleLoader.EXAMPLES_DIR + "\\";
            examplesMediaDir = examplesDir + "Media" + "\\";
            alumnoEjemplosDir = System.Environment.CurrentDirectory + "\\" + "AlumnoEjemplos" + "\\";
            alumnoEjemplosMediaDir = alumnoEjemplosDir;
            //alumnoEjemplosMediaDir = alumnoEjemplosDir + "AlumnoMedia" + "\\";
            exampleLoader.loadExamplesInGui(mainForm.TreeViewExamples, new string[] { examplesDir, alumnoEjemplosDir });

            //Cargar shaders del framework
            this.shaders.loadCommonShaders();

            //Cargar ejemplo default
            TgcExample defaultExample = exampleLoader.getExampleByName(mainForm.Config.defaultExampleName, mainForm.Config.defaultExampleCategory);
            executeExample(defaultExample);
        }
       
        /// <summary>
        /// Hacer render del ejemplo
        /// </summary>
        internal void render()
        {
            Device d3dDevice = tgcD3dDevice.D3dDevice;
            elapsedTime = HighResolutionTimer.Instance.FrameTime;

            tgcD3dDevice.doClear();

            //Acutalizar input
            tgcD3dInput.update();

            //Actualizar camaras (solo una va a estar activada a la vez)
            if (currentCamera.Enable)
            {
                this.currentCamera.updateCamera();
                this.currentCamera.updateViewMatrix(d3dDevice);
            }
            
            //actualizar posicion de pantalla en barra de estado de UI
            setStatusPosition();

            //actualizar el Frustum
            frustum.updateVolume(d3dDevice.Transform.View, d3dDevice.Transform.Projection);

            //limpiar texturas
            texturesManager.clearAll();

            //actualizar Listener3D
            directSound.updateListener3d();


            //Hacer render delegando control total al ejemplo
            if (customRenderEnabled)
            {
                //Ejecutar render del ejemplo
                if (currentExample != null)
                {
                    currentExample.render(elapsedTime);
                }
            }

            //Hacer render asistido (mas sencillo para el ejemplo)
            else
            {
                //Iniciar escena 3D
                d3dDevice.BeginScene();

                //Actualizar contador de FPS si esta activo
                if (fpsCounterEnable)
                {
                    text3d.drawText("FPS: " + HighResolutionTimer.Instance.FramesPerSecond, 0, 0, Color.Yellow);
                }

                //Ejecutar render del ejemplo
                if (currentExample != null)
                {
                    currentExample.render(elapsedTime);
                }


                //Ejes cartesianos
                if (axisLines.Enable)
                {
                    axisLines.render();
                }

                //Finalizar escena 3D
                d3dDevice.EndScene();
            }


            
      

            d3dDevice.Present();
            //this.Invalidate();
        }

        /// <summary>
        /// Cuando se selecciona un ejemplo para ejecutar del TreeNode
        /// </summary>
        internal void executeSelectedExample(TreeNode treeNode, bool fullScreenEnable)
        {
            TgcExample example = exampleLoader.getExampleByTreeNode(treeNode);
            executeExample(example);
        }

        /// <summary>
        /// Arranca a ejecutar un ejemplo.
        /// Para el ejemplo anterior, si hay alguno.
        /// </summary>
        /// <param name="example"></param>
        internal void executeExample(TgcExample example)
        {
            stopCurrentExample();
            userVars.clearVars();
            modifiers.clear();
            resetDefaultConfig();
            fpsCamera.resetValues();
            rotCamera.resetValues();
            thirdPersonCamera.resetValues();

            //Ejecutar init
            try
            {
                example.init();

                //Ver si abrimos una ventana para modo FullScreen
                if (FullScreenEnable)
                {
                    mainForm.removePanel3dFromMainForm();
                    fullScreenPanel.Controls.Add(panel3d);
                    fullScreenPanel.Show(mainForm);
                }

                this.currentExample = example;
                panel3d.Focus();
                mainForm.setCurrentExampleStatus("Ejemplo actual: " + example.getName() );
                Logger.log("Ejecutando ejemplo: " + example.getName(), Color.Blue);
            }
            catch (Exception e)
            {
                Logger.logError("Error en init() de ejemplo: " + example.getName(), e);

            }
        }

        /// <summary>
        /// Deja de ejecutar el ejemplo actual
        /// </summary>
        internal void stopCurrentExample()
        {
            if (currentExample != null)
            {
                currentExample.close();
                tgcD3dDevice.resetWorldTransofrm();
                Logger.log("Ejemplo " + currentExample.getName() + " terminado");
                currentExample = null;
                elapsedTime = -1;

                if (FullScreenEnable && fullScreenPanel.Visible)
                {
                    closeFullScreenPanel();
                }
            }
        }

        /// <summary>
        /// Cuando se cierra la ventana de FullScreen
        /// </summary>
        internal void closeFullScreenPanel()
        {
            fullScreenPanel.Controls.Remove(panel3d);
            mainForm.addPanel3dToMainForm();
            fullScreenPanel.Hide();
            panel3d.Focus();
        }

        /// <summary>
        /// Finaliza la ejecución de la aplicacion
        /// </summary>
        internal void shutDown()
        {
            if (currentExample != null)
            {
                currentExample.close();
            }
            tgcD3dDevice.shutDown();
            texturesPool.clearAll();
        }

        /// <summary>
        /// Termina y vuelve a empezar el ejemplo actual, si hay alguno ejecutando.
        /// </summary>
        internal void resetCurrentExample()
        {
            if (currentExample != null)
            {
                TgcExample exampleBackup = currentExample;
                stopCurrentExample();
                executeExample(exampleBackup);
            }
        }

        /// <summary>
        /// Vuelve la configuracion de render y otras cosas a la configuracion inicial
        /// </summary>
        internal void resetDefaultConfig()
        {
            mainForm.resetMenuOptions();
            this.axisLines.Enable = true;
            this.fpsCamera.Enable = false;
            this.rotCamera.Enable = true;
            this.currentCamera = this.rotCamera;
            this.thirdPersonCamera.Enable = false;
            this.fpsCounterEnable = true;
            tgcD3dDevice.setDefaultValues();
            this.mp3Player.closeFile();
            this.fog.resetValues();
            customRenderEnabled = false;
        }

        /// <summary>
        /// Cuando el Direct3D Device se resetea.
        /// Se reinica el ejemplo actual, si hay alguno.
        /// </summary>
        internal void onResetDevice()
        {
            TgcExample exampleBackup = currentExample;
            if (exampleBackup != null)
            {
                stopCurrentExample();
            }
            tgcD3dDevice.doResetDevice();
            if (exampleBackup != null)
            {
                executeExample(exampleBackup);
            }
        }

        /// <summary>
        /// Imprime por consola la posicion actual de la pantalla.
        /// Ideal para copiar y pegar esos valores
        /// </summary>
        internal void printCurrentPosition()
        {
            Logger.log(fpsCamera.getPositionCode());
        }

        /// <summary>
        /// Hace foco en el panel 3D de la aplicacion.
        /// Es util para evitar que el foco quede en otro contro, por ej. un boton,
        /// y que los controles de navegacion respondan mal
        /// </summary>
        internal void focus3dPanel()
        {
            panel3d.Focus();
        }

        /// <summary>
        /// Actualiza en la pantalla principal la posicion actual de la camara
        /// </summary>
        private void setStatusPosition()
        {
            //Actualizar el textbox en todos los cuadros reduce los FPS en algunas PC
            if (mainForm.MostrarPosicionDeCamaraEnable)
            {
                Vector3 pos = currentCamera.getPosition();
                Vector3 lookAt = currentCamera.getLookAt();
                string statusPosition = "Position: [" + TgcParserUtils.printFloat(pos.X) + ", " + TgcParserUtils.printFloat(pos.Y) + ", " + TgcParserUtils.printFloat(pos.Z) + "] " +
                    "- LookAt: [" + TgcParserUtils.printFloat(lookAt.X) + ", " + TgcParserUtils.printFloat(lookAt.Y) + ", " + TgcParserUtils.printFloat(lookAt.Z) + "]";
                mainForm.setStatusPosition(statusPosition);
            }
        }

        



        #endregion



        #region Getters and Setters and Public Methods

        /// <summary>
        /// Direct3D Device
        /// </summary>
        public Device D3dDevice
        {
            get { return tgcD3dDevice.D3dDevice; }
        }

        /// <summary>
        /// Herramienta para loggear mensajes en la consola inferior de la pantalla
        /// </summary>
        public Logger Logger
        {
            get { return logger; }
        }

        /// <summary>
        /// Utilidad de camara en Primera Persona
        /// </summary>
        public TgcFpsCamera FpsCamera
        {
            get { return fpsCamera; }
        }

        /// <summary>
        /// Utilidad de camara que rota alrededor de un objetivo
        /// </summary>
        public TgcRotationalCamera RotCamera
        {
            get { return rotCamera; }
        }

        /// <summary>
        /// Utilidad de camara en tercera persona que sigue a un objeto que se mueve desde atrás
        /// </summary>
        public TgcThirdPersonCamera ThirdPersonCamera
        {
            get { return thirdPersonCamera; }
        }

        /// <summary>
        /// Path absoluto del directorio en donde se encuentran las DLL de los ejemplos
        /// </summary>
        public string ExamplesDir
        {
            get { return examplesDir; }
        }

        /// <summary>
        /// Path absoluto de la carpeta Media que contiene todo el contenido visual de los
        /// ejemplos, como texturas, modelos 3D, etc.
        /// </summary>
        public string ExamplesMediaDir
        {
            get { return examplesMediaDir; }
        }

        /// <summary>
        /// Path absoluto del directorio en donde se encuentran las DLL de los ejemplos del alumno
        /// </summary>
        public string AlumnoEjemplosDir
        {
            get { return alumnoEjemplosDir; }
        }

        /// <summary>
        /// Path absoluto de la carpeta AlumnoMedia que contiene todo el contenido visual de los
        /// ejemplos del alumno
        /// </summary>
        public string AlumnoEjemplosMediaDir
        {
            get { return alumnoEjemplosMediaDir; }
        }
        

        /// <summary>
        /// Utilidad para escribir texto dentro de la pantalla 3D
        /// </summary>
        public TgcDrawText Text3d
        {
            get { return text3d; }
        }

        /// <summary>
        /// Habilita o desactiva el contador de FPS
        /// </summary>
        public bool FpsCounterEnable
        {
            get { return fpsCounterEnable; }
            set { fpsCounterEnable = value; }
        }

        /// <summary>
        /// Utilidad para administrar las variables de usuario visibles en el panel derecho de la aplicación.
        /// </summary>
        public TgcUserVars UserVars
        {
            get { return userVars; }
        }

        /// <summary>
        /// Utilidad para crear modificadores de variables de usuario, que son mostradas en el panel derecho de la aplicación.
        /// </summary>
        public TgcModifiers Modifiers
        {
            get { return modifiers; }
        }

        /// <summary>
        /// Utilidad para visualizar los ejes cartesianos
        /// </summary>
        public TgcAxisLines AxisLines
        {
            get { return axisLines; }
        }

        /// <summary>
        /// Utilidad para acceder al Input de Teclado y Mouse
        /// </summary>
        public TgcD3dInput D3dInput
        {
            get { return tgcD3dInput; }
        }

        /// <summary>
        /// Tiempo en segundos transcurridos desde el último frame.
        /// Solo puede ser invocado cuando se esta ejecutando un bloque de render() de un TgcExample
        /// </summary>
        public float ElapsedTime
        {
            get { return elapsedTime; }
        }

        /// <summary>
        /// Ventana principal de la aplicacion
        /// </summary>
        public MainForm MainForm
        {
            get { return mainForm; }
        }

        /// <summary>
        /// Control gráfico de .NET utilizado para el panel3D sobre el cual renderiza el
        /// Device de Direct3D
        /// </summary>
        public Control Panel3d
        {
            get { return panel3d; }
        }

        /// <summary>
        /// Frustum que representa el volumen de vision actual
        /// Solo puede ser invocado cuando se esta ejecutando un bloque de render() de un TgcExample
        /// </summary>
        public TgcFrustum Frustum
        {
            get { return frustum; }
        }

        /// <summary>
        /// Pool de texturas
        /// </summary>
        public TgcTexture.Pool TexturesPool
        {
            get { return texturesPool; }
        }

        /// <summary>
        /// Herramienta para configurar texturas en el Device
        /// </summary>
        public TgcTexture.Manager TexturesManager
        {
            get { return texturesManager; }
        }

        /// <summary>
        /// Configura la posicion de la cámara
        /// </summary>
        /// <param name="pos">Posición de la cámara</param>
        /// <param name="lookAt">Punto hacia el cuál se quiere ver</param>
        public void setCamera(Vector3 pos, Vector3 lookAt)
        {
            tgcD3dDevice.D3dDevice.Transform.View = Matrix.LookAtLH(pos, lookAt, new Vector3(0, 1, 0));

            //Imprimir posicion
            string statusPos = "Position: [" + TgcParserUtils.printFloat(pos.X) + ", " + TgcParserUtils.printFloat(pos.Y) + ", " + TgcParserUtils.printFloat(pos.Z) + "] " +
                "- LookAt: [" + TgcParserUtils.printFloat(lookAt.X) + ", " + TgcParserUtils.printFloat(lookAt.Y) + ", " + TgcParserUtils.printFloat(lookAt.Z) + "]";
            mainForm.setStatusPosition(statusPos);
        }

        /// <summary>
        /// Herramienta para reproducir archivos MP3s
        /// </summary>
        public TgcMp3Player Mp3Player
        {
            get { return this.mp3Player; }
        }

        /// <summary>
        /// Herramienta para manipular DirectSound
        /// </summary>
        public TgcDirectSound DirectSound
        {
            get { return this.directSound; }
        }

        /// <summary>
        /// Panel que se utiliza para ejecutar un ejemplo en modo FullScreen
        /// </summary>
        public FullScreenPanel FullScreenPanel
        {
            get { return fullScreenPanel; }
        }

        /// <summary>
        /// Indica o configura el modo FullScreen para ejecutar ejemplos.
        /// </summary>
        public bool FullScreenEnable
        {
            get { return mainForm.FullScreenEnable; }
            set { mainForm.FullScreenEnable = value; }
        }

        /// <summary>
        /// Herramienta para manipular el efecto de niebla
        /// </summary>
        public TgcFog Fog
        {
            get { return fog; }
        }

        /// <summary>
        /// Color con el que se limpia la pantalla
        /// </summary>
        public Color BackgroundColor
        {
            get { return tgcD3dDevice.ClearColor; }
            set { tgcD3dDevice.ClearColor = value; }
        }

        /// <summary>
        /// Cámara actual que utiliza el framework
        /// </summary>
        public TgcCamera CurrentCamera
        {
            get { return currentCamera; }
            set { currentCamera = value; }
        }

        /// <summary>
        /// Activa o desactiva el renderizado personalizado.
        /// Si esta activo, el ejemplo tiene la responsabilidad de hacer
        /// el BeginScene y EndScene, y tampoco se dibuja el contador de FPS y Axis.
        /// </summary>
        public bool CustomRenderEnabled
        {
            get { return customRenderEnabled; }
            set { customRenderEnabled = value; }
        }

        /// <summary>
        /// Utilidad para renderizar Sprites 2D
        /// </summary>
        public TgcDrawer2D Drawer2D
        {
            get { return drawer2D; }
        }

        /// <summary>
        /// Utilidad para manejo de shaders
        /// </summary>
        public TgcShaders Shaders
        {
            get { return shaders; }
        }


        #endregion

        




        
    }
}
