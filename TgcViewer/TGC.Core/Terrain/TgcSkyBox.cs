using System;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;

namespace TGC.Core.Terrain
{
    /// <summary>
    ///     Herramienta para crear un SkyBox conformado por un cubo de 6 caras, cada cada con su
    ///     propia textura.
    ///     Luego de creado, si se modifica cualquier valor del mismo, se debe llamar a updateValues()
    ///     para que tome efecto.
    /// </summary>
    public class TgcSkyBox : IRenderObject
    {
        /// <summary>
        ///     Caras del SkyBox
        /// </summary>
        public enum SkyFaces
        {
            Up = 0,
            Down = 1,
            Front = 2,
            Back = 3,
            Right = 4,
            Left = 5
        }

        private bool alphaBlendEnable;

        private Color color;

        /// <summary>
        ///     Crear un SkyBox vacio
        /// </summary>
        public TgcSkyBox()
        {
            Faces = new TgcMesh[6];
            FaceTextures = new string[6];
            SkyEpsilon = 5f;
            color = Color.White;
            Center = new Vector3(0, 0, 0);
            Size = new Vector3(1000, 1000, 1000);
            alphaBlendEnable = false;
        }

        /// <summary>
        ///     Valor de desplazamiento utilizado para que las caras del SkyBox encajen bien entre sí.
        ///     Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public float SkyEpsilon { get; set; }

        /// <summary>
        ///     Tamaño del SkyBox.
        ///     Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public Vector3 Size { get; set; }

        /// <summary>
        ///     Centro del SkyBox.
        ///     Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public Vector3 Center { get; set; }

        /// <summary>
        ///     Color del SkyBox.
        ///     Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        ///     Meshes de cada una de las 6 caras del cubo, en el orden en que se enumeran en SkyFaces
        /// </summary>
        public TgcMesh[] Faces { get; }

        /// <summary>
        ///     Path de las texturas de cada una de las 6 caras del cubo, en el orden en que se enumeran en SkyFaces
        /// </summary>
        public string[] FaceTextures { get; }

        /// <summary>
        ///     Habilita el renderizado con AlphaBlending para los modelos
        ///     con textura o colores por vértice de canal Alpha.
        ///     Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable
        {
            get { return Faces[0].AlphaBlendEnable; }
            set
            {
                foreach (var face in Faces)
                {
                    face.AlphaBlendEnable = true;
                }
            }
        }

        /// <summary>
        ///     Renderizar SkyBox
        /// </summary>
        public void render()
        {
            foreach (var face in Faces)
            {
                face.render();
            }
        }

        /// <summary>
        ///     Liberar recursos del SkyBox
        /// </summary>
        public void dispose()
        {
            foreach (var face in Faces)
            {
                face.dispose();
            }
        }

        /// <summary>
        ///     Configurar la textura de una cara del SkyBox
        /// </summary>
        /// <param name="face">Cara del SkyBox</param>
        /// <param name="texturePath">Path de la textura</param>
        public void setFaceTexture(SkyFaces face, string texturePath)
        {
            FaceTextures[(int)face] = texturePath;
        }

        /// <summary>
        ///     Tomar los valores configurados y crear el SkyBox
        /// </summary>
        public void updateValues()
        {
            //Crear cada cara
            for (var i = 0; i < Faces.Length; i++)
            {
                //Crear mesh de D3D
                var m = new Mesh(2, 4, MeshFlags.Managed, Core.SceneLoader.TgcSceneLoader.DiffuseMapVertexElements,
                    D3DDevice.Instance.Device);
                var skyFace = (SkyFaces)i;

                // Cargo los vértices
                using (var vb = m.VertexBuffer)
                {
                    var data = vb.Lock(0, 0, LockFlags.None);
                    var colorRgb = color.ToArgb();
                    cargarVertices(skyFace, data, colorRgb);
                    vb.Unlock();
                }

                // Cargo los índices
                using (var ib = m.IndexBuffer)
                {
                    var ibArray = new short[6];
                    cargarIndices(ibArray);
                    ib.SetData(ibArray, 0, LockFlags.None);
                }

                //Crear TgcMesh
                var faceName = Enum.GetName(typeof(SkyFaces), skyFace);
                var faceMesh = new TgcMesh(m, "SkyBox-" + faceName, TgcMesh.MeshRenderType.DIFFUSE_MAP);
                faceMesh.Materials = new[] { D3DDevice.DEFAULT_MATERIAL };
                faceMesh.createBoundingBox();
                faceMesh.Enabled = true;

                //textura
                var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, FaceTextures[i]);
                faceMesh.DiffuseMaps = new[] { texture };

                Faces[i] = faceMesh;
            }
        }

