using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using System.Drawing;
using TgcViewer.Utils.Input;
using Examples.MeshCreator.Primitives;
using TgcViewer;

namespace Examples.MeshCreator.Gizmos
{
    /// <summary>
    /// Gizmo para trasladar objetos
    /// </summary>
    public class TranslateGizmo : EditorGizmo
    {
        const float LARGE_AXIS_SIZE = 200f;
        const float INTERMEDIATE_AXIS_SIZE = 30f;
        const float SHORT_AXIS_SIZE = 5f;

        enum Axis
        {
            X, Y, Z, XY, XZ, YZ, None,
        }

        enum State
        {
            Init,
            Dragging,
        }

        State currentState;
        TgcBox boxX;
        TgcBox boxY;
        TgcBox boxZ;
        TgcBox boxXZ;
        TgcBox boxXY;
        TgcBox boxYZ;
        Axis selectedAxis;
        //Vector2 initMouseP;
        Vector3 initMouseP3d;
        Vector3 gizmoCenter;
        Vector3 acumMovement;

        public TranslateGizmo(MeshCreatorControl control)
            : base(control)
        {
            selectedAxis = Axis.None;
            currentState = State.Init;

            boxX = TgcBox.fromExtremes(new Vector3(0, 0, 0), new Vector3(LARGE_AXIS_SIZE, SHORT_AXIS_SIZE, SHORT_AXIS_SIZE), Color.Red);
            boxY = TgcBox.fromExtremes(new Vector3(0, 0, 0), new Vector3(SHORT_AXIS_SIZE, LARGE_AXIS_SIZE, SHORT_AXIS_SIZE), Color.Green);
            boxZ = TgcBox.fromExtremes(new Vector3(0, 0, 0), new Vector3(SHORT_AXIS_SIZE, SHORT_AXIS_SIZE, LARGE_AXIS_SIZE), Color.Blue);
            boxXZ = TgcBox.fromExtremes(new Vector3(0, 0, 0), new Vector3(INTERMEDIATE_AXIS_SIZE, SHORT_AXIS_SIZE, INTERMEDIATE_AXIS_SIZE), Color.Orange);
            boxXY = TgcBox.fromExtremes(new Vector3(0, 0, 0), new Vector3(INTERMEDIATE_AXIS_SIZE, INTERMEDIATE_AXIS_SIZE, SHORT_AXIS_SIZE), Color.Orange);
            boxYZ = TgcBox.fromExtremes(new Vector3(0, 0, 0), new Vector3(SHORT_AXIS_SIZE, INTERMEDIATE_AXIS_SIZE, INTERMEDIATE_AXIS_SIZE), Color.Orange);
        }

        public override void setEnabled(bool enabled)
        {
            //Activar
            if (enabled)
            {
                Control.CurrentState = MeshCreatorControl.State.GizmoActivated;
                currentState = State.Init;

                //Posicionar gizmo
                TgcBoundingBox aabb = MeshCreatorUtils.getSelectionBoundingBox(Control.SelectionList);
                gizmoCenter = aabb.calculateBoxCenter();
                setAxisPositionAndSize();
            }

        }

