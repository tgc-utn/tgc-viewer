using Microsoft.DirectX;
using TGC.Core.Direct3D;
using TGC.Core.Input;

namespace TGC.Core.Geometry
{
    /// <summary>
    ///     Utilidad para crear
    /// </summary>
    public class TgcPickingRay
    {
        private TgcD3dInput Input { get; set; }

        public TgcPickingRay(TgcD3dInput input)
        {
            Input = input;
            Ray = new TgcRay();
        }

        /// <summary>
        ///     Ray que representa la accion de Picking
        /// </summary>
        public TgcRay Ray { get; }

        /// <summary>
        ///     Actualiza el Ray de colision en base a la posicion del mouse en la pantalla
        /// </summary>
        public void updateRay()
        {
            //Crear Ray en base a coordenadas del mouse
            var sx = Input.Xpos;
            var sy = Input.Ypos;
            var w = D3DDevice.Instance.Device.Viewport.Width;
            var h = D3DDevice.Instance.Device.Viewport.Height;
            var matProj = D3DDevice.Instance.Device.Transform.Projection;

            var v = new Vector3();
            v.X = (2.0f * sx / w - 1) / matProj.M11;
            v.Y = -(2.0f * sy / h - 1) / matProj.M22;
            v.Z = 1.0f;

            //Transform the screen space pick ray into 3D space
            var m = Matrix.Invert(D3DDevice.Instance.Device.Transform.View);
            var rayDir = new Vector3(
                v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31,
                v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32,
                v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33
                );
            var rayOrig = new Vector3(m.M41, m.M42, m.M43);

            //Picking Ray creado
            Ray.Origin = rayOrig;
            Ray.Direction = rayDir;
        }
    }
}