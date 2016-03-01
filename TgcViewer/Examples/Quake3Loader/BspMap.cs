using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;
using TGC.Core.Direct3D;
using TGC.Viewer;
using TGC.Viewer.Utils.TgcGeometry;
using TGC.Viewer.Utils.TgcSceneLoader;

namespace TGC.Examples.Quake3Loader
{
    /// <summary>
    ///     Herramienta para renderizar un escenario BSP de Quake 3
    ///     Autor: Martin Giachetti
    /// </summary>
    public class BspMap
    {
        private readonly List<int> clusterVis;

        //Variables para visibilidad de clusters
        private int antCluster;

        private Matrix mViewProj;
        private float time;

        public BspMap()
        {
            Meshes = new List<TgcMesh>();
            Data = new BspMapData();
            CollisionManager = new BspCollisionManager(this);

            antCluster = -2;
            clusterVis = new List<int>();
        }

        /// <summary>
        ///     Meshes de TGC que conforman el escenario
        /// </summary>
        public List<TgcMesh> Meshes { get; private set; }

        /// <summary>
        ///     BSP Data
        /// </summary>
        public BspMapData Data { get; private set; }

        /// <summary>
        ///     Utilidad para colisiones en el escenario BSP
        /// </summary>
        public BspCollisionManager CollisionManager { get; }

        /// <summary>
        ///     Configigurar visibilidad inicial de meshes
        /// </summary>
        public void initVisibility()
        {
            //Deshabilitar todas
            foreach (var mesh in Meshes)
            {
                if (mesh != null)
                {
                    mesh.Enabled = false;
                }
            }
        }

        /// <summary>
        ///     Renderizar escenario BSP utilizando matriz PVS para descartar clusters no visibles
        /// </summary>
        /// <param name="camPos">Posición actual de la camara</param>
        public void render(Vector3 camPos)
        {
            var elapsedTime = GuiController.Instance.ElapsedTime;
            time += elapsedTime;

            //Obtener leaf actual
            var actualLeaf = FindLeaf(camPos);

            //Obtener clusters visibles segun PVS
            var actualCluster = Data.leafs[actualLeaf].cluster;
            if (actualCluster != antCluster)
            {
                antCluster = actualCluster;
                clusterVis.Clear();
                for (var i = 0; i < Data.leafs.Length; i++)
                {
                    if (isClusterVisible(actualCluster, Data.leafs[i].cluster))
                        clusterVis.Add(i);
                }
            }

            //Actualizar volumen del Frustum con nuevos valores de camara
            var frustum = GuiController.Instance.Frustum;
            frustum.updateVolume(D3DDevice.Instance.Device.Transform.View,
                D3DDevice.Instance.Device.Transform.Projection);

            foreach (var nleaf in clusterVis)
            {
                //Frustum Culling con el AABB del cluster
                var result = TgcCollisionUtils.classifyFrustumAABB(frustum, Data.leafs[nleaf].boundingBox);
                if (result == TgcCollisionUtils.FrustumResult.OUTSIDE)
                    continue;

                //Habilitar meshes de este leaf
                var currentLeaf = Data.leafs[nleaf];
                for (var i = currentLeaf.firstLeafSurface;
                    i < currentLeaf.firstLeafSurface + currentLeaf.numLeafSurfaces;
                    i++)
                {
                    var iMesh = Data.leafSurfaces[i];
                    var mesh = Meshes[iMesh];
                    if (mesh != null)
                    {
                        Meshes[iMesh].Enabled = true;
                    }
                }
            }

            //Renderizar meshes visibles
            mViewProj = D3DDevice.Instance.Device.Transform.View * D3DDevice.Instance.Device.Transform.Projection;
            for (var i = 0; i < Meshes.Count; i++)
            {
                //Ignonar si no está habilitada
                var mesh = Meshes[i];
                if (mesh == null || !mesh.Enabled)
                {
                    continue;
                }

                var shader = Data.shaderXSurface[i];

                //Renderizado con shaders
                //TODO: Mejorar el renderizado de shaders. Por ahora esta deshabilitado
                if (shader != null && shader.Stages.Count > 0)
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
        ///     Ejecuta el shader original de formato propio de Quake 3 convertido a una version similar de HLSL en DirectX.
        ///     Estado experimental. Desabilitado por el momento.
        /// </summary>
        private void renderShaderMesh(TgcMesh mesh, QShaderData shader)
        {
            // if ((shader.Stages[0].HasBlendFunc) ^ (pass == 1))
            // {
            //tiene es opaco se tiene que renderizar en la primer pasada
            //tiene alpha blending se tiene que renderizar en la segunda pasada
            //    return;
            //}

            var fx = shader.Fx;
            fx.Technique = "tec0";
            fx.SetValue("g_mWorld", Matrix.Identity);
            fx.SetValue("g_mViewProj", mViewProj);
            fx.SetValue("g_time", time);

            TgcTexture originalTexture = null;

            if (mesh.DiffuseMaps != null)
                originalTexture = mesh.DiffuseMaps[0];

            fx.Begin(FX.None);

            for (var j = 0; j < shader.Stages.Count; j++)
            {
                fx.BeginPass(j);
                if (shader.Stages[j].Textures.Count > 0)
                {
                    mesh.DiffuseMaps[0] = shader.Stages[j].Textures[0];
                }

                //mesh.render();
                D3DDevice.Instance.Device.SetTexture(0, mesh.DiffuseMaps[0].D3dTexture);
                if (mesh.LightMap != null)
                    D3DDevice.Instance.Device.SetTexture(1, mesh.LightMap.D3dTexture);
                else
                    D3DDevice.Instance.Device.SetTexture(1, null);

                mesh.D3dMesh.DrawSubset(0);

                fx.EndPass();
            }

            fx.End();

            if (mesh.DiffuseMaps != null)
                mesh.DiffuseMaps[0] = originalTexture;
        }

        /// <summary>
        ///     Encontrar la hoja actual del arbol BP
        /// </summary>
        private int FindLeaf(Vector3 pos)
        {
            var index = 0;

            while (index >= 0)
            {
                var node = Data.nodes[index];
                var plane = Data.planes[node.planeNum];

                // Distance from point to a plane
                var distance = Vector3.Dot(plane.normal, pos) - plane.dist;

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
        ///     Indica si un cluster es visible en la matriz PVS
        /// </summary>
        private bool isClusterVisible(int visCluster, int testCluster)
        {
            if ((Data.visData == null) || (visCluster < 0))
            {
                return true;
            }

            var i = visCluster * Data.visData.sizeVec + (testCluster >> 3);
            var visSet = Data.visData.data[i];

            return (visSet & (1 << (testCluster & 7))) != 0;
        }

        /// <summary>
        ///     Liberar recursos
        /// </summary>
        public void dispose()
        {
            foreach (var mesh in Meshes)
            {
                if (mesh != null)
                {
                    mesh.dispose();
                }
            }
            Meshes = null;
            foreach (var shader in Data.shaderXSurface)
            {
                if (shader != null && shader.Fx != null)
                {
                    shader.Fx.Dispose();
                }
            }
            Data = null;
        }
    }
}