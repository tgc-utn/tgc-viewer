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
        public TgcBox Box { get; set; }

        public EjemploAlumno(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Alumnos";
            Name = "Grupo 99";
            Description = "Mi idea - Descripcion de la idea";
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

            ///////////////USER VARS//////////////////

            //Crear una UserVar
            UserVars.addVar("variablePrueba");

            //Cargar valor en UserVar
            UserVars.setValue("variablePrueba", 5451);

            ///////////////MODIFIERS//////////////////

            //Crear un modifier para un valor FLOAT
            Modifiers.addFloat("valorFloat", -50f, 200f, 0f);

            //Crear un modifier para un ComboBox con opciones
            string[] opciones = { "opcion1", "opcion2", "opcion3" };
            Modifiers.addInterval("valorIntervalo", opciones, 0);

            //Crear un modifier para modificar un vértice
            Modifiers.addVertex3f("valorVertice", new Vector3(-100, -100, -100), new Vector3(50, 50, 50),
                new Vector3(0, 0, 0));

            ///////////////CONFIGURAR CAMARA ROTACIONAL//////////////////
            //Es la camara que viene por default, asi que no hace falta hacerlo siempre
            //Configurar centro al que se mira y distancia desde la que se mira
            Camara.setCamera(new Vector3(100, 100, 100), new Vector3(0, 0, 0));

            /*
            ///////////////CONFIGURAR CAMARA PRIMERA PERSONA//////////////////
            //Camara en primera persona, tipo videojuego FPS
            //Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
            //Por default la camara FPS viene desactivada
            this.Camara = new TgcFpsCamera();
            //Configurar posicion y hacia donde se mira
            this.Camara.setCamera(new Vector3(0, 0, -20), new Vector3(0, 0, 0));
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

            //Textura de la carperta Media
            var mediaFolder = MediaDir + "cajaMadera4.jpg";

            //Cargamos una textura
            var texture = TgcTexture.createTexture(mediaFolder);

            //Creamos una caja 3D ubicada en (0, -3, 0), dimensiones (5, 10, 5) y la textura como color.
            var center = new Vector3(0, -3, 0);
            var size = new Vector3(5, 10, 5);
            Box = TgcBox.fromSize(center, size, texture);
        }

        public override void Update()
        {
            //TODO aca debe ir toda la logica de calculos de lo que fue pasando....
        }

        /// <summary>
        ///     Método que se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aquí todo el código referido al renderizado.
        ///     Borrar todo lo que no haga falta
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public override void Render()
        {
            //Inicio el render de la escena
            IniciarEscena();

            //Device de DirectX para renderizar
            var d3dDevice = D3DDevice.Instance.Device;

            //Obtener valor de UserVar (hay que castear)
            var valor = (int)UserVars.getValue("variablePrueba");

            //Obtener valores de Modifiers
            var valorFloat = (float)Modifiers["valorFloat"];
            var opcionElegida = (string)Modifiers["valorIntervalo"];
            var valorVertice = (Vector3)Modifiers["valorVertice"];

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
            Box.render();

            //Ejecuto el render de la super clase
            base.Render();

            //Finaliza el render
            FinalizarEscena();
        }

        /// <summary>
        ///     Método que se llama cuando termina la ejecución del ejemplo.
        ///     Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void Close()
        {
            //Elimino los recursos de la super clase
            base.Close();

            //Dispose de la caja
            Box.dispose();
        }
    }
}