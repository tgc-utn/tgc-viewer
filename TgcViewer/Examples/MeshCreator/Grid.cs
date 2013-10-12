using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TgcViewer;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils;

namespace Examples.MeshCreator
{
    /// <summary>
    /// Grilla del piso del escenario
    /// </summary>
    public class Grid
    {

        const float GRID_RADIUS = 100f;
        const float LINE_SEPARATION = 20f;

        TgcBoundingBox boundingBox;
        /// <summary>
        /// BoundingBox
        /// </summary>
        public TgcBoundingBox BoundingBox
        {
            get { return boundingBox; }
        }


        MeshCreatorControl control;
        CustomVertex.PositionColored[] vertices;


        public Grid(MeshCreatorControl control)
        {
            this.control = control;

            boundingBox = new TgcBoundingBox(new Vector3(-100000, -0.1f, -100000), new Vector3(100000, 0, 100000));

            vertices = new CustomVertex.PositionColored[12 * 2 * 2];
            int color = Color.FromArgb(76, 76, 76).ToArgb();

            //10 lineas horizontales en X
            for (int i = 0; i < 11; i++)
            {
                vertices[i * 2] = new CustomVertex.PositionColored(-GRID_RADIUS, 0, -GRID_RADIUS + LINE_SEPARATION * i, color);
                vertices[i * 2 + 1] = new CustomVertex.PositionColored(GRID_RADIUS, 0, -GRID_RADIUS + LINE_SEPARATION * i, color);
            }

            //10 lineas horizontales en Z
            for (int i = 11; i < 22; i++)
            {
                vertices[i * 2] = new CustomVertex.PositionColored(-GRID_RADIUS * 3 + LINE_SEPARATION * i - LINE_SEPARATION, 0, -GRID_RADIUS, color);
                vertices[i * 2 + 1] = new CustomVertex.PositionColored(-GRID_RADIUS * 3 + LINE_SEPARATION * i - LINE_SEPARATION, 0, GRID_RADIUS, color);
            }
        }

        public void render()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            texturesManager.clear(0);
            texturesManager.clear(1);
            d3dDevice.Material = TgcD3dDevice.DEFAULT_MATERIAL;
            d3dDevice.Transform.World = Matrix.Identity;

            d3dDevice.VertexFormat = CustomVertex.PositionColored.Format;
            d3dDevice.DrawUserPrimitives(PrimitiveType.LineList, 22, vertices);
        }

        public void dispose()
        {
            boundingBox.dispose();
            vertices = null;
        }

        /// <summary>
        /// Picking al grid
        /// </summary>
        public Vector3 getPicking()
        {
            control.PickingRay.updateRay();
            Vector3 collisionPoint;
            if (TgcCollisionUtils.intersectRayAABB(control.PickingRay.Ray, this.boundingBox, out collisionPoint))
            {
                return collisionPoint;
            }

            return new Vector3(0, 0, 0);
            //throw new Exception("Sin colision con Grid");
        }

    }
}
