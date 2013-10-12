using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;
using System.Drawing;
using Microsoft.DirectX.Direct3D;

namespace Examples.MeshCreator.Primitives
{
    /// <summary>
    /// Primitiva que representa un Mesh importado
    /// </summary>
    public class MeshPrimitive : EditorPrimitive
    {
        TgcMesh mesh;
        Vector2 uvOffset;
        Vector2 uvTile;
        Vector2[] originalUVCoords;

        public MeshPrimitive(MeshCreatorControl control, TgcMesh mesh)
            : base(control)
        {
            this.Name = mesh.Name + "_" + EditorPrimitive.PRIMITIVE_COUNT++;
            this.mesh = mesh;

            //Ver si tiene texturas
            if(mesh.RenderType == TgcMesh.MeshRenderType.DIFFUSE_MAP || mesh.RenderType == TgcMesh.MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP)
            {
                //Tiene, habilitar la edicion
                this.ModifyCaps.ChangeTexture = true;
                this.ModifyCaps.ChangeOffsetUV = true;
                this.ModifyCaps.ChangeTilingUV = true;
                this.ModifyCaps.TextureNumbers = mesh.DiffuseMaps.Length;
                this.originalUVCoords = mesh.getTextureCoordinates();
            }
            else
            {
                //No tiene textura, deshabilitar todo
                this.ModifyCaps.ChangeTexture = false;
                this.ModifyCaps.ChangeOffsetUV = false;
                this.ModifyCaps.ChangeTilingUV = false;
                this.ModifyCaps.TextureNumbers = 0;
            }
            
            this.UserProperties = this.mesh.UserProperties;
            this.uvOffset = new Vector2(0, 0);
            this.uvTile = new Vector2(1, 1);

            //Layer
            if (this.mesh.Layer != null && this.mesh.Layer.Length > 0)
            {
                this.Layer = this.mesh.Layer;
            }
            else
            {
                this.Layer = control.CurrentLayer;
            }

            /*
            //Ubicar mesh en el origen de coordenadas respecto del centro de su AABB
            Vector3 center = this.mesh.BoundingBox.calculateBoxCenter();
            setMeshToOrigin(center);
             */ 
        }

        /// <summary>
        /// Mover vertices del mesh al centro de coordenadas
        /// </summary>
        private void setMeshToOrigin(Vector3 center)
        {
            moveMeshVertices(center);
            this.mesh.Position = -center;
            this.mesh.Transform = Matrix.Identity;
            this.mesh.BoundingBox.setExtremes(this.mesh.BoundingBox.PMin + center * 2, this.mesh.BoundingBox.PMax + center * 2);
            this.mesh.updateBoundingBox();
        }

        /// <summary>
        /// Mover fisicamente los vertices del mesh
        /// </summary>
        private void moveMeshVertices(Vector3 offset)
        {
            switch (mesh.RenderType)
            {
                case TgcMesh.MeshRenderType.VERTEX_COLOR:
                    TgcSceneLoader.VertexColorVertex[] verts1 = (TgcSceneLoader.VertexColorVertex[])mesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.VertexColorVertex), LockFlags.ReadOnly, mesh.D3dMesh.NumberVertices);
                    for (int i = 0; i < verts1.Length; i++)
                    {
                        verts1[i].Position = verts1[i].Position + offset;
                    }
                    mesh.D3dMesh.SetVertexBufferData(verts1, LockFlags.None);
                    mesh.D3dMesh.UnlockVertexBuffer();
                    break;

