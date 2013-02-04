using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils.TgcGeometry;
using System.IO;

namespace Examples.Quake3Loader
{
    /// <summary>
    /// Herramienta para renderizar un escenario BSP de Quake 3
    /// 
    /// Autor: Martin Giachetti
    /// 
    /// </summary>
    public class BspMap
    {

        private List<TgcMesh> meshes;
        /// <summary>
        /// Meshes de TGC que conforman el escenario
        /// </summary>
        public List<TgcMesh> Meshes
        {
            get { return meshes; }
        }

        private BspMapData data;
        /// <summary>
        /// BSP Data
        /// </summary>
        public BspMapData Data
        {
            get { return data; }
        }

        private BspCollisionManager collisionManager;
        /// <summary>
        /// Utilidad para colisiones en el escenario BSP
        /// </summary>
        public BspCollisionManager CollisionManager
        {
            get { return collisionManager; }
        }


        //Variables para visibilidad de clusters
        private int antCluster;
        private List<int> clusterVis;

        

        public BspMap()
        {
            meshes = new List<TgcMesh>();
            data = new BspMapData();
            collisionManager = new BspCollisionManager(this);

            antCluster = -2;
            clusterVis = new List<int>();
        }

        /// <summary>
        /// Configigurar visibilidad inicial de meshes
        /// </summary>
        public void initVisibility()
        {
            //Deshabilitar todas
            foreach (TgcMesh mesh in meshes)
            {
                if (mesh != null)
                {
                    mesh.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Renderizar escenario BSP utilizando matriz PVS para descartar clusters no visibles
        /// </summary>
        /// <param name="camPos">Posición actual de la camara</param>
        public void render(Vector3 camPos)
        {
            Device device = GuiController.Instance.D3dDevice;

            //Obtener leaf actual
            int actualLeaf = FindLeaf(camPos);

            //Obtener clusters visibles segun PVS
            int actualCluster = data.leafs[actualLeaf].cluster;
            if (actualCluster != antCluster)
            {
                antCluster = actualCluster;
                clusterVis.Clear();
                for (int i = 0; i < data.leafs.Length; i++)
                {
                    if (isClusterVisible(actualCluster, data.leafs[i].cluster))
                        clusterVis.Add(i);
                }
            }

            //Actualizar volumen del Frustum con nuevos valores de camara
            TgcFrustum frustum = GuiController.Instance.Frustum;
            frustum.updateVolume(device.Transform.View, device.Transform.Projection);

            foreach (int nleaf in clusterVis)
            {
                //Frustum Culling con el AABB del cluster
                TgcCollisionUtils.FrustumResult result = TgcCollisionUtils.classifyFrustumAABB(frustum, data.leafs[nleaf].boundingBox);
                if (result == TgcCollisionUtils.FrustumResult.OUTSIDE)
                    continue;

                //Habilitar meshes de este leaf
                QLeaf currentLeaf = data.leafs[nleaf];
                for (int i = currentLeaf.firstLeafSurface; i < currentLeaf.firstLeafSurface + currentLeaf.numLeafSurfaces; i++)
                {
                    int iMesh = data.leafSurfaces[i];
                    TgcMesh mesh = meshes[iMesh];
                    if (mesh != null)
                    {
                        meshes[iMesh].Enabled = true;
                    }
                }
            }

            //Renderizar meshes visibles
            Matrix mViewProj = device.Transform.View * device.Transform.Projection;
            for (int i = 0; i < meshes.Count; i++)
            {
                //Ignonar si no está habilitada
                TgcMesh mesh = meshes[i];
                if (mesh == null || !mesh.Enabled)
                {
                    continue;
                }
                   

                QShaderData shader = data.shaderXSurface[i];
                
                //Renderizado con shaders
                //TODO: Mejorar el renderizado de shaders. Por ahora esta deshabilitado
                if (false /*shader != null && shader.Stages.Count > 0*/)
                {
                    renderShaderMesh(mesh, shader);
                }
                //Renderizado normal
                else
                {
                    //render
                    mesh.render();

                    //deshabilitar para la proxima vuelta
                    mesh.Enabled = false;
                }
            }
        }



        //private bool first = true;

        /// <summary>
        /// Ejecuta el shader original de formato propio de Quake 3 convertido a una version similar de HLSL en DirectX.
        /// Estado experimental. Desabilitado por el momento.
        /// </summary>
        private void renderShaderMesh(TgcMesh mesh, QShaderData shader)
        {
            /*
            device.RenderState.AlphaFunction = Compare.Greater;
                    device.RenderState.BlendOperation = BlendOperation.Add;
                    device.RenderState.AlphaBlendEnable = true;
                    device.RenderState.AlphaTestEnable = true;
                    device.RenderState.SourceBlend = Blend.SourceAlpha;
                    device.RenderState.DestinationBlend = Blend.InvSourceAlpha;

            Effect fx = shader.Fx;
            if (first)
            {
                File.WriteAllText("shad" + shader.Name.Replace('/', '-') + ".txt", shader.ShaderSrc);
            }
            fx.Technique = "tec0";
            fx.SetValue("g_mWorld", Matrix.Identity);
            fx.SetValue("g_mViewProj", mViewProj);
            fx.SetValue("g_time", time);

            fx.Begin(FX.None);

            for (int j = 0; j < shader.Stages.Count; j++)
            {
                fx.BeginPass(j);

                mesh.executeRender();

                fx.EndPass();
            }

            fx.End();
            
            */
        }

        /// <summary>
        /// Encontrar la hoja actual del arbol BP
        /// </summary>
        private int FindLeaf(Vector3 pos)
        {
            int index = 0;

            while (index >= 0)
            {
                QNode node = data.nodes[index];
                QPlane plane = data.planes[node.planeNum];

                // Distance from point to a plane
                float distance = Vector3.Dot(plane.normal, pos) - plane.dist;

                if (distance >= 0)
                {
                    //front
                    index = node.children[0];
                }
                else
                {
                    //back
                    index = node.children[1];
                }
            }

            //~ => bitwise complement operation
            return ~index;
        }

        /// <summary>
        /// Indica si un cluster es visible en la matriz PVS
        /// </summary>
        private bool isClusterVisible(int visCluster, int testCluster)
        {
            if ((data.visData == null) || (visCluster < 0))
            {
                return true;
            }

            int i = (visCluster * data.visData.sizeVec) + (testCluster >> 3);
            byte visSet = data.visData.data[i];

            return (visSet & (1 << (testCluster & 7))) != 0;
        }




        /// <summary>
        /// Liberar recursos
        /// </summary>
        public void dispose()
        {
            foreach (TgcMesh mesh in meshes)
            {
                if (mesh != null)
                {
                    mesh.dispose();
                }
            }
            meshes = null;
            foreach (QShaderData shader in data.shaderXSurface)
            {
                if (shader != null && shader.Fx != null)
                {
                    shader.Fx.Dispose();
                }
            }
            data = null;
        }
    }
}
