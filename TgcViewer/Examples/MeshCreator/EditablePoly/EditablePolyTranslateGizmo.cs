using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Examples.MeshCreator.Gizmos;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer;
using TgcViewer.Utils.Input;
using Examples.MeshCreator.EditablePolyTools.Primitives;

namespace Examples.MeshCreator.EditablePolyTools
{
    /// <summary>
    /// Gizmo de Translate para Editable Poly
    /// </summary>
    public class EditablePolyTranslateGizmo
    {
        enum State
        {
            Init,
            Dragging,
        }

        EditablePoly editablePoly;
        State currentState;
        Vector3 initMouseP3d;
        Vector3 acumMovement;
        TranslateGizmoMesh gizmoMesh;

        public EditablePolyTranslateGizmo(EditablePoly editablePoly)
        {
            this.editablePoly = editablePoly;
            gizmoMesh = new TranslateGizmoMesh();
            currentState = State.Init;
        }

        /// <summary>
        /// Activar
        /// </summary>
        public void setEnabled(bool enabled)
        {
            //Activar
            if (enabled)
            {
                editablePoly.CurrentState = EditablePoly.State.TranslateGizmo;
                currentState = State.Init;

                //Posicionar gizmo
                TgcBoundingBox aabb = EditablePolyUtils.getSelectionBoundingBox(editablePoly.SelectionList);
                Vector3 aabbCenter = Vector3.TransformCoordinate(aabb.calculateBoxCenter(), editablePoly.Transform);
                gizmoMesh.setCenter(aabbCenter, editablePoly.Control.Camera);
            }
        }

