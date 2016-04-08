using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Core.Utils;

namespace TGC.Examples.MeshCreator.EditablePoly
{
    /// <summary>
    ///     Herramienta para acumular varios triangulos y dibujarlos luego todos juntos
    /// </summary>
    public class TrianglesBatchRenderer
    {
        private const int BATCH_SIZE = 1200;
        private readonly Vector3 BOX_LINE_ORIGINAL_DIR = new Vector3(0, 1, 0);
        private readonly CustomVertex.PositionColored[] vertices;
        private int idx;

        public TrianglesBatchRenderer()
        {
            vertices = new CustomVertex.PositionColored[BATCH_SIZE];
            reset();
        }

        /// <summary>
        ///     Reiniciar batch
        /// </summary>
        public void reset()
        {
            idx = 0;
        }

        /// <summary>
        ///     Agregar triangulo al batch
        /// </summary>
        public void addTriangle(CustomVertex.PositionColored a, CustomVertex.PositionColored b,
            CustomVertex.PositionColored c)
        {
            vertices[idx] = a;
            vertices[idx + 1] = b;
            vertices[idx + 2] = c;
            idx += 3;
        }

        /// <summary>
        ///     Agregar triangulo al batch
        /// </summary>
        public void addTriangle(Vector3 a, Vector3 b, Vector3 c, Color color)
        {
            var cInt = color.ToArgb();
            addTriangle(
                new CustomVertex.PositionColored(a, cInt),
                new CustomVertex.PositionColored(b, cInt),
                new CustomVertex.PositionColored(c, cInt));
        }

        /// <summary>
        ///     Indica si hay espacio suficiente para agregar la cantidad de vertices deseada
        /// </summary>
        public bool hasSpace(int vertexCount)
        {
            return idx + vertexCount < BATCH_SIZE;
        }

        /// <summary>
        ///     Dibujar batch de triangulos hasta donde se haya cargado
        /// </summary>
        public void render()
        {
            TexturesManager.Instance.clear(0);
            TexturesManager.Instance.clear(1);

            var effect = TgcShaders.Instance.VariosShader;
            TgcShaders.Instance.setShaderMatrixIdentity(effect);
            D3DDevice.Instance.Device.VertexDeclaration = TgcShaders.Instance.VdecPositionColored;
            effect.Technique = TgcShaders.T_POSITION_COLORED;

            //Alpha blend on
            D3DDevice.Instance.Device.RenderState.AlphaTestEnable = true;
            D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = true;

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.TriangleList, idx / 3, vertices);
            effect.EndPass();
            effect.End();

            //Alpha blend off
            D3DDevice.Instance.Device.RenderState.AlphaTestEnable = false;
            D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = false;
        }

        /// <summary>
        ///     Si no queda lugar para agregar los vertices que se quiere entonces se dibuja y vacia el buffer
        /// </summary>
        public void checkAndFlush(int vertexCount)
        {
            if (!hasSpace(vertexCount))
            {
                render();
                reset();
            }
        }

