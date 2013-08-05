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
    /// Primitiva de Sphere 3D
    /// </summary>
    public class SpherePrimitive : EditorPrimitive
    {

    
        TgcSphere mesh;
        Vector3 initSelectionPoint;
        TgcBoundingBox bb;
        float originalRadius;
        float scale = 1;

        public SpherePrimitive(MeshCreatorControl control)
            : base(control)
        {
            this.bb = new TgcBoundingBox();
            this.Name = "Sphere_" + EditorPrimitive.PRIMITIVE_COUNT++;
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
           // mesh.BoundingSphere.setRenderColor(color);
            bb.setRenderColor(color);
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

        /// <summary>
        /// Iniciar la creacion
        /// </summary>
        public override void initCreation(Vector3 gridPoint)
        {
            initSelectionPoint = gridPoint;

            //Crear caja inicial
            TgcTexture sphereTexture = TgcTexture.createTexture(Control.getCreationTexturePath());
            mesh = new TgcSphere();
       
            mesh.setTexture(sphereTexture);
           // mesh.BoundingSphere.setRenderColor(MeshCreatorUtils.UNSELECTED_OBJECT_COLOR);
            bb.setRenderColor(MeshCreatorUtils.UNSELECTED_OBJECT_COLOR);
        }

        /// <summary>
        /// Construir caja
        /// </summary>
        public override void doCreation()
        {
            TgcD3dInput input = GuiController.Instance.D3dInput;

            

            //Si hacen clic con el mouse, ver si hay colision con el suelo
            if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Determinar el size en XZ del box
                Vector3 collisionPoint = Control.Grid.getPicking();

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

            Vector3 r = new Vector3(mesh.Radius, mesh.Radius, mesh.Radius);
            bb.setExtremes(Vector3.Subtract(mesh.Position, r), Vector3.Add(mesh.Position, r));
        }

        public override TgcTexture Texture
        {
            get { return mesh.Texture; }
            set { mesh.setTexture(value); }
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
            set { mesh.Rotation = value; }
        }

        /// <summary>
        /// Configurar tamaño del sphere
        /// </summary>
        public override Vector3 Scale
        {
            get
            {
                return new Vector3(scale, scale, scale);
            }
            set
            {
                if (scale != value.X){ scale = value.X;}
                
                else if (scale != value.Y){scale = value.Y;}

                else if (scale != value.Z) { scale = value.Z; }

                mesh.Radius = originalRadius * scale;              
               
                mesh.updateValues();
                updateBB();

            }
        }

        public override TgcMesh createMeshToExport()
        {
            TgcMesh m = mesh.toMesh(this.Name);
            m.UserProperties = this.UserProperties;
            return m;
        }

        public override EditorPrimitive clone()
        {
            SpherePrimitive p = new SpherePrimitive(this.Control);
            p.mesh = this.mesh.clone();
            p.originalRadius = this.originalRadius;
            p.Scale = this.Scale;
            p.UserProperties = this.UserProperties;
           
            return p;
        }

    }
}