        /// <summary>
        /// Actualizar eventos
        /// </summary>
        public void update()
        {
            TgcD3dInput input = GuiController.Instance.D3dInput;
            TgcPickingRay pickingRay = editablePoly.Control.PickingRay;

            switch (currentState)
            {
                case State.Init:

                    acumMovement = Vector3.Empty;
                    gizmoMesh.unSelect();

                    //Iniciar seleccion de eje
                    if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                    {
                        //Buscar colision con ejes del gizmo
                        pickingRay.updateRay();
                        gizmoMesh.selectAxisByPicking(pickingRay.Ray);
                        if (gizmoMesh.SelectedAxis == TranslateGizmoMesh.Axis.X)
                        {
                            initMouseP3d = editablePoly.Control.Grid.getPickingX(pickingRay.Ray, gizmoMesh.GizmoCenter);
                        }
                        else if (gizmoMesh.SelectedAxis == TranslateGizmoMesh.Axis.Y)
                        {
                            initMouseP3d = editablePoly.Control.Grid.getPickingY(pickingRay.Ray, gizmoMesh.GizmoCenter);
                        }
                        else if (gizmoMesh.SelectedAxis == TranslateGizmoMesh.Axis.Z)
                        {
                            initMouseP3d = editablePoly.Control.Grid.getPickingZ(pickingRay.Ray, gizmoMesh.GizmoCenter);
                        }
                        else if (gizmoMesh.SelectedAxis == TranslateGizmoMesh.Axis.XZ)
                        {
                            initMouseP3d = editablePoly.Control.Grid.getPickingXZ(pickingRay.Ray, gizmoMesh.GizmoCenter);
                        }
                        else if (gizmoMesh.SelectedAxis == TranslateGizmoMesh.Axis.XY)
                        {
                            initMouseP3d = editablePoly.Control.Grid.getPickingXY(pickingRay.Ray, gizmoMesh.GizmoCenter);
                        }
                        else if (gizmoMesh.SelectedAxis == TranslateGizmoMesh.Axis.YZ)
                        {
                            initMouseP3d = editablePoly.Control.Grid.getPickingYZ(pickingRay.Ray, gizmoMesh.GizmoCenter);
                        }

                        //Si eligio un eje, iniciar dragging
                        if (gizmoMesh.SelectedAxis != TranslateGizmoMesh.Axis.None)
                        {
                            currentState = State.Dragging;
                        }
                        else
                        {
                            if (input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                            {
                                bool additive = input.keyDown(Microsoft.DirectX.DirectInput.Key.LeftControl) || input.keyDown(Microsoft.DirectX.DirectInput.Key.RightControl);
                                editablePoly.CurrentState = EditablePoly.State.SelectObject;
                                editablePoly.doDirectSelection(additive);
                            }
                        }
                    }
                    //Hacer mouse over sobre los ejes
                    else
                    {
                        //Buscar colision con eje con Picking
                        pickingRay.updateRay();
                        gizmoMesh.selectAxisByPicking(pickingRay.Ray);

                    }

                    break;

                case State.Dragging:

                    //Mover
                    if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                    {
                        pickingRay.updateRay();
                        Vector3 endMouseP3d = initMouseP3d;

                        //Solo se mueve un eje
                        Vector3 currentMove = new Vector3(0, 0, 0);
                        if (gizmoMesh.isSingleAxis(gizmoMesh.SelectedAxis))
                        {
                            //Desplazamiento en eje X
                            if (gizmoMesh.SelectedAxis == TranslateGizmoMesh.Axis.X)
                            {
                                endMouseP3d = editablePoly.Control.Grid.getPickingX(pickingRay.Ray, gizmoMesh.GizmoCenter);
                                currentMove.X = endMouseP3d.X - initMouseP3d.X;
                            }
                            //Desplazamiento en eje Y
                            else if (gizmoMesh.SelectedAxis == TranslateGizmoMesh.Axis.Y)
                            {
                                endMouseP3d = editablePoly.Control.Grid.getPickingY(pickingRay.Ray, gizmoMesh.GizmoCenter);
                                currentMove.Y = endMouseP3d.Y - initMouseP3d.Y;
                            }
                            //Desplazamiento en eje Z
                            else if (gizmoMesh.SelectedAxis == TranslateGizmoMesh.Axis.Z)
                            {
                                endMouseP3d = editablePoly.Control.Grid.getPickingZ(pickingRay.Ray, gizmoMesh.GizmoCenter);
                                currentMove.Z = endMouseP3d.Z - initMouseP3d.Z;
                            }
                        }
                        //Desplazamiento en dos ejes
                        else
                        {
                            //Plano XZ
                            if (gizmoMesh.SelectedAxis == TranslateGizmoMesh.Axis.XZ)
                            {
                                endMouseP3d = editablePoly.Control.Grid.getPickingXZ(pickingRay.Ray, gizmoMesh.GizmoCenter);
                                currentMove.X = endMouseP3d.X - initMouseP3d.X;
                                currentMove.Z = endMouseP3d.Z - initMouseP3d.Z;
                            }
                            //Plano XY
                            else if (gizmoMesh.SelectedAxis == TranslateGizmoMesh.Axis.XY)
                            {
                                endMouseP3d = editablePoly.Control.Grid.getPickingXY(pickingRay.Ray, gizmoMesh.GizmoCenter);
                                currentMove.X = endMouseP3d.X - initMouseP3d.X;
                                currentMove.Y = endMouseP3d.Y - initMouseP3d.Y;
                            }
                            //Plano YZ
                            else if (gizmoMesh.SelectedAxis == TranslateGizmoMesh.Axis.YZ)
                            {
                                endMouseP3d = editablePoly.Control.Grid.getPickingYZ(pickingRay.Ray, gizmoMesh.GizmoCenter);
                                currentMove.Y = endMouseP3d.Y - initMouseP3d.Y;
                                currentMove.Z = endMouseP3d.Z - initMouseP3d.Z;
                            }
                        }

                        //Actualizar pos del mouse 3D para proximo cuadro
                        initMouseP3d = endMouseP3d;

                        //Ajustar currentMove con Snap to grid
                        if (editablePoly.Control.SnapToGridEnabled)
                        {
                            snapMovementToGrid(ref currentMove.X, ref acumMovement.X, editablePoly.Control.SnapToGridCellSize);
                            snapMovementToGrid(ref currentMove.Y, ref acumMovement.Y, editablePoly.Control.SnapToGridCellSize);
                            snapMovementToGrid(ref currentMove.Z, ref acumMovement.Z, editablePoly.Control.SnapToGridCellSize);
                        }
                        if (currentMove.LengthSq() > 0.1f)
                        {
                            //Mover objetos
                            foreach (EditPolyPrimitive p in editablePoly.SelectionList)
                            {
                                p.move(currentMove);
                            }

                            //Mover ejes
                            gizmoMesh.moveGizmo(currentMove);

                            //Actualizar mesh
                            editablePoly.setDirtyValues(false);

                            //Actualizar datos de modify
                            //Control.updateModifyPanel();
                        }
                    }

                    //Soltar movimiento
                    else if (input.buttonUp(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                    {
                        currentState = State.Init;
                        gizmoMesh.unSelect();
                    }


                    break;
            }



            //Ajustar tamaño de ejes
            gizmoMesh.setCenter(gizmoMesh.GizmoCenter, editablePoly.Control.Camera);
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


        /// <summary>
        /// Dibujar
        /// </summary>
        public void render()
        {
            gizmoMesh.render();
        }

        public void dipose()
        {
            gizmoMesh.dispose();
        }

    }
}
