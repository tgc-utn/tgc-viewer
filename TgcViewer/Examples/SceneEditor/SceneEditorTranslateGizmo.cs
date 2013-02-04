using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using System.Drawing;
using TgcViewer.Utils.Input;
using TgcViewer;

namespace Examples.SceneEditor
{
    class SceneEditorTranslateGizmo
    {
        const float LARGE_AXIS_FACTOR_SIZE = 2f;
        const float LARGE_AXIS_MIN_SIZE = 50f;
        const float SHORT_AXIS_SIZE = 2f;
        const float MOVE_FACTOR = 4f;

        public enum Axis
        {
            X, Y, Z, None,
        }

        TgcBox boxX;
        TgcBox boxY;
        TgcBox boxZ;
        Vector2 initMouseP;
        TgcBox selectedAxisBox;

        SceneEditorMeshObject meshObj;
        /// <summary>
        /// Objeto sobre el cual se aplica el movimiento
        /// </summary>
        public SceneEditorMeshObject MeshObj
        {
            get { return meshObj; }
        }

        Axis selectedAxis;
        /// <summary>
        /// Eje seleccionado
        /// </summary>
        public Axis SelectedAxis
        {
            get { return selectedAxis; }
        }

        public SceneEditorTranslateGizmo()
        {
            boxX = TgcBox.fromExtremes(new Vector3(0,0,0), new Vector3(LARGE_AXIS_FACTOR_SIZE, SHORT_AXIS_SIZE, SHORT_AXIS_SIZE), Color.Red);
            boxY = TgcBox.fromExtremes(new Vector3(0, 0, 0), new Vector3(SHORT_AXIS_SIZE, LARGE_AXIS_FACTOR_SIZE, SHORT_AXIS_SIZE), Color.Green);
            boxZ = TgcBox.fromExtremes(new Vector3(0, 0, 0), new Vector3(SHORT_AXIS_SIZE, SHORT_AXIS_SIZE, LARGE_AXIS_FACTOR_SIZE), Color.Blue);
        }

        /// <summary>
        /// Acomodar Gizmo en base a un Mesh
        /// </summary>
        public void setMesh(SceneEditorMeshObject meshObj)
        {
            this.meshObj = meshObj;
            this.selectedAxis = Axis.None;

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

            
            Vector3 meshCenter = meshObj.mesh.BoundingBox.calculateBoxCenter();
            Vector3 axisRadius = meshObj.mesh.BoundingBox.calculateAxisRadius();

            float largeX = axisRadius.X + LARGE_AXIS_MIN_SIZE;
            float largeY = axisRadius.Y + LARGE_AXIS_MIN_SIZE;
            float largeZ = axisRadius.Z + LARGE_AXIS_MIN_SIZE;

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
        /// Detectar el eje seleccionado
        /// </summary>
        public void detectSelectedAxis(TgcPickingRay pickingRay)
        {
            pickingRay.updateRay();
            Vector3 collP;

            //Buscar colision con eje con Picking
            if (TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, boxX.BoundingBox, out collP))
            {
                selectedAxis = Axis.X;
                selectedAxisBox = boxX;
            }
            else if (TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, boxY.BoundingBox, out collP))
            {
                selectedAxis = Axis.Y;
                selectedAxisBox = boxY;
            }
            else if (TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, boxZ.BoundingBox, out collP))
            {
                selectedAxis = Axis.Z;
                selectedAxisBox = boxZ;
            }
            else
            {
                selectedAxis = Axis.None;
                selectedAxisBox = null;
            }

            //Desplazamiento inicial
            if (selectedAxis != Axis.None)
            {
                TgcD3dInput input = GuiController.Instance.D3dInput;
                initMouseP = new Vector2(input.XposRelative, input.YposRelative);
            }
        }

        /// <summary>
        /// Actualizar posición de la malla en base a movimientos del mouse
        /// </summary>
        public void updateMove()
        {
            TgcD3dInput input = GuiController.Instance.D3dInput;
            Vector3 currentMove = new Vector3(0, 0, 0);

            //Desplazamiento segun el mouse en X
            if (selectedAxis == Axis.X)
            {
                currentMove.X += (input.XposRelative - initMouseP.X) * MOVE_FACTOR;
            }
            //Desplazamiento segun el mouse en Y
            else if (selectedAxis == Axis.Y)
            {
                currentMove.Y -= (input.YposRelative - initMouseP.Y) * MOVE_FACTOR;
            }
            //Desplazamiento segun el mouse en X
            else if (selectedAxis == Axis.Z)
            {
                currentMove.Z -= (input.YposRelative - initMouseP.Y) * MOVE_FACTOR;
            }

            //Mover mesh
            meshObj.mesh.move(currentMove);

            //Mover ejes
            boxX.move(currentMove);
            boxY.move(currentMove);
            boxZ.move(currentMove);
        }

        /// <summary>
        /// Termina el arrastre sobre un eje
        /// </summary>
        public void endDragging()
        {
            this.selectedAxis = Axis.None;
            this.selectedAxisBox = null;
        }

        /// <summary>
        /// Esconder Gizmo
        /// </summary>
        public void hide()
        {
            meshObj = null;
        }

        public void render()
        {
            if (meshObj == null)
                return;

            boxX.render();
            boxY.render();
            boxZ.render();

            if (selectedAxis != Axis.None)
            {
                selectedAxisBox.BoundingBox.render();
            }
        }
    }
}
