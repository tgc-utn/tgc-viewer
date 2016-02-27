using Microsoft.DirectX;

namespace TgcViewer.Utils.TgcGeometry
{
    /// <summary>
    ///     Utilidad para crear
    /// </summary>
    public class TgcPickingRay
    {
        public TgcPickingRay()
        {
            Ray = new TgcRay();
        }

        /// <summary>
        ///     Ray que representa la acción de Picking
        /// </summary>
        public TgcRay Ray { get; }

        /// <summary>
        ///     Actualiza el Ray de colisión en base a la posición del mouse en la pantalla
        /// </summary>
        public void updateRay()
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            //Crear Ray en base a coordenadas del mouse
            var sx = GuiController.Instance.D3dInput.Xpos;
            var sy = GuiController.Instance.D3dInput.Ypos;
            var w = d3dDevice.Viewport.Width;
            var h = d3dDevice.Viewport.Height;
            var matProj = d3dDevice.Transform.Projection;

            var v = new Vector3();
            v.X = (2.0f*sx/w - 1)/matProj.M11;
            v.Y = -(2.0f*sy/h - 1)/matProj.M22;
            v.Z = 1.0f;

            //Transform the screen space pick ray into 3D space
            var m = Matrix.Invert(d3dDevice.Transform.View);
            var rayDir = new Vector3(
                v.X*m.M11 + v.Y*m.M21 + v.Z*m.M31,
                v.X*m.M12 + v.Y*m.M22 + v.Z*m.M32,
                v.X*m.M13 + v.Y*m.M23 + v.Z*m.M33
                );
            var rayOrig = new Vector3(m.M41, m.M42, m.M43);

            //Picking Ray creado
            Ray.Origin = rayOrig;
            Ray.Direction = rayDir;
        }
    }
}