using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using TgcViewer.Utils.Input;
using TgcViewer;
using System.Drawing;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.MeshCreator.Primitives
{
    /// <summary>
    /// Primitiva de Box 3D
    /// </summary>
    public class BoxPrimitive : EditorPrimitive
    {

        /// <summary>
        /// Estado cuando se esta creando un Box
        /// </summary>
        enum CreatingBoxState
        {
            DraggingSize,
            DraggingHeight,
        }

        TgcBox mesh;
        CreatingBoxState currentCreatingState;
        Vector3 initSelectionPoint;
        float creatingBoxInitMouseY;
        Vector3 originalSize;


        public BoxPrimitive(MeshCreatorControl control)
            : base(control)
        {
            this.Name = "Box_" + EditorPrimitive.PRIMITIVE_COUNT++;
        }


        public override void render()
        {
            mesh.render();
        }


        public override void dispose()
        {
            mesh.dispose();
        }

        public override void setSelected(bool selected)
        {
            this.selected = selected;
            Color color = selected ? MeshCreatorUtils.SELECTED_OBJECT_COLOR : MeshCreatorUtils.UNSELECTED_OBJECT_COLOR;
            mesh.BoundingBox.setRenderColor(color);
        }

        public override TgcBoundingBox BoundingBox
        {
            get { return mesh.BoundingBox; }
        }

        public override bool AlphaBlendEnable
        {
            get { return mesh.AlphaBlendEnable; }
            set { mesh.AlphaBlendEnable = value; }
        }

        /// <summary>
        /// Iniciar la creacion
        /// </summary>
        public override void initCreation(Vector3 gridPoint)
        {
            initSelectionPoint = gridPoint;
            currentCreatingState = CreatingBoxState.DraggingSize;

            //Crear caja inicial
            TgcTexture boxTexture = TgcTexture.createTexture(Control.getCreationTexturePath());
            mesh = TgcBox.fromExtremes(initSelectionPoint, initSelectionPoint, boxTexture);
            mesh.BoundingBox.setRenderColor(MeshCreatorUtils.UNSELECTED_OBJECT_COLOR);
            this.Layer = Control.CurrentLayer;
        }

        /// <summary>
        /// Construir caja
        /// </summary>
        public override void doCreation()
        {
            TgcD3dInput input = GuiController.Instance.D3dInput;

            switch (currentCreatingState)
            {
                case CreatingBoxState.DraggingSize:

                    //Si hacen clic con el mouse, ver si hay colision con el suelo
                    if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                    {
                        //Determinar el size en XZ del box
                        Vector3 collisionPoint = Control.Grid.getPicking();

                        //Obtener extremos del rectángulo de selección
                        Vector3 min = Vector3.Minimize(initSelectionPoint, collisionPoint);
                        Vector3 max = Vector3.Maximize(initSelectionPoint, collisionPoint);
                        min.Y = initSelectionPoint.Y;
                        max.Y = initSelectionPoint.Y + 0.2f;

                        //Configurar BOX
                        mesh.setExtremes(min, max);
                        mesh.updateValues();

                    }
                    //Solto el clic del mouse, pasar a configurar el Height del box
                    else if (input.buttonUp(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                    {
                        //Tiene el tamaño minimo tolerado
                        Vector3 size = mesh.BoundingBox.calculateSize();
                        if (size.X > 1 && size.Z > 1)
                        {
                            currentCreatingState = CreatingBoxState.DraggingHeight;
                            creatingBoxInitMouseY = input.Ypos;
                        }
                        //Sino, descartar
                        else
                        {
                            Control.CurrentState = MeshCreatorControl.State.CreatePrimitiveSelected;
                            mesh.dispose();
                            mesh = null;
                        }

                    }

                    break;

                case CreatingBoxState.DraggingHeight:

                    //Si presiona clic, terminar de configurar la altura y generar box definitivo
                    if (input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                    {
                        //Guardar size original del Box para hacer Scaling
                        originalSize = mesh.BoundingBox.calculateSize();

                        //Dejar cargado para que se pueda crear un nuevo box
                        Control.CurrentState = MeshCreatorControl.State.CreatePrimitiveSelected;
                        Control.CreatingPrimitive = new BoxPrimitive(Control);

                        //Agregar box a la lista de modelos
                        Control.addMesh(this);

                        //Seleccionar Box
                        Control.SelectionRectangle.clearSelection();
                        Control.SelectionRectangle.selectObject(this);
                        Control.updateModifyPanel();
                    }
                    //Determinar altura en base a la posicion Y del mouse
                    else
                    {
                        float heightY = creatingBoxInitMouseY - input.Ypos;
                        float adjustedHeightY = MeshCreatorUtils.getMouseIncrementHeightSpeed(Control.Camera, this.BoundingBox, heightY);

                        Vector3 min = mesh.BoundingBox.PMin;
                        min.Y = initSelectionPoint.Y;
                        Vector3 max = mesh.BoundingBox.PMax;
                        max.Y = initSelectionPoint.Y + adjustedHeightY;

                        //Configurar BOX
                        mesh.setExtremes(min, max);
                        mesh.updateValues();
                    }

                    break;
            }
        }


        public override void move(Vector3 move)
        {
            mesh.move(move);
        }

        public override void setTexture(TgcTexture texture, int slot)
        {
            mesh.setTexture(texture);
        }

        public override TgcTexture getTexture(int slot)
        {
            return mesh.Texture;
        }

        public override Vector2 TextureOffset
        {
            get { return mesh.UVOffset; }
            set { 
                mesh.UVOffset = value;
                mesh.updateValues();
            }
        }

        public override Vector2 TextureTiling
        {
            get { return mesh.UVTiling; }
            set { 
                mesh.UVTiling = value;
                mesh.updateValues();
            }
        }

        public override Vector3 Position
        {
            get { return mesh.Position; }
            set { mesh.Position = value; }
        }

        public override Vector3 Rotation
        {
            get { return mesh.Rotation; }
            set { mesh.Rotation = value; }
        }

        /// <summary>
        /// Configurar tamaño del box
        /// </summary>
        public override Vector3 Scale
        {
            get {
                Vector3 size = mesh.BoundingBox.calculateSize();
                return TgcVectorUtils.div(size, originalSize);
            }
            set
            {
                Vector3 newSize = TgcVectorUtils.mul(originalSize, value);
                mesh.setPositionSize(mesh.Position, newSize);
                mesh.updateValues();
            }
        }

        public override TgcMesh createMeshToExport()
        {
            TgcMesh m = mesh.toMesh(this.Name);
            m.UserProperties = this.UserProperties;
            m.Layer = this.Layer;
            return m;
        }

        public override EditorPrimitive clone()
        {
            BoxPrimitive p = new BoxPrimitive(this.Control);
            p.mesh = this.mesh.clone();
            p.originalSize = this.originalSize;
            p.UserProperties = this.UserProperties;
            p.Layer = this.Layer;
            return p;
        }

        public override void updateBoundingBox()
        {
            TgcMesh m = mesh.toMesh(this.Name);
            this.mesh.BoundingBox.setExtremes(m.BoundingBox.PMin, m.BoundingBox.PMax);
            m.dispose();
        }

    }
}
