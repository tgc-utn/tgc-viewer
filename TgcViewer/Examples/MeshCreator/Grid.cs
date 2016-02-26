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
using TgcViewer.Utils.Shaders;
using TGC.Core.Utils;

namespace Examples.MeshCreator
{
    /// <summary>
    /// Grilla del piso del escenario
    /// </summary>
    public class Grid
    {

        const float BIG_VAL = 100000;
        const float SMALL_VAL = 0.1f;

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

        TgcBoundingBox pickingXZAabb;
        TgcBoundingBox pickingXYAabb;
        TgcBoundingBox pickingYZAabb;

        MeshCreatorControl control;
        CustomVertex.PositionColored[] vertices;


        public Grid(MeshCreatorControl control)
        {
            this.control = control;

            //El bounding box del piso es bien grande para hacer colisiones
            boundingBox = new TgcBoundingBox(new Vector3(-BIG_VAL, -SMALL_VAL, -BIG_VAL), new Vector3(BIG_VAL, 0, BIG_VAL));

            //Planos para colision de picking
            pickingXZAabb = new TgcBoundingBox(new Vector3(-BIG_VAL, -SMALL_VAL, -BIG_VAL), new Vector3(BIG_VAL, 0, BIG_VAL));
            pickingXYAabb = new TgcBoundingBox(new Vector3(-BIG_VAL, -BIG_VAL, -SMALL_VAL), new Vector3(BIG_VAL, BIG_VAL, 0));
            pickingYZAabb = new TgcBoundingBox(new Vector3(-SMALL_VAL, -BIG_VAL, -BIG_VAL), new Vector3(0, BIG_VAL, BIG_VAL));

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

            Effect effect = GuiController.Instance.Shaders.VariosShader;
            effect.Technique = TgcShaders.T_POSITION_COLORED;
            GuiController.Instance.Shaders.setShaderMatrixIdentity(effect);
            d3dDevice.VertexDeclaration = GuiController.Instance.Shaders.VdecPositionColored;

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            d3dDevice.DrawUserPrimitives(PrimitiveType.LineList, 22, vertices);
            effect.EndPass();
            effect.End();
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

        /// <summary>
        /// Picking con plano XZ ubicado en el centro del objeto
        /// </summary>
        public Vector3 getPickingXZ(TgcRay ray, Vector3 objCenter)
        {
            //Mover aabb en Y al centro del mesh
            pickingXZAabb.setExtremes(
                new Vector3(pickingXZAabb.PMin.X, objCenter.Y - SMALL_VAL, pickingXZAabb.PMin.Z),
                new Vector3(pickingXZAabb.PMax.X, objCenter.Y, pickingXZAabb.PMax.Z));
            Vector3 q;
            bool r = TgcCollisionUtils.intersectRayAABB(ray, pickingXZAabb, out q);
            if(r)
                return clampPickingResult(q);
            return objCenter;
        }

        /// <summary>
        /// Picking con plano XY ubicado en el centro del objeto
        /// </summary>
        public Vector3 getPickingXY(TgcRay ray, Vector3 objCenter)
        {
            //Mover aabb en Y al centro del mesh
            pickingXYAabb.setExtremes(
                new Vector3(pickingXYAabb.PMin.X, pickingXYAabb.PMin.Y, objCenter.Z - SMALL_VAL),
                new Vector3(pickingXYAabb.PMax.X, pickingXYAabb.PMax.Y, objCenter.Z));
            Vector3 q;
            bool r = TgcCollisionUtils.intersectRayAABB(ray, pickingXYAabb, out q);
            if (r)
                return clampPickingResult(q);
            return objCenter;
        }

        /// <summary>
        /// Picking con plano YZ ubicado en el centro del objeto
        /// </summary>
        public Vector3 getPickingYZ(TgcRay ray, Vector3 objCenter)
        {
            //Mover aabb en Y al centro del mesh
            pickingYZAabb.setExtremes(
                new Vector3(objCenter.X - SMALL_VAL, pickingYZAabb.PMin.Y, pickingYZAabb.PMin.Z),
                new Vector3(objCenter.X, pickingYZAabb.PMax.Y, pickingYZAabb.PMax.Z));
            Vector3 q;
            bool r = TgcCollisionUtils.intersectRayAABB(ray, pickingYZAabb, out q);
            if (r)
                return clampPickingResult(q);
            return objCenter;
        }

        /// <summary>
        /// Picking con los planos XZ e XY ubicados en el centro del objeto
        /// </summary>
        public Vector3 getPickingX(TgcRay ray, Vector3 objCenter)
        {
            //Mover ambos planos hacia el centro del objeto
            pickingXZAabb.setExtremes(
                new Vector3(pickingXZAabb.PMin.X, objCenter.Y - SMALL_VAL, pickingXZAabb.PMin.Z),
                new Vector3(pickingXZAabb.PMax.X, objCenter.Y, pickingXZAabb.PMax.Z));
            pickingXYAabb.setExtremes(
                new Vector3(pickingXYAabb.PMin.X, pickingXYAabb.PMin.Y, objCenter.Z - SMALL_VAL),
                new Vector3(pickingXYAabb.PMax.X, pickingXYAabb.PMax.Y, objCenter.Z));

            Vector3 q1, q2;
            bool r1, r2;
            r1 = TgcCollisionUtils.intersectRayAABB(ray, pickingXZAabb, out q1);
            r2 = TgcCollisionUtils.intersectRayAABB(ray, pickingXYAabb, out q2);

            if (r1 && r2)
            {
                Vector2 objPos = new Vector2(objCenter.Y, objCenter.Z);
                float diff1 = Vector2.Length(new Vector2(q1.Y, q1.Z) - objPos);
                float diff2 = Vector2.Length(new Vector2(q2.Y, q2.Z) - objPos);
                return diff1 < diff2 ? q1 : q2;
            }
            else if (r1)
                return clampPickingResult(q1);
            else if (r2)
                return clampPickingResult(q2);
            return objCenter;
        }

        /// <summary>
        /// Picking con los planos XY e YZ ubicados en el centro del objeto
        /// </summary>
        public Vector3 getPickingY(TgcRay ray, Vector3 objCenter)
        {
            //Mover ambos planos hacia el centro del objeto
            pickingXYAabb.setExtremes(
                new Vector3(pickingXYAabb.PMin.X, pickingXYAabb.PMin.Y, objCenter.Z - SMALL_VAL),
                new Vector3(pickingXYAabb.PMax.X, pickingXYAabb.PMax.Y, objCenter.Z));
            pickingYZAabb.setExtremes(
                new Vector3(objCenter.X - SMALL_VAL, pickingYZAabb.PMin.Y, pickingYZAabb.PMin.Z),
                new Vector3(objCenter.X, pickingYZAabb.PMax.Y, pickingYZAabb.PMax.Z));

            Vector3 q1, q2;
            bool r1, r2;
            r1 = TgcCollisionUtils.intersectRayAABB(ray, pickingXYAabb, out q1);
            r2 = TgcCollisionUtils.intersectRayAABB(ray, pickingYZAabb, out q2);

            if (r1 && r2)
            {
                Vector2 objPos = new Vector2(objCenter.X, objCenter.Z);
                float diff1 = Vector2.Length(new Vector2(q1.X, q1.Z) - objPos);
                float diff2 = Vector2.Length(new Vector2(q2.X, q2.Z) - objPos);
                return diff1 < diff2 ? q1 : q2;
            }
            else if (r1)
                return clampPickingResult(q1);
            else if (r2)
                return clampPickingResult(q2);
            return objCenter;
        }

        /// <summary>
        /// Picking con los planos XZ e YZ ubicados en el centro del objeto
        /// </summary>
        public Vector3 getPickingZ(TgcRay ray, Vector3 objCenter)
        {
            //Mover ambos planos hacia el centro del objeto
            pickingXZAabb.setExtremes(
                new Vector3(pickingXZAabb.PMin.X, objCenter.Y - SMALL_VAL, pickingXZAabb.PMin.Z),
                new Vector3(pickingXZAabb.PMax.X, objCenter.Y, pickingXZAabb.PMax.Z));
            pickingYZAabb.setExtremes(
                new Vector3(objCenter.X - SMALL_VAL, pickingYZAabb.PMin.Y, pickingYZAabb.PMin.Z),
                new Vector3(objCenter.X, pickingYZAabb.PMax.Y, pickingYZAabb.PMax.Z));

            Vector3 q1, q2;
            bool r1, r2;
            r1 = TgcCollisionUtils.intersectRayAABB(ray, pickingXZAabb, out q1);
            r2 = TgcCollisionUtils.intersectRayAABB(ray, pickingYZAabb, out q2);

            if (r1 && r2)
            {
                Vector2 objPos = new Vector2(objCenter.X, objCenter.Y);
                float diff1 = Vector2.Length(new Vector2(q1.X, q1.Y) - objPos);
                float diff2 = Vector2.Length(new Vector2(q2.X, q2.Y) - objPos);
                return diff1 < diff2 ? q1 : q2;
            }
            else if (r1)
                return clampPickingResult(q1);
            else if (r2)
                return clampPickingResult(q2);
            return objCenter;
        }

        private Vector3 clampPickingResult(Vector3 v)
        {
            v.X = FastMath.Clamp(v.X, -BIG_VAL, BIG_VAL);
            v.Y = FastMath.Clamp(v.Y, -BIG_VAL, BIG_VAL);
            v.Z = FastMath.Clamp(v.Z, -BIG_VAL, BIG_VAL);
            return v;
        }

    }
}

