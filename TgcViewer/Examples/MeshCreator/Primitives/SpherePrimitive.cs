using Microsoft.DirectX;
using TGC.Viewer;
using TGC.Viewer.Utils.Input;
using TGC.Viewer.Utils.TgcGeometry;
using TGC.Viewer.Utils.TgcSceneLoader;

namespace TGC.Examples.MeshCreator.Primitives
{
    /// <summary>
    ///     Primitiva de Sphere 3D
    /// </summary>
    public class SpherePrimitive : EditorPrimitive
    {
        private readonly TgcBoundingBox bb;
        private Vector3 initSelectionPoint;
        private TgcSphere mesh;
        private float originalRadius;
        private float scale = 1;

        public SpherePrimitive(MeshCreatorControl control)
            : base(control)
        {
            bb = new TgcBoundingBox();
            Name = "Sphere_" + PRIMITIVE_COUNT++;
        }

        /*public override TgcBoundingSphere BoundingSphere
        {
            get { return mesh.BoundingSphere; }
        }*/

        public override TgcBoundingBox BoundingBox
        {
            get { return bb; }
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
            get { return mesh.UVTiling; }
            set
            {
                mesh.UVTiling = value;
                mesh.updateValues();
            }
        }

        public override Vector3 Position
        {
            get { return mesh.Position; }
            set
            {
                mesh.Position = value;
                updateBB();
            }
        }

        public override Vector3 Rotation
        {
            get { return mesh.Rotation; }
        }

        /// <summary>
        ///     Configurar tamaño del sphere
        /// </summary>
        public override Vector3 Scale
        {
            get { return new Vector3(scale, scale, scale); }
            set
            {
                if (scale != value.X)
                {
                    scale = value.X;
                }
                else if (scale != value.Y)
                {
                    scale = value.Y;
                }
                else if (scale != value.Z)
                {
                    scale = value.Z;
                }

                mesh.Radius = originalRadius * scale;

                mesh.updateValues();
                updateBB();
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
            // mesh.BoundingSphere.setRenderColor(color);
            bb.setRenderColor(color);
        }

        /// <summary>
        ///     Iniciar la creacion
        /// </summary>
        public override void initCreation(Vector3 gridPoint)
        {
            initSelectionPoint = gridPoint;

            //Crear caja inicial
            var sphereTexture = TgcTexture.createTexture(Control.getCreationTexturePath());
            mesh = new TgcSphere();

            mesh.setTexture(sphereTexture);
            // mesh.BoundingSphere.setRenderColor(MeshCreatorUtils.UNSELECTED_OBJECT_COLOR);
            bb.setRenderColor(MeshCreatorUtils.UNSELECTED_OBJECT_COLOR);
            Layer = Control.CurrentLayer;
        }

        /// <summary>
        ///     Construir caja
        /// </summary>
        public override void doCreation()
        {
            var input = GuiController.Instance.D3dInput;

            //Si hacen clic con el mouse, ver si hay colision con el suelo
            if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Determinar el size en XZ del box
                var collisionPoint = Control.Grid.getPicking();

                mesh.Position = initSelectionPoint;
                //Configurar BOX
                mesh.Radius = (collisionPoint - initSelectionPoint).Length();
                mesh.updateValues();
            }
            else
            {
                originalRadius = mesh.Radius;
                updateBB();
                //Dejar cargado para que se pueda crear un nuevo sphere
                Control.CurrentState = MeshCreatorControl.State.CreatePrimitiveSelected;
                Control.CreatingPrimitive = new SpherePrimitive(Control);

                //Agregar sphere a la lista de modelos
                Control.addMesh(this);

                //Seleccionar Box
                Control.SelectionRectangle.clearSelection();
                Control.SelectionRectangle.selectObject(this);
                Control.updateModifyPanel();
            }
        }

        public override void move(Vector3 move)
        {
            mesh.move(move);
            updateBB();
        }

        private void updateBB()
        {
            var r = new Vector3(mesh.Radius, mesh.Radius, mesh.Radius);
            bb.setExtremes(Vector3.Subtract(mesh.Position, r), Vector3.Add(mesh.Position, r));
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
            mesh.Rotation = rotation;
            var translation = pivot - mesh.Position;
            var m = Matrix.Translation(-translation) * Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z) *
                    Matrix.Translation(translation);
            mesh.move(new Vector3(m.M41, m.M42, m.M43));
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
            var p = new SpherePrimitive(Control);
            p.mesh = mesh.clone();
            p.originalRadius = originalRadius;
            p.Scale = Scale;
            p.UserProperties = UserProperties;
            p.Layer = Layer;
            return p;
        }

        public override void updateBoundingBox()
        {
            var m = mesh.toMesh(Name);
            bb.setExtremes(m.BoundingBox.PMin, m.BoundingBox.PMax);
            m.dispose();
        }
    }
}