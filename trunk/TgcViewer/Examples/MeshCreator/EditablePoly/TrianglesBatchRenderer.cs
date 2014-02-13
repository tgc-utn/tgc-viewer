using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.MeshCreator.EditablePolyTools
{
    /// <summary>
    /// Herramienta para acumular varios triangulos y dibujarlos luego todos juntos
    /// </summary>
    public class TrianglesBatchRenderer
    {
        readonly Vector3 BOX_LINE_ORIGINAL_DIR = new Vector3(0, 1, 0);

        const int BATCH_SIZE = 1200;
        readonly CustomVertex.PositionColored[] vertices;
        int idx;

        public TrianglesBatchRenderer()
        {
            vertices = new CustomVertex.PositionColored[BATCH_SIZE];
            reset();
        }

        /// <summary>
        /// Reiniciar batch
        /// </summary>
        public void reset()
        {
            idx = 0;
        }

        /// <summary>
        /// Agregar triangulo al batch
        /// </summary>
        public void addTriangle(CustomVertex.PositionColored a, CustomVertex.PositionColored b, CustomVertex.PositionColored c)
        {
            vertices[idx] = a;
            vertices[idx + 1] = b;
            vertices[idx + 2] = c;
            idx += 3;
        }

        /// <summary>
        /// Agregar triangulo al batch
        /// </summary>
        public void addTriangle(Vector3 a, Vector3 b, Vector3 c, Color color)
        {
            int cInt = color.ToArgb();
            addTriangle(
                new CustomVertex.PositionColored(a, cInt), 
                new CustomVertex.PositionColored(b, cInt), 
                new CustomVertex.PositionColored(c, cInt));
        }


        /// <summary>
        /// Indica si hay espacio suficiente para agregar la cantidad de vertices deseada
        /// </summary>
        public bool hasSpace(int vertexCount)
        {
            return idx + vertexCount < BATCH_SIZE;
        }

        /// <summary>
        /// Dibujar batch de triangulos hasta donde se haya cargado
        /// </summary>
        public void render()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            texturesManager.clear(0);
            texturesManager.clear(1);

            Effect effect = GuiController.Instance.Shaders.VariosShader;
            GuiController.Instance.Shaders.setShaderMatrixIdentity(effect);
            d3dDevice.VertexDeclaration = GuiController.Instance.Shaders.VdecPositionColored;
            effect.Technique = TgcShaders.T_POSITION_COLORED;

            //Alpha blend on
            d3dDevice.RenderState.AlphaTestEnable = true;
            d3dDevice.RenderState.AlphaBlendEnable = true;

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            d3dDevice.DrawUserPrimitives(PrimitiveType.TriangleList, idx / 3, vertices);
            effect.EndPass();
            effect.End();

            //Alpha blend off
            d3dDevice.RenderState.AlphaTestEnable = false;
            d3dDevice.RenderState.AlphaBlendEnable = false;
        }

        /// <summary>
        /// Si no queda lugar para agregar los vertices que se quiere entonces se dibuja y vacia el buffer
        /// </summary>
        public void checkAndFlush(int vertexCount)
        {
            if(!hasSpace(vertexCount))
            {
                render();
                reset();
            }
        }

        /// <summary>
        /// Add new BoxLine mesh
        /// </summary>
        public void addBoxLine(Vector3 pStart, Vector3 pEnd, float thickness, Color color)
        {
            const int vertexCount = 36;
            checkAndFlush(vertexCount);
            int initIdx = this.idx;

            int c = color.ToArgb();

            //Crear caja en vertical en Y con longitud igual al módulo de la recta.
            Vector3 lineVec = Vector3.Subtract(pEnd, pStart);
            float lineLength = lineVec.Length();
            Vector3 min = new Vector3(-thickness, 0, -thickness);
            Vector3 max = new Vector3(thickness, lineLength, thickness);

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
            float angle = FastMath.Acos(Vector3.Dot(BOX_LINE_ORIGINAL_DIR, lineVec));
            Vector3 axisRotation = Vector3.Cross(BOX_LINE_ORIGINAL_DIR, lineVec);
            axisRotation.Normalize();
            Matrix t = Matrix.RotationAxis(axisRotation, angle) * Matrix.Translation(pStart);

            //Transformar todos los puntos
            for (int i = initIdx; i < initIdx + vertexCount; i++)
            {
                vertices[i].Position = Vector3.TransformCoordinate(vertices[i].Position, t);
            }

        }



        public void dispose()
        {

        }
    }
}
