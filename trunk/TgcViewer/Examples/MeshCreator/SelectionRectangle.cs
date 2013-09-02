using System;
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
        static readonly int RECT_COLOR = Color.White.ToArgb();


        MeshCreatorControl control;
        CustomVertex.TransformedColored[] vertices;
        Vector2 initMousePos;
        List<TgcBoundingBox> auxBoundingBoxList;

        public SelectionRectangle(MeshCreatorControl control)
        {
            this.control = control;
            vertices = new CustomVertex.TransformedColored[8];
            auxBoundingBoxList = new List<TgcBoundingBox>();
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

            //Hace clic y suelta, picking directo de un solo objeto
            if (input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                doDirectSelection();
            }
            //Si mantiene el clic con el mouse, iniciar cuadro de seleccion
            else if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                control.CurrentState = MeshCreatorControl.State.SelectingObject;
                this.initMousePos = new Vector2(input.Xpos, input.Ypos);
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

                updateMesh(min, max);

            }
            //Solo el mouse
            else if(input.buttonUp(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Definir recuadro
                Vector2 mousePos = new Vector2(input.Xpos, input.Ypos);
                Vector2 min = Vector2.Minimize(initMousePos, mousePos);
                Vector2 max = Vector2.Maximize(initMousePos, mousePos);
                Rectangle r = new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));

                //Seleccionar solo si el recuadro tiene un tamaño minimo
                if (r.Width > 1 && r.Height > 1)
                {
                    //Buscar que objetos del escenario caen dentro de la seleccion y elegirlos
                    clearSelection();
                    foreach (EditorPrimitive p in control.Meshes)
                    {
                        //Solo los visibles
                        if (p.Visible)
                        {
                            //Ver si hay colision
                            Rectangle primRect = MeshCreatorUtils.projectAABB(p.BoundingBox);
                            if (r.IntersectsWith(primRect))
                            {
                                selectObject(p);
                            }
                        }
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



            //Dibujar recuadro
            renderMesh();
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

        /// <summary>
        /// Actualizar mesh del recuadro de seleccion
        /// </summary>
        private void updateMesh(Vector2 min, Vector2 max)
        {
            //Horizontal arriba
            vertices[0] = new CustomVertex.TransformedColored(min.X, min.Y, 0, 1, RECT_COLOR);
            vertices[1] = new CustomVertex.TransformedColored(max.X, min.Y, 0, 1, RECT_COLOR);

            //Horizontal abajo
            vertices[2] = new CustomVertex.TransformedColored(min.X, max.Y, 0, 1, RECT_COLOR);
            vertices[3] = new CustomVertex.TransformedColored(max.X, max.Y, 0, 1, RECT_COLOR);

            //Vertical izquierda
            vertices[4] = new CustomVertex.TransformedColored(min.X, min.Y, 0, 1, RECT_COLOR);
            vertices[5] = new CustomVertex.TransformedColored(min.X, max.Y, 0, 1, RECT_COLOR);

            //Vertical derecha
            vertices[6] = new CustomVertex.TransformedColored(max.X, min.Y, 0, 1, RECT_COLOR);
            vertices[7] = new CustomVertex.TransformedColored(max.X, max.Y, 0, 1, RECT_COLOR);
        }

        /// <summary>
        /// Dibujar recuadro
        /// </summary>
        private void renderMesh()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            texturesManager.clear(0);
            texturesManager.clear(1);
            d3dDevice.Material = TgcD3dDevice.DEFAULT_MATERIAL;
            d3dDevice.Transform.World = Matrix.Identity;

            d3dDevice.VertexFormat = CustomVertex.TransformedColored.Format;
            d3dDevice.DrawUserPrimitives(PrimitiveType.LineList, 4, vertices);
        }

        public void dispose()
        {
            vertices = null;
            auxBoundingBoxList = null;
        }



        /// <summary>
        /// Hacer picking para seleccionar el objeto mas cercano del ecenario.
        /// </summary>
        public void doDirectSelection()
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

            //Seleccionar el mas cerca
            clearSelection();
            control.CurrentState = MeshCreatorControl.State.SelectObject;
            if (closestPrimitive != null)
            {
                selectObject(closestPrimitive);
                activateCurrentGizmo();
            }
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

        
        
    }
}
