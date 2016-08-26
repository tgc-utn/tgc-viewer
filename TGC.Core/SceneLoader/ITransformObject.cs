using SharpDX;

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
        Matrix Transform { get; set; }

        /// <summary>
        ///     En True hace que la matriz de transformacion (Transform) del objeto se actualiza en
        ///     cada cuadro en forma automática, según los valores de: Position, Rotation, Scale.
        ///     En False se respeta lo que el usuario haya cargado a mano en la matriz.
        ///     Por default está en True.
        /// </summary>
        bool AutoTransformEnable { get; set; }

        /// <summary>
        ///     Posicion absoluta del objeto
        /// </summary>
        Vector3 Position { get; set; }

        /// <summary>
        ///     Rotación absoluta del objeto
        /// </summary>
        Vector3 Rotation { get; set; }

        /// <summary>
        ///     Escalado absoluto del objeto
        /// </summary>
        Vector3 Scale { get; set; }

        /// <summary>
        ///     Desplaza la malla la distancia especificada, respecto de su posicion actual
        /// </summary>
        void move(Vector3 v);

        /// <summary>
        ///     Desplaza la malla la distancia especificada, respecto de su posicion actual
        /// </summary>
        void move(float x, float y, float z);

        /// <summary>
        ///     Mueve la malla en base a la orientacion actual de rotacion.
        ///     Es necesario rotar la malla primero
        /// </summary>
        /// <param name="movement">Desplazamiento. Puede ser positivo (hacia adelante) o negativo (hacia atras)</param>
        void moveOrientedY(float movement);

        /// <summary>
        ///     Obtiene la posicion absoluta de la malla, recibiendo un vector ya creado para
        ///     almacenar el resultado
        /// </summary>
        /// <param name="pos">Vector ya creado en el que se carga el resultado</param>
        void getPosition(Vector3 pos);

        /// <summary>
        ///     Rota la malla respecto del eje X
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        void rotateX(float angle);

        /// <summary>
        ///     Rota la malla respecto del eje Y
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        void rotateY(float angle);

        /// <summary>
        ///     Rota la malla respecto del eje Z
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        void rotateZ(float angle);
    }
}