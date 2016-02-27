using Examples.MeshCreator.Primitives;
using Microsoft.DirectX;

namespace Examples.MeshCreator.Gizmos
{
    /// <summary>
    ///     Gizmo generico
    /// </summary>
    public abstract class EditorGizmo
    {
        public EditorGizmo(MeshCreatorControl control)
        {
            Control = control;
        }

        /// <summary>
        ///     Control
        /// </summary>
        public MeshCreatorControl Control { get; }

        /// <summary>
        ///     Activar o desactivar gizmo
        /// </summary>
        public abstract void setEnabled(bool enabled);

        /// <summary>
        ///     Actualizar estado
        /// </summary>
        public abstract void update();

        /// <summary>
        ///     Dibujar gizmo, sin Z Buffer
        /// </summary>
        public abstract void render();

        /// <summary>
        ///     Mover gizmo de lugar para acomodarse a la nueva posicion del objeto seleccionado
        /// </summary>
        public abstract void move(EditorPrimitive selectedPrimitive, Vector3 movement);

        /// <summary>
        ///     Liberar recursos
        /// </summary>
        public abstract void dipose();
    }
}