                case TgcMesh.MeshRenderType.DIFFUSE_MAP:
                    TgcSceneLoader.DiffuseMapVertex[] verts2 = (TgcSceneLoader.DiffuseMapVertex[])mesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapVertex), LockFlags.ReadOnly, mesh.D3dMesh.NumberVertices);
                    for (int i = 0; i < verts2.Length; i++)
                    {
                        verts2[i].Position = verts2[i].Position + offset;
                    }
                    mesh.D3dMesh.SetVertexBufferData(verts2, LockFlags.None);
                    mesh.D3dMesh.UnlockVertexBuffer();
                    break;

                case TgcMesh.MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:
                    TgcSceneLoader.DiffuseMapAndLightmapVertex[] verts3 = (TgcSceneLoader.DiffuseMapAndLightmapVertex[])mesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapAndLightmapVertex), LockFlags.ReadOnly, mesh.D3dMesh.NumberVertices);
                    for (int i = 0; i < verts3.Length; i++)
                    {
                        verts3[i].Position = verts3[i].Position + offset;
                    }
                    mesh.D3dMesh.SetVertexBufferData(verts3, LockFlags.None);
                    mesh.D3dMesh.UnlockVertexBuffer();
                    break;
            }
        }

        public override void render()
        {
            mesh.render();
        }


        public override void dispose()
        {
            mesh.dispose();
            mesh = null;
            originalUVCoords = null;
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

        public override void setTexture(TgcTexture texture, int slot)
        {
            TgcTexture[] newTextures = new TgcTexture[mesh.DiffuseMaps.Length];
            for (int i = 0; i < newTextures.Length; i++)
            {
                if (i != slot)
                {
                    newTextures[i] = mesh.DiffuseMaps[i].clone();
                }
                else
                {
                    newTextures[i] = texture;
                }
            }
            mesh.changeDiffuseMaps(newTextures);
        }

        public override TgcTexture getTexture(int slot)
        {
            return mesh.DiffuseMaps[slot];
        }

        public override Vector2 TextureOffset
        {
            get { return this.uvOffset; }
            set {
                this.uvOffset = value;
                updateTextureCoordinates();
            }
        }

        public override Vector2 TextureTiling
        {
            get { return this.uvTile; }
            set
            {
                this.uvTile = value;
                updateTextureCoordinates();
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

        public override Vector3 Scale
        {
            get { return mesh.Scale; }
            set { mesh.Scale = value; }
        }

        public override TgcMesh createMeshToExport()
        {
            mesh.UserProperties = this.UserProperties;
            mesh.Layer = this.Layer;
            TgcMesh cloneMesh = mesh.clone(mesh.Name);
            return cloneMesh;
        }

        public override EditorPrimitive clone()
        {
            mesh.UserProperties = this.UserProperties;
            mesh.Layer = this.Layer;
            TgcMesh cloneMesh = mesh.clone(mesh.Name);
            return new MeshPrimitive(this.Control, cloneMesh);
        }

        /// <summary>
        /// Actualizar coordenadas de textura del mesh en base al offset y tiling
        /// </summary>
        private void updateTextureCoordinates()
        {
            switch (mesh.RenderType)
            {
                case TgcMesh.MeshRenderType.DIFFUSE_MAP:
                    TgcSceneLoader.DiffuseMapVertex[] verts = (TgcSceneLoader.DiffuseMapVertex[])mesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapVertex), LockFlags.ReadOnly, mesh.D3dMesh.NumberVertices);
                    for (int i = 0; i < verts.Length; i++)
                    {
                        verts[i].Tu = uvOffset.X + originalUVCoords[i].X * uvTile.X;
                        verts[i].Tv = uvOffset.Y + originalUVCoords[i].Y * uvTile.Y;
                    }
                    mesh.D3dMesh.SetVertexBufferData(verts, LockFlags.None);
                    mesh.D3dMesh.UnlockVertexBuffer();
                    break;

                case TgcMesh.MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:
                    TgcSceneLoader.DiffuseMapAndLightmapVertex[] verts2 = (TgcSceneLoader.DiffuseMapAndLightmapVertex[])mesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapAndLightmapVertex), LockFlags.ReadOnly, mesh.D3dMesh.NumberVertices);
                    for (int i = 0; i < verts2.Length; i++)
                    {
                        verts2[i].Tu0 = uvOffset.X + originalUVCoords[i].X * uvTile.X;
                        verts2[i].Tv0 = uvOffset.Y + originalUVCoords[i].Y * uvTile.Y;
                    }
                    mesh.D3dMesh.SetVertexBufferData(verts2, LockFlags.None);
                    mesh.D3dMesh.UnlockVertexBuffer();
                    break;
            }
        }

        public override void updateBoundingBox()
        {
            Vector3[] vertices = this.mesh.getVertexPositions();
            Matrix rotM = Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z);
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = TgcVectorUtils.transform(vertices[i], rotM);
            }
            this.mesh.BoundingBox = TgcBoundingBox.computeFromPoints(vertices);
            //this.mesh.updateBoundingBox();
        }

    }
}
