using System;
using TGC.Core.Mathematica;

namespace TGC.Core.SceneLoader
{
    /// <summary>
    ///     Interfaz genérica para permitir que se le apliquen transformaciones a un objeto
    /// </summary>
    public interface ITransformObject
    {
        /// <summary>
        ///     Matriz final que se utiliza para aplicar transformaciones al objeto.
        ///     Si la propiedad AutoTransformEnable esta en True, la matriz se reconstruye en cada cuadro
        ///     en base a los valores de: Position, Rotation, Scale.
        ///     Si AutoTransformEnable está en False, se respeta el valor que el usuario haya cargado en la matriz.
        /// </summary>
        TGCMatrix Transform { get; set; }

        /// <summary>
        ///     En True hace que la matriz de transformacion (Transform) del objeto se actualiza en
        ///     cada cuadro en forma automática, según los valores de: Position, Rotation, Scale.
        ///     En False se respeta lo que el usuario haya cargado a mano en la matriz.
        ///     Por default está en True.
        /// </summary>
        [Obsolete("Utilizar esta propiedad en juegos complejos se pierde el control, es mejor utilizar transformaciones con matrices.")]
        bool AutoTransformEnable { get; set; }

        /// <summary>
        ///     Posicion absoluta del objeto
        /// </summary>
        TGCVector3 Position { get; set; }

        /// <summary>
        ///     Rotación absoluta del objeto
        /// </summary>
        TGCVector3 Rotation { get; set; }

        /// <summary>
        ///     Escalado absoluto del objeto
        /// </summary>
        TGCVector3 Scale { get; set; }

        /// <summary>
        ///     Desplaza la malla la distancia especificada, respecto de su posicion actual
        /// </summary>
        [Obsolete]
        void Move(TGCVector3 v);

        /// <summary>
        ///     Desplaza la malla la distancia especificada, respecto de su posicion actual
        /// </summary>
        [Obsolete]
        void Move(float x, float y, float z);

        /// <summary>
        ///     Mueve la malla en base a la orientacion actual de rotacion.
        ///     Es necesario rotar la malla primero
        /// </summary>
        /// <param name="movement">Desplazamiento. Puede ser positivo (hacia adelante) o negativo (hacia atras)</param>
        [Obsolete]
        void MoveOrientedY(float movement);

        /// <summary>
        ///     Obtiene la posicion absoluta de la malla, recibiendo un vector ya creado para
        ///     almacenar el resultado
        /// </summary>
        /// <param name="pos">Vector ya creado en el que se carga el resultado</param>
        void GetPosition(TGCVector3 pos);

        /// <summary>
        ///     Rota la malla respecto del eje X
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        [Obsolete]
        void RotateX(float angle);

        /// <summary>
        ///     Rota la malla respecto del eje Y
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        [Obsolete]
        void RotateY(float angle);

        /// <summary>
        ///     Rota la malla respecto del eje Z
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        [Obsolete]
        void RotateZ(float angle);
    }
}