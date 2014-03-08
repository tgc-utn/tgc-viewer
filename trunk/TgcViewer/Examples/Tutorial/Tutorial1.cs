using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.Tutorial
{
    /// <summary>
    /// Tutorial 1:
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos B�sicos de 3D - Mesh
    /// 
    /// Muestra como crear una caja 3D de color y como mostrarla por pantalla.
    /// 
    /// Autor: Mat�as Leone
    /// 
    /// </summary>
    public class Tutorial1 : TgcExample
    {

        //Variable para caja 3D
        TgcBox box;


        public override string getCategory()
        {
            return "Tutorial";
        }

        public override string getName()
        {
            return "Tutorial 1";
        }

        public override string getDescription()
        {
            return "Muestra como crear una caja 3D de color y como mostrarla por pantalla.";
        }

        /// <summary>
        /// M�todo en el que se deben crear todas las cosas que luego se van a querer usar.
        /// Es invocado solo una vez al inicio del ejemplo.
        /// </summary>
        public override void init()
        {
            //Acceso a Device de DirectX. Siempre conviene tenerlo a mano. Suele ser pedido como par�metro de varios m�todos
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Creamos una caja 3D de color rojo, ubicada en el origen y lado 10
            Vector3 center = new Vector3(0, 0, 0);
            Vector3 size = new Vector3(10, 10, 10);
            Color color = Color.Red;
            box = TgcBox.fromSize(center, size, color);

            //Todos los recursos que se van a necesitar (objetos 3D, meshes, texturas, etc) se deben cargar en el metodo init().
            //Crearlos cada vez en el metodo render() es un error grave. Destruye la performance y suele provocar memory leaks.


            //Ubicar la camara del framework mirando al centro de este objeto.
            //La camara por default del framework es RotCamera, cuyo comportamiento es
            //centrarse sobre un objeto y permitir rotar y hacer zoom con el mouse.
            //Con clic izquierdo del mouse se rota la c�mara, con clic derecho se traslada y con la rueda
            //del mouse se hace zoom.
            //Otras c�maras disponibles son: FpsCamera (1ra persona) y ThirdPersonCamera (3ra persona).
            GuiController.Instance.RotCamera.targetObject(box.BoundingBox);
        }

        /// <summary>
        /// M�todo que se invoca todo el tiempo. Es el render-loop de una aplicaci�n gr�fica.
        /// En este m�todo se deben dibujar todos los objetos que se desean mostrar.
        /// Antes de llamar a este m�todo el framework limpia toda la pantalla. 
        /// Por lo tanto para que un objeto se vea hay volver a dibujarlo siempre.
        /// La variable elapsedTime indica la cantidad de segundos que pasaron entre esta invocaci�n
        /// y la anterior de render(). Es �til para animar e interpolar valores.
        /// </summary>
        public override void render(float elapsedTime)
        {
            //Acceso a Device de DirectX. Siempre conviene tenerlo a mano. Suele ser pedido como par�metro de varios m�todos
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Dibujar la caja en pantalla
            box.render();
        }

        /// <summary>
        /// M�todo que se invoca una sola vez al finalizar el ejemplo.
        /// Se debe liberar la memoria de todos los recursos utilizados.
        /// </summary>
        public override void close()
        {
            //Liberar memoria de la caja 3D.
            //Por mas que estamos en C# con Garbage Collector igual hay que liberar la memoria de los recursos gr�ficos.
            //Porque est�n utilizando memoria de la placa de video (y ah� no hay Garbage Collector).
            box.dispose();
        }

    }
}
