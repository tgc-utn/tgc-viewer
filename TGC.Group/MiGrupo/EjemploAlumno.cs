using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Diagnostics;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Group.MiGrupo
{
    /// <summary>
    ///     Ejemplo del alumno
    /// </summary>
    public class EjemploAlumno : TgcExample
    {
        //Caja que se muestra en el ejemplo
        private TgcBox box;

        public EjemploAlumno(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers, TgcAxisLines axisLines, TgcCamera camara) : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            this.Category = "Alumnos";
            this.Name = "Grupo 99";
            this.Description = "Mi idea - Descripcion de la idea";
        }

        /// <summary>
        ///     Método que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, modifiers, uservars, etc.
        ///     Borrar todo lo que no haga falta
        /// </summary>
        public override void Init()
        {
            //Device de DirectX para crear primitivas
            var d3dDevice = D3DDevice.Instance.Device;

            //Carpeta de archivos Media del alumno
            var alumnoMediaFolder = this.MediaDir;

            ///////////////USER VARS//////////////////

            //Crear una UserVar
            this.UserVars.addVar("variablePrueba");

            //Cargar valor en UserVar
            this.UserVars.setValue("variablePrueba", 5451);

            ///////////////MODIFIERS//////////////////

            //Crear un modifier para un valor FLOAT
            this.Modifiers.addFloat("valorFloat", -50f, 200f, 0f);

            //Crear un modifier para un ComboBox con opciones
            string[] opciones = { "opcion1", "opcion2", "opcion3" };
            this.Modifiers.addInterval("valorIntervalo", opciones, 0);

            //Crear un modifier para modificar un vértice
            this.Modifiers.addVertex3f("valorVertice", new Vector3(-100, -100, -100), new Vector3(50, 50, 50), new Vector3(0, 0, 0));

            ///////////////CONFIGURAR CAMARA ROTACIONAL//////////////////
            //Es la camara que viene por default, asi que no hace falta hacerlo siempre
            //Configurar centro al que se mira y distancia desde la que se mira
            this.Camara.setCamera(new Vector3(100, 100, 100), new Vector3(0, 0, 0));

            /*
            ///////////////CONFIGURAR CAMARA PRIMERA PERSONA//////////////////
            //Camara en primera persona, tipo videojuego FPS
            //Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
            //Por default la camara FPS viene desactivada
            this.Camara = new TgcFpsCamera();
            CamaraManager.Instance.CurrentCamera = camara;
            camara.Enable = true;
            //Configurar posicion y hacia donde se mira
            camara.setCamera(new Vector3(0, 0, -20), new Vector3(0, 0, 0));
            */


            ///////////////LISTAS EN C#//////////////////
            //crear
            var lista = new List<string>();

            //agregar elementos
            lista.Add("elemento1");
            lista.Add("elemento2");

            //obtener elementos
            var elemento1 = lista[0];

            //bucle foreach
            foreach (var elemento in lista)
            {
                Debug.WriteLine(elemento);
            }

            //bucle for
            for (var i = 0; i < lista.Count; i++)
            {
                var element = lista[i];
            }

            //Cargamos una textura
            var texture = TgcTexture.createTexture(this.MediaDir + "Texturas\\baldosaFacultad.jpg");

            //Creamos una caja 3D ubicada en (0, -3, 0), dimensiones (5, 10, 5) y la textura como color.
            var center = new Vector3(0, -3, 0);
            var size = new Vector3(5, 10, 5);
            box = TgcBox.fromSize(center, size, texture);
        }

        public override void Update(float elapsedTime)
        {
            //TODO aca debe ir toda la logica de calculos de lo que fue pasando....
        }

        /// <summary>
        ///     Método que se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aquí todo el código referido al renderizado.
        ///     Borrar todo lo que no haga falta
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public override void Render(float elapsedTime)
        {
            //Ejecuto el render de la super clase
            base.Render(elapsedTime);

            //Device de DirectX para renderizar
            var d3dDevice = D3DDevice.Instance.Device;

            //Obtener valor de UserVar (hay que castear)
            var valor = (int)this.UserVars.getValue("variablePrueba");

            //Obtener valores de Modifiers
            var valorFloat = (float)this.Modifiers["valorFloat"];
            var opcionElegida = (string)this.Modifiers["valorIntervalo"];
            var valorVertice = (Vector3)this.Modifiers["valorVertice"];

            ///////////////INPUT//////////////////
            //conviene deshabilitar ambas camaras para que no haya interferencia

            //Capturar Input teclado
            if (TgcD3dInput.Instance.keyPressed(Key.F))
            {
                //Tecla F apretada
            }

            //Capturar Input Mouse
            if (TgcD3dInput.Instance.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Boton izq apretado
            }

            //Render de la caja
            box.render();
        }

        /// <summary>
        ///     Método que se llama cuando termina la ejecución del ejemplo.
        ///     Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void Close()
        {
            base.Close();

            //Dispose de la caja
            box.dispose();
        }
    }
}