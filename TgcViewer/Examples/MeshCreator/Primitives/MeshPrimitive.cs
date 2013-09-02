using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;
using System.Drawing;

namespace Examples.MeshCreator.Primitives
{
    /// <summary>
    /// Primitiva que representa un Mesh importado
    /// </summary>
    public class MeshPrimitive : EditorPrimitive
    {
        TgcMesh mesh;

        public MeshPrimitive(MeshCreatorControl control, TgcMesh mesh)
            : base(control)
        {
            this.Name = mesh.Name + "_" + EditorPrimitive.PRIMITIVE_COUNT++;
            this.mesh = mesh;
            this.ModifyCaps.ChangeTexture = false;
            this.ModifyCaps.ChangeOffsetUV = false;
            this.ModifyCaps.ChangeTilingUV = false;
            this.UserProperties = this.mesh.UserProperties;
            this.Layer = this.mesh.Layer;
        }

        public override void render()
        {
            mesh.render();
        }


        public override void dispose()
        {
            //Al hacer Dispose por haber eliminado un mesh, se caga el render de los demas Mesh, es totalmente inexplicable
            //mesh.dispose();

            mesh = null;
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

        public override void initCreation(Vector3 gridPoint)
        {
            throw new NotImplementedException("Nunca se deberia iniciar una creacion de primitiva para un Mesh. Siempre se importan");
        }

        public override void doCreation()
        {
            throw new NotImplementedException("Nunca se deberia iniciar una creacion de primitiva para un Mesh. Siempre se importan");
        }

        public override void move(Vector3 move)
        {
            mesh.move(move);
        }

        public override TgcTexture Texture
        {
            get { return null; }
            set { ; }
        }

        public override Vector2 TextureOffset
        {
            get { return Vector2.Empty; }
            set { ; }
        }

        public override Vector2 TextureTiling
        {
            get { return Vector2.Empty; }
            set { ; }
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

        public override Vector3 Scale
        {
            get { return mesh.Scale; }
            set { mesh.Scale = value; }
        }

        public override TgcMesh createMeshToExport()
        {
            mesh.UserProperties = this.UserProperties;
            mesh.Layer = this.Layer;
            return mesh;
        }

        public override EditorPrimitive clone()
        {
            mesh.UserProperties = this.UserProperties;
            TgcMesh cloneMesh = mesh.clone(mesh.Name);
            return new MeshPrimitive(this.Control, cloneMesh);
        }



    }
}
