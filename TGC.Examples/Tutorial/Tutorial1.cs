using Microsoft.DirectX;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.Tutorial
{
    /// <summary>
    ///     Tutorial 1:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Muestra como crear una caja 3D de color y como mostrarla por pantalla.
    ///     Autor: Matías Leone
    /// </summary>
    public class Tutorial1 : TgcExample
    {
        //Variable para caja 3D
        private TgcBox box;

        public Tutorial1(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Tutorial";
            Name = "Tutorial 1";
            Description = "Muestra como crear una caja 3D de color y como mostrarla por pantalla.";
        }

        /// <summary>
        ///     Método en el que se deben crear todas las cosas que luego se van a querer usar.
        ///     Es invocado solo una vez al inicio del ejemplo.
        /// </summary>
        public override void Init()
        {
            //Creamos una caja 3D de color rojo, ubicada en el origen y lado 10
            var center = new Vector3(0, 0, 0);
            var size = new Vector3(10, 10, 10);
            var color = Color.Red;
            box = TgcBox.fromSize(center, size, color);

            //Todos los recursos que se van a necesitar (objetos 3D, meshes, texturas, etc) se deben cargar en el metodo init().
            //Crearlos cada vez en el metodo render() es un error grave. Destruye la performance y suele provocar memory leaks.

            //Ubicar la camara del framework mirando al centro de este objeto.
            //La camara por default del framework es RotCamera, cuyo comportamiento es
            //centrarse sobre un objeto y permitir rotar y hacer zoom con el mouse.
            //Con clic izquierdo del mouse se rota la cámara, con clic derecho se traslada y con la rueda
            //del mouse se hace zoom.
            //Otras cámaras disponibles son: FpsCamera (1ra persona) y ThirdPersonCamera (3ra persona).
            Camara = new TgcRotationalCamera(box.BoundingBox);
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Método que se invoca todo el tiempo. Es el render-loop de una aplicación gráfica.
        ///     En este método se deben dibujar todos los objetos que se desean mostrar.
        ///     Antes de llamar a este método el framework limpia toda la pantalla.
        ///     Por lo tanto para que un objeto se vea hay volver a dibujarlo siempre.
        ///     La variable elapsedTime indica la cantidad de segundos que pasaron entre esta invocación
        ///     y la anterior de render(). Es útil para animar e interpolar valores.
        /// </summary>
        public override void Render()
        {
            //Iniciamoss la escena
            IniciarEscena();
            //Render de la super clase
            base.Render();

            //Dibujar la caja en pantalla
            box.render();

            //Finalizamos el renderizado de la escena
            FinalizarEscena();
        }

        /// <summary>
        ///     Método que se invoca una sola vez al finalizar el ejemplo.
        ///     Se debe liberar la memoria de todos los recursos utilizados.
        /// </summary>
        public override void Close()
        {
            base.Close();

            //Liberar memoria de la caja 3D.
            //Por mas que estamos en C# con Garbage Collector igual hay que liberar la memoria de los recursos gráficos.
            //Porque están utilizando memoria de la placa de video (y ahí no hay Garbage Collector).
            box.dispose();
        }
    }
}