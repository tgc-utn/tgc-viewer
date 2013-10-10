using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;
using System.Drawing;
using TgcViewer.Utils.Input;
using TgcViewer;

namespace Examples.MeshCreator.Primitives
{
    /// <summary>
    /// Primitiva de plano en el eje XY
    /// </summary>
    public class PlaneXYPrimitive : EditorPrimitive
    {

        TgcPlaneWall mesh;
        Vector3 initSelectionPoint;
        Vector3 originalSize;
        float creatingInitMouseY;

        public PlaneXYPrimitive(MeshCreatorControl control)
            : base(control)
        {
            this.Name = "Plane_" + EditorPrimitive.PRIMITIVE_COUNT++;
            this.ModifyCaps.ChangeRotation = false;
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
            creatingInitMouseY = GuiController.Instance.D3dInput.Ypos;

            //Crear plano inicial
            TgcTexture planeTexture = TgcTexture.createTexture(Control.getCreationTexturePath());
            mesh = new TgcPlaneWall(initSelectionPoint, new Vector3(0, 0, 0), TgcPlaneWall.Orientations.XYplane, planeTexture);
            mesh.AutoAdjustUv = false;
            mesh.UTile = 1;
            mesh.VTile = 1;
            mesh.BoundingBox.setRenderColor(MeshCreatorUtils.UNSELECTED_OBJECT_COLOR);
            this.Layer = Control.CurrentLayer;
        }

        /// <summary>
        /// Construir plano
        /// </summary>
        public override void doCreation()
        {
            TgcD3dInput input = GuiController.Instance.D3dInput;

            //Si hacen clic con el mouse, ver si hay colision con el suelo
            if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Obtener altura en Y segun movimient en Y del mouse
                float heightY = creatingInitMouseY - input.Ypos;
                float adjustedHeightY = MeshCreatorUtils.getMouseIncrementHeightSpeed(Control.Camera, this.BoundingBox, heightY);

                //Determinar posicion X segun la colision con el grid
                Vector3 collisionPoint = Control.Grid.getPicking();
                Vector3 extensionPoint = new Vector3(collisionPoint.X, initSelectionPoint.Y + adjustedHeightY, initSelectionPoint.Z);

                //Obtener maximo y minimo
                Vector3 min = Vector3.Minimize(initSelectionPoint, extensionPoint);
                Vector3 max = Vector3.Maximize(initSelectionPoint, extensionPoint);
                min.Z = initSelectionPoint.Z;
                max.Z = initSelectionPoint.Z + 1;

                //Configurar plano
                mesh.setExtremes(min, max);
                mesh.updateValues();

            }
            //Solto el clic del mouse, generar plano definitivo
            else if (input.buttonUp(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Tiene el tamaño minimo tolerado
                Vector3 size = mesh.BoundingBox.calculateSize();
                if (size.X > 1 && size.Y > 1)
                {
                    //Guardar size original del plano para hacer Scaling
                    originalSize = mesh.Size;

                    //Dejar cargado para que se pueda crear un nuevo plano
                    Control.CurrentState = MeshCreatorControl.State.CreatePrimitiveSelected;
                    Control.CreatingPrimitive = new PlaneXYPrimitive(Control);

                    //Agregar plano a la lista de modelos
                    Control.addMesh(this);

                    //Seleccionar plano
                    Control.SelectionRectangle.clearSelection();
                    Control.SelectionRectangle.selectObject(this);
                    Control.updateModifyPanel();
                }
                //Sino, descartar
                else
                {
                    Control.CurrentState = MeshCreatorControl.State.CreatePrimitiveSelected;
                    mesh.dispose();
                    mesh = null;
                }
            }

        }

        public override void move(Vector3 move)
        {
            mesh.Origin += move;
            mesh.updateValues();
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
            set
            {
                mesh.UVOffset = value;
                mesh.updateValues();
            }
        }

        public override Vector2 TextureTiling
        {
            get
            {
                return new Vector2(mesh.UTile, mesh.VTile);
            }
            set
            {
                mesh.UTile = value.X;
                mesh.VTile = value.Y;
                mesh.updateValues();
            }
        }

        public override Vector3 Position
        {
            get { return mesh.Origin; }
            set { 
                mesh.Origin = value;
                mesh.updateValues();
            }
        }

        public override Vector3 Rotation
        {
            get { return Vector3.Empty; }
            set { 
                //NO SOPORTADO ACTUALMENTE
                /*mesh.Rotation = value;*/ 
            }
        }

        public override Vector3 Scale
        {
            get
            {
                return TgcVectorUtils.div(mesh.Size, originalSize);
            }
            set
            {
                Vector3 newSize = TgcVectorUtils.mul(originalSize, value);
                mesh.Size = newSize; 
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
            PlaneXYPrimitive p = new PlaneXYPrimitive(this.Control);
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