        /// <summary>
        ///     Crear Vertices segun la cara pedida
        /// </summary>
        private void cargarVertices(SkyFaces face, GraphicsStream data, int color)
        {
            switch (face)
            {
                case SkyFaces.Up:
                    cargarVerticesUp(data, color);
                    break;

                case SkyFaces.Down:
                    cargarVerticesDown(data, color);
                    break;

                case SkyFaces.Front:
                    cargarVerticesFront(data, color);
                    break;

                case SkyFaces.Back:
                    cargarVerticesBack(data, color);
                    break;

                case SkyFaces.Right:
                    cargarVerticesRight(data, color);
                    break;

                case SkyFaces.Left:
                    cargarVerticesLeft(data, color);
                    break;
            }
        }

        /// <summary>
        ///     Crear vertices para la cara Up
        /// </summary>
        private void cargarVerticesUp(GraphicsStream data, int color)
        {
            Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex v;
            var n = new Vector3(0, 1, 0);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X - Size.X / 2 - SkyEpsilon,
                Center.Y + Size.Y / 2,
                Center.Z - Size.Z / 2 - SkyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 0;
            data.Write(v);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X - Size.X / 2 - SkyEpsilon,
                Center.Y + Size.Y / 2,
                Center.Z + Size.Z / 2 + SkyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 0;
            data.Write(v);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X + Size.X / 2 + SkyEpsilon,
                Center.Y + Size.Y / 2,
                Center.Z + Size.Z / 2 + SkyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 1;
            data.Write(v);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X + Size.X / 2 + SkyEpsilon,
                Center.Y + Size.Y / 2,
                Center.Z - Size.Z / 2 - SkyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 1;
            data.Write(v);
        }

        /// <summary>
        ///     Crear vertices para la cara Down
        /// </summary>
        private void cargarVerticesDown(GraphicsStream data, int color)
        {
            Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex v;
            var n = new Vector3(0, -1, 0);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X - Size.X / 2 - SkyEpsilon,
                Center.Y - Size.Y / 2,
                Center.Z + Size.Z / 2 + SkyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 1;
            data.Write(v);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X - Size.X / 2 - SkyEpsilon,
                Center.Y - Size.Y / 2,
                Center.Z - Size.Z / 2 - SkyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 1;
            data.Write(v);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X + Size.X / 2 + SkyEpsilon,
                Center.Y - Size.Y / 2,
                Center.Z - Size.Z / 2 - SkyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 0;
            data.Write(v);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X + Size.X / 2 + SkyEpsilon,
                Center.Y - Size.Y / 2,
                Center.Z + Size.Z / 2 + SkyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 0;
            data.Write(v);
        }

