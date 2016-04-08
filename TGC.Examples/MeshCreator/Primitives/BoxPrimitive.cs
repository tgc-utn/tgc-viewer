using Microsoft.DirectX;
using TGC.Core.Geometries;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.Utils;
using TGC.Viewer;
using TGC.Viewer.Utils.Input;

namespace TGC.Examples.MeshCreator.Primitives
{
    /// <summary>
    ///     Primitiva de Box 3D
    /// </summary>
    public class BoxPrimitive : EditorPrimitive
    {
        private float creatingBoxInitMouseY;
        private CreatingBoxState currentCreatingState;
        private Vector3 initSelectionPoint;

        private TgcBox mesh;
        private Vector3 originalSize;

        public BoxPrimitive(MeshCreatorControl control)
            : base(control)
        {
            Name = "Box_" + PRIMITIVE_COUNT++;
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
            set { mesh.Position = value; }
        }

        public override Vector3 Rotation
        {
            get { return mesh.Rotation; }
        }

        /// <summary>
        ///     Configurar tamaño del box
        /// </summary>
        public override Vector3 Scale
        {
            get
            {
                var size = mesh.BoundingBox.calculateSize();
                return TgcVectorUtils.div(size, originalSize);
            }
            set
            {
                var newSize = TgcVectorUtils.mul(originalSize, value);
                mesh.setPositionSize(mesh.Position, newSize);
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
            currentCreatingState = CreatingBoxState.DraggingSize;

            //Crear caja inicial
            var boxTexture = TgcTexture.createTexture(Control.getCreationTexturePath());
            mesh = TgcBox.fromExtremes(initSelectionPoint, initSelectionPoint, boxTexture);
            mesh.BoundingBox.setRenderColor(MeshCreatorUtils.UNSELECTED_OBJECT_COLOR);
            Layer = Control.CurrentLayer;
        }

        /// <summary>
        ///     Construir caja
        /// </summary>
        public override void doCreation()
        {
            var input = GuiController.Instance.D3dInput;

            switch (currentCreatingState)
            {
                case CreatingBoxState.DraggingSize:

                    //Si hacen clic con el mouse, ver si hay colision con el suelo
                    if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                    {
                        //Determinar el size en XZ del box
                        var collisionPoint = Control.Grid.getPicking();

                        //Obtener extremos del rectángulo de selección
                        var min = Vector3.Minimize(initSelectionPoint, collisionPoint);
                        var max = Vector3.Maximize(initSelectionPoint, collisionPoint);
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
                        var size = mesh.BoundingBox.calculateSize();
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
                        var heightY = creatingBoxInitMouseY - input.Ypos;
                        var adjustedHeightY = MeshCreatorUtils.getMouseIncrementHeightSpeed(Control.Camera, BoundingBox,
                            heightY);

                        var min = mesh.BoundingBox.PMin;
                        min.Y = initSelectionPoint.Y;
                        var max = mesh.BoundingBox.PMax;
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
            var p = new BoxPrimitive(Control);
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

        /// <summary>
        ///     Estado cuando se esta creando un Box
        /// </summary>
        private enum CreatingBoxState
        {
            DraggingSize,
            DraggingHeight
        }
    }
}