using TGC.Core.Mathematica;

namespace TGC.Core
{
    public abstract class TGCGeometry
    {
        /// <summary>
        ///     Habilita el renderizado con AlphaBlending para los modelos
        ///     con textura o colores por vértice de canal Alpha.
        ///     Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlend { get; set; }

        /// <summary>
        ///     Posicion absoluta del objeto
        /// </summary>
        public TGCVector3 Position { get; set; }

        public TGCVector3 Up { get; set; }

        public TGCVector3 Right { get; set; }

        /// <summary>
        ///     Rotación absoluta del objeto
        /// </summary>
        public TGCVector3 Rotation { get; set; }

        /// <summary>
        ///     Escalado absoluto del objeto
        /// </summary>
        public TGCVector3 Scale { get; set; }

        /// <summary>
        ///     Matriz final que se utiliza para aplicar transformaciones al objeto.
        ///     Si la propiedad AutoTransformEnable esta en True, la matriz se reconstruye en cada cuadro
        ///     en base a los valores de: Position, Rotation, Scale.
        ///     Si AutoTransformEnable está en False, se respeta el valor que el usuario haya cargado en la matriz.
        /// </summary>
        public TGCMatrix Transform { get; set; }

        public abstract void Update();

        /// <summary>
        ///     Renderiza el objeto
        /// </summary>
        public abstract void Render();

        /// <summary>
        ///     Libera los recursos del objeto
        /// </summary>
        public abstract void Dispose();
    }
}