        /// <summary>
        ///     Crear vertices para la cara Front
        /// </summary>
        private void cargarVerticesFront(GraphicsStream data, int color)
        {
            Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex v;
            var n = new Vector3(0, -1, 0);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X - Size.X / 2 - SkyEpsilon,
                Center.Y + Size.Y / 2 + SkyEpsilon,
                Center.Z + Size.Z / 2
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 0;
            data.Write(v);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X - Size.X / 2 - SkyEpsilon,
                Center.Y - Size.Y / 2 - SkyEpsilon,
                Center.Z + Size.Z / 2
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 1;
            data.Write(v);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X + Size.X / 2 + SkyEpsilon,
                Center.Y - Size.Y / 2 - SkyEpsilon,
                Center.Z + Size.Z / 2
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 1;
            data.Write(v);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X + Size.X / 2 + SkyEpsilon,
                Center.Y + Size.Y / 2 + SkyEpsilon,
                Center.Z + Size.Z / 2
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 0;
            data.Write(v);
        }

        /// <summary>
        ///     Crear vertices para la cara Back
        /// </summary>
        private void cargarVerticesBack(GraphicsStream data, int color)
        {
            Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex v;
            var n = new Vector3(0, -1, 0);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X + Size.X / 2 + SkyEpsilon,
                Center.Y + Size.Y / 2 + SkyEpsilon,
                Center.Z - Size.Z / 2
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 0;
            data.Write(v);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X + Size.X / 2 + SkyEpsilon,
                Center.Y - Size.Y / 2 - SkyEpsilon,
                Center.Z - Size.Z / 2
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 1;
            data.Write(v);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X - Size.X / 2 - SkyEpsilon,
                Center.Y - Size.Y / 2 - SkyEpsilon,
                Center.Z - Size.Z / 2
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 1;
            data.Write(v);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X - Size.X / 2 - SkyEpsilon,
                Center.Y + Size.Y / 2 + SkyEpsilon,
                Center.Z - Size.Z / 2
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 0;
            data.Write(v);
        }

        /// <summary>
        ///     Crear vertices para la cara Right
        /// </summary>
        private void cargarVerticesRight(GraphicsStream data, int color)
        {
            Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex v;
            var n = new Vector3(0, -1, 0);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X + Size.X / 2,
                Center.Y + Size.Y / 2 + SkyEpsilon,
                Center.Z + Size.Z / 2 + SkyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 0;
            data.Write(v);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X + Size.X / 2,
                Center.Y - Size.Y / 2 - SkyEpsilon,
                Center.Z + Size.Z / 2 + SkyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 1;
            data.Write(v);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X + Size.X / 2,
                Center.Y - Size.Y / 2 - SkyEpsilon,
                Center.Z - Size.Z / 2 - SkyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 1;
            data.Write(v);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X + Size.X / 2,
                Center.Y + Size.Y / 2 + SkyEpsilon,
                Center.Z - Size.Z / 2 - SkyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 0;
            data.Write(v);
        }

        /// <summary>
        ///     Crear vertices para la cara Left
        /// </summary>
        private void cargarVerticesLeft(GraphicsStream data, int color)
        {
            Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex v;
            var n = new Vector3(0, -1, 0);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X - Size.X / 2,
                Center.Y + Size.Y / 2 + SkyEpsilon,
                Center.Z - Size.Z / 2 - SkyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 0;
            data.Write(v);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X - Size.X / 2,
                Center.Y - Size.Y / 2 - SkyEpsilon,
                Center.Z - Size.Z / 2 - SkyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 1;
            data.Write(v);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X - Size.X / 2,
                Center.Y - Size.Y / 2 - SkyEpsilon,
                Center.Z + Size.Z / 2 + SkyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 1;
            data.Write(v);

            v = new Core.SceneLoader.TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                Center.X - Size.X / 2,
                Center.Y + Size.Y / 2 + SkyEpsilon,
                Center.Z + Size.Z / 2 + SkyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 0;
            data.Write(v);
        }

        /// <summary>
        ///     Generar array de indices
        /// </summary>
        private void cargarIndices(short[] ibArray)
        {
            var i = 0;
            ibArray[i++] = 0;
            ibArray[i++] = 1;
            ibArray[i++] = 2;
            ibArray[i++] = 0;
            ibArray[i++] = 2;
            ibArray[i++] = 3;
        }
    }
}