        /// <summary>
        ///     Add new BoxLine mesh
        /// </summary>
        public void addBoxLine(Vector3 pStart, Vector3 pEnd, float thickness, Color color)
        {
            const int vertexCount = 36;
            checkAndFlush(vertexCount);
            var initIdx = idx;

            var c = color.ToArgb();

            //Crear caja en vertical en Y con longitud igual al módulo de la recta.
            var lineVec = Vector3.Subtract(pEnd, pStart);
            var lineLength = lineVec.Length();
            var min = new Vector3(-thickness, 0, -thickness);
            var max = new Vector3(thickness, lineLength, thickness);

            //Vértices de la caja con forma de linea
            // Front face
            addTriangle(
                new CustomVertex.PositionColored(min.X, max.Y, max.Z, c),
                new CustomVertex.PositionColored(min.X, min.Y, max.Z, c),
                new CustomVertex.PositionColored(max.X, max.Y, max.Z, c)
                );
            addTriangle(
                new CustomVertex.PositionColored(min.X, min.Y, max.Z, c),
                new CustomVertex.PositionColored(max.X, min.Y, max.Z, c),
                new CustomVertex.PositionColored(max.X, max.Y, max.Z, c)
                );

            // Back face (remember this is facing *away* from the camera, so vertices should be clockwise order)
            addTriangle(
                new CustomVertex.PositionColored(min.X, max.Y, min.Z, c),
                new CustomVertex.PositionColored(max.X, max.Y, min.Z, c),
                new CustomVertex.PositionColored(min.X, min.Y, min.Z, c)
                );
            addTriangle(
                new CustomVertex.PositionColored(min.X, min.Y, min.Z, c),
                new CustomVertex.PositionColored(max.X, max.Y, min.Z, c),
                new CustomVertex.PositionColored(max.X, min.Y, min.Z, c)
                );

            // Top face
            addTriangle(
                new CustomVertex.PositionColored(min.X, max.Y, max.Z, c),
                new CustomVertex.PositionColored(max.X, max.Y, min.Z, c),
                new CustomVertex.PositionColored(min.X, max.Y, min.Z, c)
                );
            addTriangle(
                new CustomVertex.PositionColored(min.X, max.Y, max.Z, c),
                new CustomVertex.PositionColored(max.X, max.Y, max.Z, c),
                new CustomVertex.PositionColored(max.X, max.Y, min.Z, c)
                );

            // Bottom face (remember this is facing *away* from the camera, so vertices should be clockwise order)
            addTriangle(
                new CustomVertex.PositionColored(min.X, min.Y, max.Z, c),
                new CustomVertex.PositionColored(min.X, min.Y, min.Z, c),
                new CustomVertex.PositionColored(max.X, min.Y, min.Z, c)
                );
            addTriangle(
                new CustomVertex.PositionColored(min.X, min.Y, max.Z, c),
                new CustomVertex.PositionColored(max.X, min.Y, min.Z, c),
                new CustomVertex.PositionColored(max.X, min.Y, max.Z, c)
                );

            // Left face
            addTriangle(
                new CustomVertex.PositionColored(min.X, max.Y, max.Z, c),
                new CustomVertex.PositionColored(min.X, min.Y, min.Z, c),
                new CustomVertex.PositionColored(min.X, min.Y, max.Z, c)
                );
            addTriangle(
                new CustomVertex.PositionColored(min.X, max.Y, min.Z, c),
                new CustomVertex.PositionColored(min.X, min.Y, min.Z, c),
                new CustomVertex.PositionColored(min.X, max.Y, max.Z, c)
                );

            // Right face (remember this is facing *away* from the camera, so vertices should be clockwise order)
            addTriangle(
                new CustomVertex.PositionColored(max.X, max.Y, max.Z, c),
                new CustomVertex.PositionColored(max.X, min.Y, max.Z, c),
                new CustomVertex.PositionColored(max.X, min.Y, min.Z, c)
                );
            addTriangle(
                new CustomVertex.PositionColored(max.X, max.Y, min.Z, c),
                new CustomVertex.PositionColored(max.X, max.Y, max.Z, c),
                new CustomVertex.PositionColored(max.X, min.Y, min.Z, c)
                );

            //Obtener matriz de rotacion respecto del vector de la linea
            lineVec.Normalize();
            var angle = FastMath.Acos(Vector3.Dot(BOX_LINE_ORIGINAL_DIR, lineVec));
            var axisRotation = Vector3.Cross(BOX_LINE_ORIGINAL_DIR, lineVec);
            axisRotation.Normalize();
            var t = Matrix.RotationAxis(axisRotation, angle) * Matrix.Translation(pStart);

            //Transformar todos los puntos
            for (var i = initIdx; i < initIdx + vertexCount; i++)
            {
                vertices[i].Position = Vector3.TransformCoordinate(vertices[i].Position, t);
            }
        }

        public void dispose()
        {
        }
    }
}