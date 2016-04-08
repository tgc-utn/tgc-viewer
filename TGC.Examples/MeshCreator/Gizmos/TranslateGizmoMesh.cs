using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Geometries;

namespace TGC.Examples.MeshCreator.Gizmos
{
    /// <summary>
    ///     Mesh para renderizar un gizmo de Translate
    /// </summary>
    public class TranslateGizmoMesh
    {
        public enum Axis
        {
            X,
            Y,
            Z,
            XY,
            XZ,
            YZ,
            None
        }

        private const float LARGE_AXIS_SIZE = 200f;
        private const float INTERMEDIATE_AXIS_SIZE = 30f;
        private const float SHORT_AXIS_SIZE = 5f;

        private readonly TgcBox boxX;
        private readonly TgcBox boxXY;
        private readonly TgcBox boxXZ;
        private readonly TgcBox boxY;
        private readonly TgcBox boxYZ;
        private readonly TgcBox boxZ;

        public TranslateGizmoMesh()
        {
            SelectedAxis = Axis.None;

            boxX = TgcBox.fromExtremes(new Vector3(0, 0, 0),
                new Vector3(LARGE_AXIS_SIZE, SHORT_AXIS_SIZE, SHORT_AXIS_SIZE), Color.Red);
            boxY = TgcBox.fromExtremes(new Vector3(0, 0, 0),
                new Vector3(SHORT_AXIS_SIZE, LARGE_AXIS_SIZE, SHORT_AXIS_SIZE), Color.Green);
            boxZ = TgcBox.fromExtremes(new Vector3(0, 0, 0),
                new Vector3(SHORT_AXIS_SIZE, SHORT_AXIS_SIZE, LARGE_AXIS_SIZE), Color.Blue);
            boxXZ = TgcBox.fromExtremes(new Vector3(0, 0, 0),
                new Vector3(INTERMEDIATE_AXIS_SIZE, SHORT_AXIS_SIZE, INTERMEDIATE_AXIS_SIZE), Color.Orange);
            boxXY = TgcBox.fromExtremes(new Vector3(0, 0, 0),
                new Vector3(INTERMEDIATE_AXIS_SIZE, INTERMEDIATE_AXIS_SIZE, SHORT_AXIS_SIZE), Color.Orange);
            boxYZ = TgcBox.fromExtremes(new Vector3(0, 0, 0),
                new Vector3(SHORT_AXIS_SIZE, INTERMEDIATE_AXIS_SIZE, INTERMEDIATE_AXIS_SIZE), Color.Orange);
        }

        /// <summary>
        ///     Eje seleccionado
        /// </summary>
        public Axis SelectedAxis { get; private set; }

        /// <summary>
        ///     Centro del gizmo
        /// </summary>
        public Vector3 GizmoCenter { get; private set; }

        /// <summary>
        ///     Setear centro del gizmo y ajustar tamaño segun distancia con la camara
        /// </summary>
        public void setCenter(Vector3 gizmoCenter, MeshCreatorCamera camera)
        {
            GizmoCenter = gizmoCenter;
            var increment = MeshCreatorUtils.getTranslateGizmoSizeIncrement(camera, gizmoCenter);

            boxX.Size = Vector3.Multiply(new Vector3(LARGE_AXIS_SIZE, SHORT_AXIS_SIZE, SHORT_AXIS_SIZE), increment);
            boxY.Size = Vector3.Multiply(new Vector3(SHORT_AXIS_SIZE, LARGE_AXIS_SIZE, SHORT_AXIS_SIZE), increment);
            boxZ.Size = Vector3.Multiply(new Vector3(SHORT_AXIS_SIZE, SHORT_AXIS_SIZE, LARGE_AXIS_SIZE), increment);
            boxXZ.Size = Vector3.Multiply(new Vector3(INTERMEDIATE_AXIS_SIZE, SHORT_AXIS_SIZE, INTERMEDIATE_AXIS_SIZE),
                increment);
            boxXY.Size = Vector3.Multiply(new Vector3(INTERMEDIATE_AXIS_SIZE, INTERMEDIATE_AXIS_SIZE, SHORT_AXIS_SIZE),
                increment);
            boxYZ.Size = Vector3.Multiply(new Vector3(SHORT_AXIS_SIZE, INTERMEDIATE_AXIS_SIZE, INTERMEDIATE_AXIS_SIZE),
                increment);

            boxX.Position = gizmoCenter + Vector3.Multiply(boxX.Size, 0.5f) + new Vector3(SHORT_AXIS_SIZE, 0, 0);
            boxY.Position = gizmoCenter + Vector3.Multiply(boxY.Size, 0.5f) + new Vector3(0, SHORT_AXIS_SIZE, 0);
            boxZ.Position = gizmoCenter + Vector3.Multiply(boxZ.Size, 0.5f) + new Vector3(0, 0, SHORT_AXIS_SIZE);
            boxXZ.Position = gizmoCenter + new Vector3(boxXZ.Size.X / 2, 0, boxXZ.Size.Z / 2);
            boxXY.Position = gizmoCenter + new Vector3(boxXY.Size.X / 2, boxXY.Size.Y / 2, 0);
            boxYZ.Position = gizmoCenter + new Vector3(0, boxYZ.Size.Y / 2, boxYZ.Size.Z / 2);

            boxX.updateValues();
            boxY.updateValues();
            boxZ.updateValues();
            boxXZ.updateValues();
            boxXY.updateValues();
            boxYZ.updateValues();
        }

