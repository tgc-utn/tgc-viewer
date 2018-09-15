using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Input;
using TGC.Core.Shaders;
using TGC.Core.Sound;
using TGC.Core.Textures;
using TGC.Viewer.UI;

namespace TGC.Viewer.Model
{
    public class ViewerModel
    {
        private ViewerForm Form { get; set; }

        /// <summary>
        /// Obtener o parar el estado del RenderLoop.
        /// </summary>
        public bool ApplicationRunning { get; set; }

        /// <summary>
        /// Controlador de sonido.
        /// </summary>
        private TgcDirectSound DirectSound { get; set; }

        /// <summary>
        /// Controlador de inputs.
        /// </summary>
        private TgcD3dInput Input { get; set; }

        /// <summary>
        /// Cargador de ejemplos.
        /// </summary>
        private ExampleLoader ExampleLoader { get; set; }

        /// <summary>
        /// Inicia el device basado en el panel, el sonido, los inputs y carga los shaders basicos.
        /// </summary>
        /// <param name="form"> Ventana que contiene la aplicacion.</param>
        /// <param name="control"> Control donde van a correr los ejemplos.</param>
        /// <param name="pathCommonShaders"> Ruta con los shaders basicos.</param>
        public void InitGraphics(ViewerForm form, Control control, string pathCommonShaders)
        {
            ApplicationRunning = true;
            Form = form;

            //Inicio Device
            D3DDevice.Instance.InitializeD3DDevice(control);
            D3DDevice.Instance.Device.DeviceReset += OnResetDevice;

            //Inicio inputs
            Input = new TgcD3dInput();
            Input.Initialize(Form, control);

            //Inicio sonido
            DirectSound = new TgcDirectSound();
            DirectSound.InitializeD3DDevice(control);

            //Cargar shaders del framework
            TgcShaders.Instance.loadCommonShaders(pathCommonShaders);
        }

        /// <summary>
        /// Verifica si existe la carpeta.
        /// </summary>
        /// <param name="path"> Ruta a verificar.</param>
        /// <returns> True si la carpeta no existe.</returns>
        public bool CheckFolder(string path)
        {
            return !Directory.Exists(path);
        }

        /// <summary>
        /// Carga los ejemplos de TGC.Examples al arbol de la aplicacion.
        /// </summary>
        /// <param name="treeViewExamples"> Arbol donde se cargaran los ejemplos.</param>
        /// <param name="panelModifiers"> Panel donde van los modificadores del ejemplo.</param>
        /// <param name="dataGridUserVars"> Panel donde van los datos del ejemplo.</param>
        /// <param name="mediaDirectory"> Ruta donde estan los media.</param>
        /// <param name="shadersDirectory"> Ruta donde estan los shaders.</param>
        public void LoadExamples(TreeView treeViewExamples, Panel panelModifiers, DataGridView dataGridUserVars, string mediaDirectory, string shadersDirectory)
        {
            //Directorio actual de ejecucion
            var currentDirectory = Environment.CurrentDirectory + "\\";

            //Cargo los ejemplos en el arbol
            ExampleLoader = new ExampleLoader(mediaDirectory, shadersDirectory, dataGridUserVars, panelModifiers);
            ExampleLoader.LoadExamplesInGui(treeViewExamples, currentDirectory);
        }

        /// <summary>
        /// Ejemplo actual de la aplicacion.
        /// </summary>
        /// <returns></returns>
        public TgcExample CurrentExample()
        {
            return ExampleLoader.CurrentExample;
        }

        /// <summary>
        /// Obtiene un ejemplo por su nombre y categoria.
        /// </summary>
        /// <param name="defaultExampleName"> Nombre del ejemplo.</param>
        /// <param name="defaultExampleCategory"> Categoria del ejemplo.</param>
        /// <returns></returns>
        public TgcExample GetExampleByName(string defaultExampleName, string defaultExampleCategory)
        {
            return ExampleLoader.GetExampleByName(defaultExampleName, defaultExampleCategory);
        }

        /// <summary>
        /// Se inicia el render loop del ejemplo.
        /// </summary>
        public void InitRenderLoop()
        {
            while (ApplicationRunning)
            {
                //Renderizo si es que hay un ejemplo activo
                if (ExampleLoader.CurrentExample != null)
                {
                    //Solo renderizamos si la aplicacion tiene foco, para no consumir recursos innecesarios
                    if (Form.ApplicationActive())
                    {
                        ExampleLoader.CurrentExample.Update();
                        ExampleLoader.CurrentExample.Render();
                    }
                    else
                    {
                        //Si no tenemos el foco, dormir cada tanto para no consumir gran cantidad de CPU
                        Thread.Sleep(100);
                    }
                }
                // Process application messages
                Application.DoEvents();
            }
        }

