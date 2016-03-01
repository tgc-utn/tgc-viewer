using Microsoft.DirectX;
using System.Drawing;
using TGC.Viewer;
using TGC.Viewer.Utils.TgcGeometry;

namespace TGC.Examples.SceneEditor
{
    internal class SceneEditorTranslateGizmo
    {
        public enum Axis
        {
            X,
            Y,
            Z,
            None
        }

        private const float LARGE_AXIS_FACTOR_SIZE = 2f;
        private const float LARGE_AXIS_MIN_SIZE = 50f;
        private const float SHORT_AXIS_SIZE = 2f;
        private const float MOVE_FACTOR = 4f;

        private readonly TgcBox boxX;
        private readonly TgcBox boxY;
        private readonly TgcBox boxZ;
        private Vector2 initMouseP;

        private TgcBox selectedAxisBox;

        public SceneEditorTranslateGizmo()
        {
            boxX = TgcBox.fromExtremes(new Vector3(0, 0, 0),
                new Vector3(LARGE_AXIS_FACTOR_SIZE, SHORT_AXIS_SIZE, SHORT_AXIS_SIZE), Color.Red);
            boxY = TgcBox.fromExtremes(new Vector3(0, 0, 0),
                new Vector3(SHORT_AXIS_SIZE, LARGE_AXIS_FACTOR_SIZE, SHORT_AXIS_SIZE), Color.Green);
            boxZ = TgcBox.fromExtremes(new Vector3(0, 0, 0),
                new Vector3(SHORT_AXIS_SIZE, SHORT_AXIS_SIZE, LARGE_AXIS_FACTOR_SIZE), Color.Blue);
        }

        /// <summary>
        ///     Objeto sobre el cual se aplica el movimiento
        /// </summary>
        public SceneEditorMeshObject MeshObj { get; private set; }

        /// <summary>
        ///     Eje seleccionado
        /// </summary>
        public Axis SelectedAxis { get; private set; }

        /// <summary>
        ///     Acomodar Gizmo en base a un Mesh
        /// </summary>
        public void setMesh(SceneEditorMeshObject meshObj)
        {
            MeshObj = meshObj;
            SelectedAxis = Axis.None;

            /*
            float aabbbR = meshObj.mesh.BoundingBox.calculateBoxRadius();
            float largeSize = LARGE_AXIS_SIZE * aabbbR;
            float shortSize = SHORT_AXIS_SIZE;

            boxX.Size = new Vector3(largeSize, shortSize, shortSize);
            boxY.Size = new Vector3(shortSize, largeSize, shortSize);
            boxZ.Size = new Vector3(shortSize, shortSize, largeSize);

            Vector3 pos = meshObj.mesh.Position;
            boxX.Position = pos + Vector3.Scale(boxX.Size, 0.5f);
            boxY.Position = pos + Vector3.Scale(boxY.Size, 0.5f);
            boxZ.Position = pos + Vector3.Scale(boxZ.Size, 0.5f);
            */

            var meshCenter = meshObj.mesh.BoundingBox.calculateBoxCenter();
            var axisRadius = meshObj.mesh.BoundingBox.calculateAxisRadius();

            var largeX = axisRadius.X + LARGE_AXIS_MIN_SIZE;
            var largeY = axisRadius.Y + LARGE_AXIS_MIN_SIZE;
            var largeZ = axisRadius.Z + LARGE_AXIS_MIN_SIZE;

            boxX.Size = new Vector3(largeX, SHORT_AXIS_SIZE, SHORT_AXIS_SIZE);
            boxY.Size = new Vector3(SHORT_AXIS_SIZE, largeY, SHORT_AXIS_SIZE);
            boxZ.Size = new Vector3(SHORT_AXIS_SIZE, SHORT_AXIS_SIZE, largeZ);

            boxX.Position = meshCenter + Vector3.Multiply(boxX.Size, 0.5f);
            boxY.Position = meshCenter + Vector3.Multiply(boxY.Size, 0.5f);
            boxZ.Position = meshCenter + Vector3.Multiply(boxZ.Size, 0.5f);

            boxX.updateValues();
            boxY.updateValues();
            boxZ.updateValues();
        }

        /// <summary>
        ///     Detectar el eje seleccionado
        /// </summary>
        public void detectSelectedAxis(TgcPickingRay pickingRay)
        {
            pickingRay.updateRay();
            Vector3 collP;

            //Buscar colision con eje con Picking
            if (TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, boxX.BoundingBox, out collP))
            {
                SelectedAxis = Axis.X;
                selectedAxisBox = boxX;
            }
            else if (TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, boxY.BoundingBox, out collP))
            {
                SelectedAxis = Axis.Y;
                selectedAxisBox = boxY;
            }
            else if (TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, boxZ.BoundingBox, out collP))
            {
                SelectedAxis = Axis.Z;
                selectedAxisBox = boxZ;
            }
            else
            {
                SelectedAxis = Axis.None;
                selectedAxisBox = null;
            }

            //Desplazamiento inicial
            if (SelectedAxis != Axis.None)
            {
                var input = GuiController.Instance.D3dInput;
                initMouseP = new Vector2(input.XposRelative, input.YposRelative);
            }
        }

        /// <summary>
        ///     Actualizar posición de la malla en base a movimientos del mouse
        /// </summary>
        public void updateMove()
        {
            var input = GuiController.Instance.D3dInput;
            var currentMove = new Vector3(0, 0, 0);

            //Desplazamiento segun el mouse en X
            if (SelectedAxis == Axis.X)
            {
                currentMove.X += (input.XposRelative - initMouseP.X) * MOVE_FACTOR;
            }
            //Desplazamiento segun el mouse en Y
            else if (SelectedAxis == Axis.Y)
            {
                currentMove.Y -= (input.YposRelative - initMouseP.Y) * MOVE_FACTOR;
            }
            //Desplazamiento segun el mouse en X
            else if (SelectedAxis == Axis.Z)
            {
                currentMove.Z -= (input.YposRelative - initMouseP.Y) * MOVE_FACTOR;
            }

            //Mover mesh
            MeshObj.mesh.move(currentMove);

            //Mover ejes
            boxX.move(currentMove);
            boxY.move(currentMove);
            boxZ.move(currentMove);
        }

        /// <summary>
        ///     Termina el arrastre sobre un eje
        /// </summary>
        public void endDragging()
        {
            SelectedAxis = Axis.None;
            selectedAxisBox = null;
        }

        /// <summary>
        ///     Esconder Gizmo
        /// </summary>
        public void hide()
        {
            MeshObj = null;
        }

        public void render()
        {
            if (MeshObj == null)
                return;

            boxX.render();
            boxY.render();
            boxZ.render();

            if (SelectedAxis != Axis.None)
            {
                selectedAxisBox.BoundingBox.render();
            }
        }
    }
}