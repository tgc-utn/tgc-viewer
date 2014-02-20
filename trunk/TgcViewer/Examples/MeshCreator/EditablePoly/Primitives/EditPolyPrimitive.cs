using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using System.Drawing;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.MeshCreator.EditablePolyTools.Primitives
{
    /// <summary>
    /// Primitiva generica
    /// </summary>
    public abstract class EditPolyPrimitive
    {
        protected bool selected;
        /// <summary>
        /// Indica si la primitiva esta seleccionada
        /// </summary>
        public bool Selected
        {
            get { return selected; }
            set { selected = value; }
        }

        public EditPolyPrimitive()
        {
            selected = false;
        }

        /// <summary>
        /// Tipo de primitiva
        /// </summary>
        public abstract EditablePoly.PrimitiveType Type { get; }

        /// <summary>
        /// Proyectar primitva a rectangulo 2D en la pantalla
        /// </summary>
        /// <param name="transform">Mesh transform</param>
        /// <param name="box2D">Rectangulo 2D proyectado</param>
        /// <returns>False si es un caso degenerado de proyeccion y no debe considerarse</returns>
        public abstract bool projectToScreen(Matrix transform, out Rectangle box2D);

        /// <summary>
        /// Intersect ray againts primitive
        /// </summary>
        public abstract bool intersectRay(TgcRay tgcRay, Matrix transform, out Vector3 q);

        /// <summary>
        /// Centro de la primitiva
        /// </summary>
        public abstract Vector3 computeCenter();

        /// <summary>
        /// Mover primitiva
        /// </summary>
        public abstract void move(Vector3 movement);
    }
}
