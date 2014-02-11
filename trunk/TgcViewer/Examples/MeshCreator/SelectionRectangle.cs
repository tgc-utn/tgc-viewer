﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils.Input;
using TgcViewer;
using Microsoft.DirectX;
using System.Drawing;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils;
using Examples.MeshCreator.Primitives;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.MeshCreator
{
    /// <summary>
    /// Cuadro 2D de seleccion de objetos con el mouse
    /// </summary>
    public class SelectionRectangle
    {

        MeshCreatorControl control;
        Vector2 initMousePos;
        List<TgcBoundingBox> auxBoundingBoxList;
        bool selectiveObjectsAdditive;
        SelectionRectangleMesh rectMesh;

        public SelectionRectangle(MeshCreatorControl control)
        {
            this.control = control;
            this.rectMesh = new SelectionRectangleMesh();
            auxBoundingBoxList = new List<TgcBoundingBox>();
            this.selectiveObjectsAdditive = false;
        }

        /// <summary>
        /// Iniciar seleccion
        /// </summary>
        public void initSelection(Vector2 mousePos)
        {
            this.initMousePos = mousePos;
        }

        /// <summary>
        /// Bucle general de actualizacion de estado
        /// </summary>
        public void doSelectObject()
        {
            TgcD3dInput input = GuiController.Instance.D3dInput;

            //Si mantiene control y clic con el mouse, iniciar cuadro de seleccion para agregar/quitar a la seleccion actual
            if ((input.keyDown(Microsoft.DirectX.DirectInput.Key.LeftControl) || input.keyDown(Microsoft.DirectX.DirectInput.Key.RightControl))
                && input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                control.CurrentState = MeshCreatorControl.State.SelectingObject;
                this.initMousePos = new Vector2(input.Xpos, input.Ypos);
                this.selectiveObjectsAdditive = true;
            }
            //Si mantiene el clic con el mouse, iniciar cuadro de seleccion
            else if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                control.CurrentState = MeshCreatorControl.State.SelectingObject;
                this.initMousePos = new Vector2(input.Xpos, input.Ypos);
                this.selectiveObjectsAdditive = false;
            }
        }


        /// <summary>
        /// Actualizar y dibujar seleccion
        /// </summary>
        public void render()
        {
            TgcD3dInput input = GuiController.Instance.D3dInput;

            //Mantiene el mouse apretado
            if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Definir recuadro
                Vector2 mousePos = new Vector2(input.Xpos, input.Ypos);
                Vector2 min = Vector2.Minimize(initMousePos, mousePos);
                Vector2 max = Vector2.Maximize(initMousePos, mousePos);

                rectMesh.updateMesh(min, max);

            }
            //Solo el mouse
            else if(input.buttonUp(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Definir recuadro
                Vector2 mousePos = new Vector2(input.Xpos, input.Ypos);
                Vector2 min = Vector2.Minimize(initMousePos, mousePos);
                Vector2 max = Vector2.Maximize(initMousePos, mousePos);
                Rectangle r = new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));

                //Usar recuadro de seleccion solo si tiene un tamaño minimo
                if (r.Width > 1 && r.Height > 1)
                {
                    //Limpiar seleccionar anterior si no estamos agregando en forma aditiva
                    if (!selectiveObjectsAdditive)
                    {
                        clearSelection();
                    }

                    //Buscar que objetos del escenario caen dentro de la seleccion y elegirlos
                    foreach (EditorPrimitive p in control.Meshes)
                    {
                        //Solo los visibles
                        if (p.Visible)
                        {
                            //Ver si hay colision contra la proyeccion del AABB del mesh
                            //Rectangle primRect = MeshCreatorUtils.projectAABB(p.BoundingBox);
                            Rectangle primRect;
                            if (MeshCreatorUtils.projectBoundingBox(p.BoundingBox, out primRect))
                            {
                                if (r.IntersectsWith(primRect))
                                {
                                    //Agregar el objeto en forma aditiva
                                    if (selectiveObjectsAdditive)
                                    {
                                        selectOrRemoveObjectIfPresent(p);
                                    }
                                    //Agregar el objeto en forma simple
                                    else
                                    {
                                        selectObject(p);
                                    }
                                }
                            }
                        }
                    }
                }
                //Si el recuadro no tiene tamaño suficiente, hacer seleccion directa
                else
                {
                    doDirectSelection(selectiveObjectsAdditive);
                }

                control.CurrentState = MeshCreatorControl.State.SelectObject;

                //Si quedo algo seleccionado activar gizmo
                if (control.SelectionList.Count > 0)
                {
                    activateCurrentGizmo();
                }

                //Actualizar panel de Modify con lo que se haya seleccionado, o lo que no
                control.updateModifyPanel();
            }



            //Dibujar recuadro
            rectMesh.render();
        }

        /// <summary>
        /// Selecciona un solo objeto
        /// </summary>
        public void selectObject(EditorPrimitive p)
        {
            p.setSelected(true);
            control.SelectionList.Add(p);
        }

        /// <summary>
        /// Selecciona un solo objeto pero antes se fija si ya no estaba en la lista de seleccion.
        /// Si ya estaba, entonces lo quita de la lista de seleccion
        /// </summary>
        public void selectOrRemoveObjectIfPresent(EditorPrimitive p)
        {
            //Ya existe, quitar
            if (control.SelectionList.Contains(p))
            {
                p.setSelected(false);
                control.SelectionList.Remove(p);
            }
            //No existe, agregar
            else
            {
                p.setSelected(true);
                control.SelectionList.Add(p);
            }
        }

        /// <summary>
        /// Seleccionar todo
        /// </summary>
        public void selectAll()
        {
            clearSelection();
            foreach (EditorPrimitive p in control.Meshes)
            {
                //Solo los visibles
                if (p.Visible)
                {
                    selectObject(p);
                }
            }

            control.CurrentState = MeshCreatorControl.State.SelectObject;

            //Si quedo algo seleccionado activar gizmo
            if (control.SelectionList.Count > 0)
            {
                activateCurrentGizmo();
            }

            //Actualizar panel de Modify con lo que se haya seleccionado, o lo que no
            control.updateModifyPanel();
        }

        /// <summary>
        /// Deseleccionar todo
        /// </summary>
        public void clearSelection()
        {
            foreach (EditorPrimitive p in control.SelectionList)
            {
                p.setSelected(false);
            }
            control.SelectionList.Clear();
            control.updateModifyPanel();
        }


        public void dispose()
        {
            rectMesh.dipose();
            auxBoundingBoxList = null;
        }



        /// <summary>
        /// Hacer picking para seleccionar el objeto mas cercano del ecenario.
        /// </summary>
        /// <param name="additive">En True agrega/quita el objeto a la seleccion actual</param>
        public void doDirectSelection(bool additive)
        {
            control.PickingRay.updateRay();

            //Buscar menor colision con objetos
            float minDistSq = float.MaxValue;
            EditorPrimitive closestPrimitive = null;
            Vector3 q;
            foreach (EditorPrimitive p in control.Meshes)
            {
                //Solo los visibles
                if (p.Visible)
                {
                    if (TgcCollisionUtils.intersectRayAABB(control.PickingRay.Ray, p.BoundingBox, out q))
                    {
                        float lengthSq = Vector3.Subtract(control.PickingRay.Ray.Origin, q).LengthSq();
                        if (lengthSq < minDistSq)
                        {
                            minDistSq = lengthSq;
                            closestPrimitive = p;
                        }
                    }
                }
            }

            //Agregar
            if (closestPrimitive != null)
            {
                //Sumar a la lista de seleccion
                if (additive)
                {
                    selectOrRemoveObjectIfPresent(closestPrimitive);
                }
                //Seleccionar uno solo
                else
                {
                    clearSelection();
                    selectObject(closestPrimitive);
                }
                activateCurrentGizmo();
            }
            //Nada seleccionado
            else
            {
                //Limpiar seleccion
                clearSelection();
            }

            //Pasar a modo seleccion
            control.setSelectObjectState();
            control.updateModifyPanel();
        }

        /// <summary>
        /// Activar el gizmo actual para los objetos seleccionados
        /// </summary>
        public void activateCurrentGizmo()
        {
            if (control.CurrentGizmo != null)
            {
                control.CurrentGizmo.setEnabled(true);
            }
        }

        /// <summary>
        /// Centrar la camara sobre un objeto seleccionado
        /// </summary>
        public void zoomObject()
        {
            TgcBoundingBox aabb = MeshCreatorUtils.getSelectionBoundingBox(control.SelectionList);
            if (aabb != null)
            {
                control.Camera.CameraCenter = aabb.calculateBoxCenter();
            }
        }

        /// <summary>
        /// Poner la camara en top view respecto de un objeto seleccionado
        /// </summary>
        public void setTopView()
        {
            TgcBoundingBox aabb = MeshCreatorUtils.getSelectionBoundingBox(control.SelectionList);
            Vector3 lookAt;
            if (aabb != null)
            {
                lookAt = aabb.calculateBoxCenter();
            }
            else
            {
                lookAt = new Vector3(0, 0, 0);
            }
            control.Camera.setFixedView(lookAt, -FastMath.PI_HALF, 0, control.Camera.CameraDistance);
        }

        /// <summary>
        /// Poner la camara en left view respecto de un objeto seleccionado
        /// </summary>
        public void setLeftView()
        {
            TgcBoundingBox aabb = MeshCreatorUtils.getSelectionBoundingBox(control.SelectionList);
            Vector3 lookAt;
            if (aabb != null)
            {
                lookAt = aabb.calculateBoxCenter();
            }
            else
            {
                lookAt = new Vector3(0, 0, 0);
            }
            control.Camera.setFixedView(lookAt, 0, FastMath.PI_HALF, control.Camera.CameraDistance);
        }

        /// <summary>
        /// Poner la camara en front view respecto de un objeto seleccionado
        /// </summary>
        public void setFrontView()
        {
            TgcBoundingBox aabb = MeshCreatorUtils.getSelectionBoundingBox(control.SelectionList);
            Vector3 lookAt;
            if (aabb != null)
            {
                lookAt = aabb.calculateBoxCenter();
            }
            else
            {
                lookAt = new Vector3(0, 0, 0);
            }
            control.Camera.setFixedView(lookAt, 0, 0, control.Camera.CameraDistance);
        }

        /// <summary>
        /// Obtener pivote central para efectuar la rotacion.
        /// Se busca el centro de todos los AABB
        /// </summary>
        public Vector3 getRotationPivot()
        {
            TgcBoundingBox aabb = MeshCreatorUtils.getSelectionBoundingBox(control.SelectionList);
            return aabb.calculateBoxCenter();
        }

    }
}
