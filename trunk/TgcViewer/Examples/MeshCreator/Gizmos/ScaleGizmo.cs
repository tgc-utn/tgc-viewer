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
    /// Gizmo para escalar objetos
    /// </summary>
    public class ScaleGizmo : EditorGizmo
    {
        const float LARGE_AXIS_SIZE = 200f;
        const float SHORT_AXIS_SIZE = 5f;

        enum Axis
        {
            X, Y, Z, None,
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
        Axis selectedAxis;
        Vector2 initMouseP;
        Vector3 gizmoCenter;

        public ScaleGizmo(MeshCreatorControl control)
            : base(control)
        {
            selectedAxis = Axis.None;
            currentState = State.Init;

            boxX = TgcBox.fromExtremes(new Vector3(0, 0, 0), new Vector3(LARGE_AXIS_SIZE, SHORT_AXIS_SIZE, SHORT_AXIS_SIZE), Color.FromArgb(200, 50, 50));
            boxY = TgcBox.fromExtremes(new Vector3(0, 0, 0), new Vector3(SHORT_AXIS_SIZE, LARGE_AXIS_SIZE, SHORT_AXIS_SIZE), Color.FromArgb(50, 200, 50));
            boxZ = TgcBox.fromExtremes(new Vector3(0, 0, 0), new Vector3(SHORT_AXIS_SIZE, SHORT_AXIS_SIZE, LARGE_AXIS_SIZE), Color.FromArgb(50, 50, 200));
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

            boxX.Position = gizmoCenter + Vector3.Multiply(boxX.Size, 0.5f) + new Vector3(SHORT_AXIS_SIZE, 0, 0);
            boxY.Position = gizmoCenter + Vector3.Multiply(boxY.Size, 0.5f) + new Vector3(0, SHORT_AXIS_SIZE, 0);
            boxZ.Position = gizmoCenter + Vector3.Multiply(boxZ.Size, 0.5f) + new Vector3(0, 0, SHORT_AXIS_SIZE);

            boxX.updateValues();
            boxY.updateValues();
            boxZ.updateValues();
        }

        public override void update()
        {
            TgcD3dInput input = GuiController.Instance.D3dInput;

            switch (currentState)
            {
                case State.Init:

                    //Iniciar seleccion de eje
                    if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
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
                        else
                        {
                            selectedAxis = Axis.None;
                        }

                        //Si eligio un eje, iniciar dragging
                        if (selectedAxis != Axis.None)
                        {
                            currentState = State.Dragging;
                            initMouseP = new Vector2(input.XposRelative, input.YposRelative);
                        }
                        else
                        {
                            if (input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                            {
                                Control.CurrentState = MeshCreatorControl.State.SelectObject;
                                Control.SelectionRectangle.doDirectSelection(false);
                            }
                        }
                    }

                    break;

                case State.Dragging:

                    //Mover
                    if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                    {
                        //Obtener vector 2D de movimiento relativo del mouse
                        Vector2 mouseScreenVec = new Vector2(input.XposRelative, input.YposRelative);
                        //mouseScreenVec.Normalize();

                        //Projectar vector 2D del eje elegido
                        TgcBox currentAxisBox = getAxisBox(selectedAxis);
                        Vector2 axisScreenVec = MeshCreatorUtils.projectAABBScreenVec(currentAxisBox.BoundingBox);
                        
                        //Hacer DOT product entre ambos vectores para obtener la contribucion del mouse en esa direccion
                        float scaling = Vector2.Dot(axisScreenVec, mouseScreenVec);
                        scaling = MeshCreatorUtils.getMouseScaleIncrementSpeed(Control.Camera, currentAxisBox.BoundingBox, scaling);
                        Vector3 currentScale = new Vector3(0, 0, 0);

                        //Escala en eje X
                        if (selectedAxis == Axis.X)
                        {
                            currentScale.X = scaling;
                        }
                        //Escala en eje Y
                        else if (selectedAxis == Axis.Y)
                        {
                            currentScale.Y = scaling;
                        }
                        //Escala en eje Z
                        else if (selectedAxis == Axis.Z)
                        {
                            currentScale.Z = scaling;
                        }


                        //Mover objetos
                        foreach (EditorPrimitive p in Control.SelectionList)
	                    {
                            //Agregar scaling, controlando que no sea menor a cero
                            Vector3 scale = p.Scale;
                            scale += currentScale;
                            scale.X = scale.X < 0.1f ? 0.1f : scale.X;
                            scale.Y = scale.Y < 0.1f ? 0.1f : scale.Y;
                            scale.Z = scale.Z < 0.1f ? 0.1f : scale.Z;

                            p.Scale = scale;
	                    }
                        

                        //TODO: Hay que ir estirando los ejes a medida que se agranda la escala
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
            }
            return null;
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

    }
}
