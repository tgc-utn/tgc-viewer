using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TGC.Core.Utils;

namespace Examples.MeshCreator.Primitives
{
    /// <summary>
    ///     Primitiva de plano en el eje XZ
    /// </summary>
    public class PlaneXZPrimitive : EditorPrimitive
    {
        private Vector3 initSelectionPoint;
        private TgcPlaneWall mesh;
        private Vector3 originalSize;

        public PlaneXZPrimitive(MeshCreatorControl control)
            : base(control)
        {
            Name = "Plane_" + PRIMITIVE_COUNT++;
            ModifyCaps.ChangeRotation = false;
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
            get { return new Vector2(mesh.UTile, mesh.VTile); }
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
            set
            {
                mesh.Origin = value;
                mesh.updateValues();
            }
        }

        public override Vector3 Rotation
        {
            get { return Vector3.Empty; }
        }

        public override Vector3 Scale
        {
            get { return TgcVectorUtils.div(mesh.Size, originalSize); }
            set
            {
                var newSize = TgcVectorUtils.mul(originalSize, value);
                mesh.Size = newSize;
                mesh.updateValues();
            }
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
            var color = selected ? MeshCreatorUtils.SELECTED_OBJECT_COLOR : MeshCreatorUtils.UNSELECTED_OBJECT_COLOR;
            mesh.BoundingBox.setRenderColor(color);
        }

        /// <summary>
        ///     Iniciar la creacion
        /// </summary>
        public override void initCreation(Vector3 gridPoint)
        {
            initSelectionPoint = gridPoint;

            //Crear plano inicial
            var planeTexture = TgcTexture.createTexture(Control.getCreationTexturePath());
            mesh = new TgcPlaneWall(initSelectionPoint, new Vector3(0, 0, 0), TgcPlaneWall.Orientations.XZplane,
                planeTexture);
            mesh.AutoAdjustUv = false;
            mesh.UTile = 1;
            mesh.VTile = 1;
            mesh.BoundingBox.setRenderColor(MeshCreatorUtils.UNSELECTED_OBJECT_COLOR);
            Layer = Control.CurrentLayer;
        }

        /// <summary>
        ///     Construir plano
        /// </summary>
        public override void doCreation()
        {
            var input = GuiController.Instance.D3dInput;

            //Si hacen clic con el mouse, ver si hay colision con el suelo
            if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Determinar el size en XZ del box
                var collisionPoint = Control.Grid.getPicking();

                //Obtener extremos del rectángulo de selección
                var min = Vector3.Minimize(initSelectionPoint, collisionPoint);
                var max = Vector3.Maximize(initSelectionPoint, collisionPoint);
                min.Y = 0;
                max.Y = 1;

                //Configurar plano
                mesh.setExtremes(min, max);
                mesh.updateValues();
            }
            //Solto el clic del mouse, generar plano definitivo
            else if (input.buttonUp(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Tiene el tamaño minimo tolerado
                var size = mesh.BoundingBox.calculateSize();
                if (size.X > 1 && size.Z > 1)
                {
                    //Guardar size original del plano para hacer Scaling
                    originalSize = mesh.Size;

                    //Dejar cargado para que se pueda crear un nuevo plano
                    Control.CurrentState = MeshCreatorControl.State.CreatePrimitiveSelected;
                    Control.CreatingPrimitive = new PlaneXZPrimitive(Control);

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

        public override void setRotationFromPivot(Vector3 rotation, Vector3 pivot)
        {
            //NO SOPORTADO ACTUALMENTE
        }

        public override TgcMesh createMeshToExport()
        {
            var m = mesh.toMesh(Name);
            m.UserProperties = UserProperties;
            m.Layer = Layer;
            return m;
        }

        public override EditorPrimitive clone()
        {
            var p = new PlaneXZPrimitive(Control);
            p.mesh = mesh.clone();
            p.originalSize = originalSize;
            p.UserProperties = UserProperties;
            p.Layer = Layer;
            return p;
        }

        public override void updateBoundingBox()
        {
            var m = mesh.toMesh(Name);
            mesh.BoundingBox.setExtremes(m.BoundingBox.PMin, m.BoundingBox.PMax);
            m.dispose();
        }
    }
}