        /// <summary>
        ///     Hacer picking contra todos los ejes y devolver el eje seleccionado (si hay colision).
        ///     Tambien se evaluan los ejes compuestos (XY, XZ, YZ)
        /// </summary>
        public Axis doPickAxis(TgcRay ray)
        {
            Vector3 collP;
            if (TgcCollisionUtils.intersectRayAABB(ray, boxX.BoundingBox, out collP))
            {
                return Axis.X;
            }
            if (TgcCollisionUtils.intersectRayAABB(ray, boxY.BoundingBox, out collP))
            {
                return Axis.Y;
            }
            if (TgcCollisionUtils.intersectRayAABB(ray, boxZ.BoundingBox, out collP))
            {
                return Axis.Z;
            }
            if (TgcCollisionUtils.intersectRayAABB(ray, boxXZ.BoundingBox, out collP))
            {
                return Axis.XZ;
            }
            if (TgcCollisionUtils.intersectRayAABB(ray, boxXY.BoundingBox, out collP))
            {
                return Axis.XY;
            }
            if (TgcCollisionUtils.intersectRayAABB(ray, boxYZ.BoundingBox, out collP))
            {
                return Axis.YZ;
            }
            return Axis.None;
        }

        /// <summary>
        ///     Selecciona el eje actual del gizmo haciendo picking
        /// </summary>
        public void selectAxisByPicking(TgcRay ray)
        {
            SelectedAxis = doPickAxis(ray);
        }

        /// <summary>
        ///     Set Axis.None as selected
        /// </summary>
        public void unSelect()
        {
            SelectedAxis = Axis.None;
        }

        /// <summary>
        ///     Indica si el eje especificado es simple (X, Y, Z) o compusto (XY, XZ, YZ)
        /// </summary>
        public bool isSingleAxis(Axis axis)
        {
            return axis == Axis.X || axis == Axis.Y || axis == Axis.Z;
        }

        /// <summary>
        ///     Mover ejes del gizmo
        /// </summary>
        public void moveGizmo(Vector3 movement)
        {
            GizmoCenter += movement;
            boxX.move(movement);
            boxY.move(movement);
            boxZ.move(movement);
            boxXZ.move(movement);
            boxXY.move(movement);
            boxYZ.move(movement);
        }

        /// <summary>
        ///     Dibujar gizmo
        /// </summary>
        public void render()
        {
            //Desactivar Z-Buffer para dibujar arriba de todo el escenario
            D3DDevice.Instance.Device.RenderState.ZBufferEnable = false;

            //Dibujar
            boxXZ.render();
            boxXY.render();
            boxYZ.render();
            boxX.render();
            boxY.render();
            boxZ.render();

            var selectedBox = getAxisBox(SelectedAxis);
            if (selectedBox != null)
            {
                selectedBox.BoundingBox.render();
            }

            D3DDevice.Instance.Device.RenderState.ZBufferEnable = true;
        }

        private TgcBox getAxisBox(Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return boxX;

                case Axis.Y:
                    return boxY;

                case Axis.Z:
                    return boxZ;

                case Axis.XZ:
                    return boxXZ;

                case Axis.XY:
                    return boxXY;

                case Axis.YZ:
                    return boxYZ;
            }
            return null;
        }

        public void dispose()
        {
            boxX.dispose();
            boxY.dispose();
            boxZ.dispose();
            boxXY.dispose();
            boxXZ.dispose();
            boxYZ.dispose();
        }
    }
}