        /// <summary>
        /// Le activa o desactiva la herramienta de wireframe al ejemplo.
        /// </summary>
        /// <param name="state"> Estado que se quiere de la herramienta.</param>
        public void Wireframe(bool state)
        {
            if (state)
            {
                D3DDevice.Instance.FillModeWireFrame();
            }
            else
            {
                D3DDevice.Instance.FillModeWireSolid();
            }
        }

        /// <summary>
        /// Le activa o desactiva el contador de FPS al ejemplo.
        /// </summary>
        /// <param name="state"> Estado que se quiere de la herramienta.</param>
        public void ContadorFPS(bool state)
        {
            ExampleLoader.CurrentExample.FPS = state;
        }

        /// <summary>
        /// Le activa o desactiva los ejes cartesianos al ejemplo.
        /// </summary>
        /// <param name="state"> Estado que se quiere de la herramienta.</param>
        public void AxisLines(bool state)
        {
            ExampleLoader.CurrentExample.AxisLinesEnable = state;
        }

        /// <summary>
        ///  Arranca a ejecutar un ejemplo.
        ///  Para el ejemplo anterior, si hay alguno.
        /// </summary>
        /// <param name="example"></param>
        public void ExecuteExample(TgcExample example)
        {
            StopCurrentExample();

            //Ejecutar Init
            ExampleLoader.CurrentExample = example;
            //TODO esto no me cierra mucho OnResetDevice
            OnResetDevice(D3DDevice.Instance.Device, null);
            example.ResetDefaultConfig();
            example.DirectSound = DirectSound;
            example.Input = Input;
            example.Init();
        }

        /// <summary>
        ///  Deja de ejecutar el ejemplo actual
        /// </summary>
        public void StopCurrentExample()
        {
            if (ExampleLoader.CurrentExample != null)
            {
                ExampleLoader.CurrentExample.Dispose();
                ExampleLoader.CurrentExample = null;
            }
        }

        public void Dispose()
        {
            ApplicationRunning = false;

            StopCurrentExample();

            //Liberar Device al finalizar la aplicacion
            D3DDevice.Instance.Dispose();
            TexturesPool.Instance.clearAll();
        }

        /// <summary>
        /// Actualiza los directorios donde estan los medios y los shaders.
        /// </summary>
        public void UpdateMediaAndShaderDirectories(string mediaDirectory, string shadersDirectory)
        {
            if (ExampleLoader != null)
            {
                ExampleLoader.UpdateMediaAndShaderDirectories(mediaDirectory, shadersDirectory);
            }
        }

        /// <summary>
        /// Actualiza el aspect ratio segun el estado del panel.
        /// </summary>
        /// <param name="panel"></param>
        public void UpdateAspectRatio(Panel panel)
        {
            D3DDevice.Instance.UpdateAspectRatioAndProjection(panel.Width, panel.Height);
        }

        /// <summary>
        /// Obtiene el ejemplo del nodo seleccionado.
        /// </summary>
        /// <param name="selectedNode"></param>
        /// <returns></returns>
        public TgcExample GetExampleByTreeNode(TreeNode selectedNode)
        {
            return ExampleLoader.GetExampleByTreeNode(selectedNode);
        }

        /// <summary>
        /// Cuando el Direct3D Device se resetea.
        /// Se reinica el ejemplo actual, si hay alguno.
        /// </summary>
        public void OnResetDevice()
        {
            var exampleBackup = ExampleLoader.CurrentExample;

            if (exampleBackup != null)
            {
                StopCurrentExample();
            }

            DoResetDevice();

            if (exampleBackup != null)
            {
                ExecuteExample(exampleBackup);
            }
        }

        /// <summary>
        /// Limpia el atributo de ExamplerLoader.
        /// </summary>
        public void ClearCurrentExample()
        {
            ExampleLoader.CurrentExample = null;
        }

        /// <summary>
        /// This event-handler is a good place to create and initialize any Direct3D related objects, which may become invalid during a device reset.
        /// </summary>
        public void OnResetDevice(object sender, EventArgs e)
        {
            //TODO antes hacia esto que no entiendo porque GuiController.Instance.onResetDevice();
            //pero solo detenia el ejemplo ejecutaba doResetDevice y lo volvia a cargar...
            DoResetDevice();
        }

        /// <summary>
        /// Hace las operaciones de Reset del device.
        /// </summary>
        public void DoResetDevice()
        {
            //Default values para el device
            ExampleLoader.CurrentExample.DeviceDefaultValues();

            //Reset Timer
            ExampleLoader.CurrentExample.ResetTimer();
        }
    }
}