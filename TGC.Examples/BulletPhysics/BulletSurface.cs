using Microsoft.DirectX.Direct3D;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Examples.Bullet.Physics;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.Bullet
{
    public class BulletSurface : TGCExampleViewer
    {
        //Fisica
        private TrianglePhysics physicsExample;

        //Vertex buffer que se va a utilizar
        private VertexBuffer vertexBuffer;

        private int totalVertices;
        private Effect effect;
        private string technique;
        private Texture terrainTexture;

        public BulletSurface(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "BulletPhysics";
            Name = "Triangles + Regular Shapes vs Capsule";
            Description = "Ejemplo de como poder utilizar el motor de fisica Bullet con \"BulletSharp + TGC.Core\". Donde se emplea una capsula para el personaje, un terreno generado matematicamente con muchos triangulos y elementos varios para colisionar.";
        }

        public override void Init()
        {
            //Ideas para generar el terreno para Bullet
            //We are getting a llitle bit crazy xD https://es.wikipedia.org/wiki/Paraboloide
            //Paraboloide Hiperbolico
            // definicion matematica
            //(x / a) ^ 2 - ( y / b) ^ 2 - z = 0.
            //
            //DirectX
            //(x / a) ^ 2 - ( z / b) ^ 2 - y = 0.

            //Paraboloide Circular
            //definicion matematica
            //(x / a) ^ 2 + ( y / b) ^ 2 - z = 0 ; a=b.
            //
            //DirectX
            //(x / a) ^ 2 + ( z / a) ^ 2 - y = 0.

            //Crear vertexBuffer
            int width = 1200;
            int length = 1200;
            totalVertices = 2 * 3 * (width - 1) * (length - 1);
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionTextured), totalVertices, D3DDevice.Instance.Device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);
            //Almacenar informacion en VertexBuffer

            //Cargar vertices
            var dataIdx = 0;
            var data = new CustomVertex.PositionTextured[totalVertices];

            TGCVector3 center = TGCVector3.Empty;

            center.X = center.X - width / 2;
            center.Z = center.Z - length / 2;

            int a = 64;
            int size = 80;
            int n = 0;
            int triangles = 0;
            int vertexes = 0;
            for (var i = 0; i < width - 1; i = i + size)
            {
                for (var j = 0; j < length - 1; j = j + size)
                {
                    //Vertices
                    var v1 = new TGCVector3(center.X + i, center.Y + (FastMath.Pow2((center.X + i) / a) + FastMath.Pow2((center.Z + j) / a)), center.Z + j);
                    var v2 = new TGCVector3(center.X + i, center.Y + (FastMath.Pow2((center.X + i) / a) + FastMath.Pow2((center.Z + j + size) / a)), center.Z + (j + size));
                    var v3 = new TGCVector3(center.X + (i + size), center.Y + (FastMath.Pow2((center.X + i + size) / a) + FastMath.Pow2((center.Z + j) / a)), center.Z + j);
                    var v4 = new TGCVector3(center.X + (i + size), center.Y + (FastMath.Pow2((center.X + i + size) / a) + FastMath.Pow2((center.Z + j + size) / a)), center.Z + (j + size));
                    vertexes = +vertexes + 4;

                    //Coordendas de textura
                    var t1 = new TGCVector2(0, 0);
                    var t2 = new TGCVector2(0, 1);
                    var t3 = new TGCVector2(1, 0);
                    var t4 = new TGCVector2(1, 1);

                    //Cargar triangulo 1
                    data[dataIdx] = new CustomVertex.PositionTextured(v1, t1.X, t1.Y);
                    data[dataIdx + 1] = new CustomVertex.PositionTextured(v2, t2.X, t2.Y);
                    data[dataIdx + 2] = new CustomVertex.PositionTextured(v4, t4.X, t4.Y);
                    triangles++;

                    //Cargar triangulo 2
                    data[dataIdx + 3] = new CustomVertex.PositionTextured(v1, t1.X, t1.Y);
                    data[dataIdx + 4] = new CustomVertex.PositionTextured(v4, t4.X, t4.Y);
                    data[dataIdx + 5] = new CustomVertex.PositionTextured(v3, t3.X, t3.Y);
                    triangles++;

                    dataIdx += 6;
                    n++;
                }
            }

            vertexBuffer.SetData(data, 0, LockFlags.None);

            //Rotar e invertir textura
            var b = (Bitmap)Image.FromFile(MediaDir + "//Texturas//pasto.jpg");
            b.RotateFlip(RotateFlipType.Rotate90FlipX);
            terrainTexture = Texture.FromBitmap(D3DDevice.Instance.Device, b, Usage.AutoGenerateMipMap, Pool.Managed);

            //Shader
            effect = TgcShaders.Instance.VariosShader;
            technique = TgcShaders.T_POSITION_TEXTURED;

            physicsExample = new TrianglePhysics();
            physicsExample.SetTriangleDataVB(data);
            physicsExample.Init(MediaDir);

            UserVars.addVar("Tgccito_Position");

            Camara = new TgcRotationalCamera(new TGCVector3(0, 20, 0), 1000, Input);
        }

        public override void Update()
        {
            PreUpdate();
            physicsExample.Update(Input);
            UserVars.setValue("Tgccito_Position", physicsExample.GetCharacterPosition());
            PostUpdate();
        }

        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            physicsExample.Render(ElapsedTime);

            //Textura
            effect.SetValue("texDiffuseMap", terrainTexture);
            TexturesManager.Instance.clear(1);

            TgcShaders.Instance.setShaderMatrix(effect, TGCMatrix.Identity);
            D3DDevice.Instance.Device.VertexDeclaration = TgcShaders.Instance.VdecPositionTextured;
            effect.Technique = technique;
            D3DDevice.Instance.Device.SetStreamSource(0, vertexBuffer, 0);

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, totalVertices / 3);
            effect.EndPass();
            effect.End();

            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }

        public override void Dispose()
        {
            physicsExample.Dispose();
            vertexBuffer.Dispose();
        }
    }
}