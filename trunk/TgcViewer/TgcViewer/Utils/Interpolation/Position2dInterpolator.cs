using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using TgcViewer;

namespace TgcViewer.Utils.Interpolation
{
    /// <summary>
    /// Utilidad para interpolar linealmente entre dos posiciones 2D
    /// </summary>
    public class Position2dInterpolator
    {
        Vector2 current;
        Vector2 dir;
        float distanceToTravel;

        float speed;
        /// <summary>
        /// Velocidad de desplazamiento en segundos
        /// </summary>
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        Vector2 init;
        /// <summary>
        /// Posicion inicial
        /// </summary>
        public Vector2 Init
        {
            get { return init; }
            set { init = value; }
        }

        Vector2 end;
        /// <summary>
        /// Posicion final
        /// </summary>
        public Vector2 End
        {
            get { return end; }
            set { end = value; }
        }

        /// <summary>
        /// Crear interpolador
        /// </summary>
        public Position2dInterpolator()
        {
        }

        /// <summary>
        /// Cargar valores iniciales del interpolador
        /// </summary>
        public void reset()
        {
            dir = end - init;
            distanceToTravel = dir.Length();
            dir.Normalize(); 
            current = init;
        }

        /// <summary>
        /// Actualizar estado del interpolador.
        /// Llamar a reset() la primera vez.
        /// </summary>
        /// <returns>Nueva posicion</returns>
        public Vector2 update()
        {
            float movement = speed * GuiController.Instance.ElapsedTime;
            distanceToTravel -= movement;
            if (distanceToTravel < 0)
            {
                distanceToTravel = 0;
                current = end;
            }
            else
            {
                current += Vector2.Scale(dir, movement);
            }
            return current;
        }

        /// <summary>
        /// Indica si el interpolador llego al final
        /// </summary>
        public bool isEnd()
        {
            return distanceToTravel == 0;
        } 

    }
}