        /// <summary>
        /// Configurar posicion y tamaño de ejes segun la distancia a la camara
        /// </summary>
        private void setAxisPositionAndSize()
        {
            float increment = MeshCreatorUtils.getTranslateGizmoSizeIncrement(Control.Camera, gizmoCenter);

            boxX.Size = Vector3.Multiply(new Vector3(LARGE_AXIS_SIZE, SHORT_AXIS_SIZE, SHORT_AXIS_SIZE), increment);
            boxY.Size = Vector3.Multiply(new Vector3(SHORT_AXIS_SIZE, LARGE_AXIS_SIZE, SHORT_AXIS_SIZE), increment);
            boxZ.Size = Vector3.Multiply(new Vector3(SHORT_AXIS_SIZE, SHORT_AXIS_SIZE, LARGE_AXIS_SIZE), increment);
            boxXZ.Size = Vector3.Multiply(new Vector3(INTERMEDIATE_AXIS_SIZE, SHORT_AXIS_SIZE, INTERMEDIATE_AXIS_SIZE), increment);
            boxXY.Size = Vector3.Multiply(new Vector3(INTERMEDIATE_AXIS_SIZE, INTERMEDIATE_AXIS_SIZE, SHORT_AXIS_SIZE), increment);
            boxYZ.Size = Vector3.Multiply(new Vector3(SHORT_AXIS_SIZE, INTERMEDIATE_AXIS_SIZE, INTERMEDIATE_AXIS_SIZE), increment);

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

        public override void update()
        {
            TgcD3dInput input = GuiController.Instance.D3dInput;

            switch (currentState)
            {
                case State.Init:

                    acumMovement = Vector3.Empty;
                    selectedAxis = Axis.None;

                    //Iniciar seleccion de eje
                    if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                    {
                        Control.PickingRay.updateRay();
                        Vector3 collP;

                        //Buscar colision con eje con Picking
                        if (TgcCollisionUtils.intersectRayAABB(Control.PickingRay.Ray, boxX.BoundingBox, out collP))
                        {
                            selectedAxis = Axis.X;
                            initMouseP3d = Control.Grid.getPickingX(Control.PickingRay.Ray, gizmoCenter);
                        }
                        else if (TgcCollisionUtils.intersectRayAABB(Control.PickingRay.Ray, boxY.BoundingBox, out collP))
                        {
                            selectedAxis = Axis.Y;
                            initMouseP3d = Control.Grid.getPickingY(Control.PickingRay.Ray, gizmoCenter);
                        }
                        else if (TgcCollisionUtils.intersectRayAABB(Control.PickingRay.Ray, boxZ.BoundingBox, out collP))
                        {
                            selectedAxis = Axis.Z;
                            initMouseP3d = Control.Grid.getPickingZ(Control.PickingRay.Ray, gizmoCenter);
                        }
                        else if (TgcCollisionUtils.intersectRayAABB(Control.PickingRay.Ray, boxXZ.BoundingBox, out collP))
                        {
                            selectedAxis = Axis.XZ;
                            initMouseP3d = Control.Grid.getPickingXZ(Control.PickingRay.Ray, gizmoCenter);
                        }
                        else if (TgcCollisionUtils.intersectRayAABB(Control.PickingRay.Ray, boxXY.BoundingBox, out collP))
                        {
                            selectedAxis = Axis.XY;
                            initMouseP3d = Control.Grid.getPickingXY(Control.PickingRay.Ray, gizmoCenter);
                        }
                        else if (TgcCollisionUtils.intersectRayAABB(Control.PickingRay.Ray, boxYZ.BoundingBox, out collP))
                        {
                            selectedAxis = Axis.YZ;
                            initMouseP3d = Control.Grid.getPickingYZ(Control.PickingRay.Ray, gizmoCenter);
                        }
                        else
                        {
                            selectedAxis = Axis.None;
                        }

                        //Si eligio un eje, iniciar dragging
                        if (selectedAxis != Axis.None)
                        {
                            currentState = State.Dragging;
                        }
                        else
                        {
                            if (input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                            {
                                bool additive = input.keyDown(Microsoft.DirectX.DirectInput.Key.LeftControl) || input.keyDown(Microsoft.DirectX.DirectInput.Key.RightControl);
                                Control.CurrentState = MeshCreatorControl.State.SelectObject;
                                Control.SelectionRectangle.doDirectSelection(additive);
                            }
                        }
                    }
                    //Hacer mouse over sobre los ejes
                    else
                    {
                        Control.PickingRay.updateRay();
                        Vector3 collP;

                        //Buscar colision con eje con Picking
                        if (TgcCollisionUtils.intersectRayAABB(Control.PickingRay.Ray, boxX.BoundingBox, out collP))
                        {
                            selectedAxis = Axis.X;
                        }
                        else if (TgcCollisionUtils.intersectRayAABB(Control.PickingRay.Ray, boxY.BoundingBox, out collP))
                        {
                            selectedAxis = Axis.Y;
                        }
                        else if (TgcCollisionUtils.intersectRayAABB(Control.PickingRay.Ray, boxZ.BoundingBox, out collP))
                        {
                            selectedAxis = Axis.Z;
                        }
                        else if (TgcCollisionUtils.intersectRayAABB(Control.PickingRay.Ray, boxXZ.BoundingBox, out collP))
                        {
                            selectedAxis = Axis.XZ;
                        }
                        else if (TgcCollisionUtils.intersectRayAABB(Control.PickingRay.Ray, boxXY.BoundingBox, out collP))
                        {
                            selectedAxis = Axis.XY;
                        }
                        else if (TgcCollisionUtils.intersectRayAABB(Control.PickingRay.Ray, boxYZ.BoundingBox, out collP))
                        {
                            selectedAxis = Axis.YZ;
                        }
                        else
                        {
                            selectedAxis = Axis.None;
                        }

                    }

                    break;

                case State.Dragging:

                    //Mover
                    if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                    {
                        Control.PickingRay.updateRay();
                        Vector3 endMouseP3d = initMouseP3d;

                        //Solo se mueve un eje
                        Vector3 currentMove = new Vector3(0, 0, 0);
                        if (isSingleAxis(selectedAxis))
                        {
                            //Desplazamiento en eje X
                            if (selectedAxis == Axis.X)
                            {
                                endMouseP3d = Control.Grid.getPickingX(Control.PickingRay.Ray, gizmoCenter);
                                currentMove.X = endMouseP3d.X - initMouseP3d.X;
                            }
                            //Desplazamiento en eje Y
                            else if (selectedAxis == Axis.Y)
                            {
                                endMouseP3d = Control.Grid.getPickingY(Control.PickingRay.Ray, gizmoCenter);
                                currentMove.Y = endMouseP3d.Y - initMouseP3d.Y;
                            }
                            //Desplazamiento en eje Z
                            else if (selectedAxis == Axis.Z)
                            {
                                endMouseP3d = Control.Grid.getPickingZ(Control.PickingRay.Ray, gizmoCenter);
                                currentMove.Z = endMouseP3d.Z - initMouseP3d.Z;
                            }
                        }
                        //Desplazamiento en dos ejes
                        else
                        {
                            //Plano XZ
                            if (selectedAxis == Axis.XZ)
                            {
                                endMouseP3d = Control.Grid.getPickingXZ(Control.PickingRay.Ray, gizmoCenter);
                                currentMove.X = endMouseP3d.X - initMouseP3d.X;
                                currentMove.Z = endMouseP3d.Z - initMouseP3d.Z;
                            }
                            //Plano XY
                            else if (selectedAxis == Axis.XY)
                            {
                                endMouseP3d = Control.Grid.getPickingXY(Control.PickingRay.Ray, gizmoCenter);
                                currentMove.X = endMouseP3d.X - initMouseP3d.X;
                                currentMove.Y = endMouseP3d.Y - initMouseP3d.Y;
                            }
                            //Plano YZ
                            else if (selectedAxis == Axis.YZ)
                            {
                                endMouseP3d = Control.Grid.getPickingYZ(Control.PickingRay.Ray, gizmoCenter);
                                currentMove.Y = endMouseP3d.Y - initMouseP3d.Y;
                                currentMove.Z = endMouseP3d.Z - initMouseP3d.Z;
                            }
                        }

                        //Actualizar pos del mouse 3D para proximo cuadro
                        initMouseP3d = endMouseP3d;

                        //Ajustar currentMove con Snap to grid
                        if (Control.SnapToGridEnabled)
                        {
                            snapMovementToGrid(ref currentMove.X, ref acumMovement.X, Control.SnapToGridCellSize);
                            snapMovementToGrid(ref currentMove.Y, ref acumMovement.Y, Control.SnapToGridCellSize);
                            snapMovementToGrid(ref currentMove.Z, ref acumMovement.Z, Control.SnapToGridCellSize);
                        }                      

                        //Mover objetos
                        foreach (EditorPrimitive p in Control.SelectionList)
	                    {
                    		 p.move(currentMove);
	                    }
                        
                        //Mover ejes
                        moveGizmo(currentMove);

                        //Actualizar datos de modify
                        Control.updateModifyPanel();
                    }

                    //Soltar movimiento
                    else if(input.buttonUp(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                    {
                        currentState = State.Init;
                        selectedAxis = Axis.None;
                    }


                    break;
            }

            

            //Ajustar tamaño de ejes
            setAxisPositionAndSize();
        }

        

        public override void render()
        {
            //Desactivar Z-Buffer para dibujar arriba de todo el escenario
            GuiController.Instance.D3dDevice.RenderState.ZBufferEnable = false;

            //Dibujar
            boxXZ.render();
            boxXY.render();
            boxYZ.render();
            boxX.render();
            boxY.render();
            boxZ.render();
            

            TgcBox selectedBox = getAxisBox(selectedAxis);
            if (selectedBox != null)
            {
                selectedBox.BoundingBox.render();
            }

            GuiController.Instance.D3dDevice.RenderState.ZBufferEnable = true;
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

        private bool isSingleAxis(Axis axis)
        {
            return axis == Axis.X || axis == Axis.Y || axis == Axis.Z;
        }

        /// <summary>
        /// Mover ejes del gizmo
        /// </summary>
        private void moveGizmo(Vector3 movement)
        {
            gizmoCenter += movement;
            boxX.move(movement);
            boxY.move(movement);
            boxZ.move(movement);
        }

        /// <summary>
        /// Mover gizmo
        /// </summary>
        public override void move(EditorPrimitive selectedPrimitive, Vector3 movement)
        {
            moveGizmo(movement);
        }


        /// <summary>
        /// Calcular desplazamiento en unidades de grilla
        /// </summary>
        private float snapMovementToGrid(ref float currentMove, ref float acumMove, float cellSize)
        {
            //Se movio algo?
            float totalMove = acumMove + currentMove;
            const float epsilon = 0.1f;
            float absMove = FastMath.Abs(totalMove);
            float snapMove = 0;
            if (absMove > epsilon)
            {
                if (absMove > cellSize)
                {
                    //Moverse en unidades de la grilla
                    currentMove = ((int)(totalMove / cellSize)) * cellSize;
                    acumMove = 0;
                }
                else
                {
                    //Acumular movimiento para la proxima
                    acumMove += currentMove;
                    currentMove = 0;
                }
            }

            return snapMove;
        }

    }
}
