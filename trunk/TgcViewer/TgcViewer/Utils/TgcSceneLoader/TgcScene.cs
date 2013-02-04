using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using System.Collections;
using TgcViewer.Utils.PortalRendering;

namespace TgcViewer.Utils.TgcSceneLoader
{
    /// <summary>
    /// Escena compuesta por un conjunto de Meshes estáticos
    /// </summary>
    public class TgcScene
    {
        string sceneName;
        /// <summary>
        /// Nombre de la escena
        /// </summary>
        public string SceneName
        {
            get { return sceneName; }
        }

        string filePath;
        /// <summary>
        /// Path del archivo XML de la escena
        /// </summary>
        public string FilePath
        {
            get { return filePath; }
        }

        List<TgcMesh> meshes;
        /// <summary>
        /// Mallas cargadas en la escena
        /// </summary>
        public List<TgcMesh> Meshes
        {
            get { return meshes; }
        }

        TgcBoundingBox boundingBox;
        /// <summary>
        /// BoundingBox de toda la escena, englobando todos los modelos.
        /// No se actualiza con los movimientos particulares de cada modelo.
        /// </summary>
        public TgcBoundingBox BoundingBox
        {
            get { return boundingBox; }
            set { boundingBox = value; }
        }

        TgcPortalRenderingManager portalRendering;
        /// <summary>
        /// Herramienta de PortalRendering.
        /// Puede estar en null si no hay información cargada para esto.
        /// </summary>
        public TgcPortalRenderingManager PortalRendering
        {
            get { return portalRendering; }
            set { portalRendering = value; }
        }

        /// <summary>
        /// Crea una nueva escena
        /// </summary>
        /// <param name="sceneName">Nombre de la escena</param>
        /// <param name="filePath">Path del archivo XML</param>
        public TgcScene(string sceneName, string filePath)
        {
            this.sceneName = sceneName;
            this.filePath = filePath;
            this.meshes = new List<TgcMesh>();
        }

        /// <summary>
        /// Habilita o deshabilita todas las mallas
        /// </summary>
        /// <param name="flag"></param>
        public void setMeshesEnabled(bool flag)
        {
            foreach (TgcMesh mesh in meshes)
            {
                mesh.Enabled = flag;
            }
        }

        /// <summary>
        /// Renderiza todas las mallas que se encuentran habilitadas
        /// </summary>
        public void renderAll()
        {
            renderAll(false);
        }

        /// <summary>
        /// Renderiza todas las mallas que se encuentran habilitadas, indicando
        /// si se debe mostrar el BoundingBox de las mismas.
        /// </summary>
        /// <param name="showBoundingBox">True para renderizar el BoundingBox de cada malla</param>
        public void renderAll(bool showBoundingBox)
        {
            foreach (TgcMesh mesh in meshes)
            {
                mesh.render();
            }

            if (showBoundingBox)
            {
                foreach (TgcMesh mesh in meshes)
                {
                    mesh.BoundingBox.render();
                }
            }
        }

        /// <summary>
        /// Libera los recursos de todas las mallas
        /// </summary>
        public void disposeAll()
        {
            foreach (TgcMesh mesh in meshes)
            {
                mesh.dispose();
            }
        }

        /// <summary>
        /// Devuelve dos listas de meshes utilizando el criterio establecido.
        /// Todos los meshes cuyo nombre está en el array list1Criteria se cargan en la list1.
        /// El resto se cargan en la list2.
        /// </summary>
        /// <param name="list1Criteria">Nombre de meshes a filtrar</param>
        /// <param name="list1">Lista con los meshes que cumplen con list1Criteria</param>
        /// <param name="list2">Lista con el resto de los meshes</param>
        public void separeteMeshList(string[] list1Criteria, out List<TgcMesh> list1, out List<TgcMesh> list2)
        {
            list1 = new List<TgcMesh>();
            list2 = new List<TgcMesh>();

            foreach (TgcMesh mesh in meshes)
            {
                for (int i = 0; i < list1Criteria.Length; i++)
                {
                    if (list1Criteria[i] == mesh.Name)
                    {
                        list1.Add(mesh);
                    }
                    else
                    {
                        list2.Add(mesh);
                    }
                }
            }
        }

        /// <summary>
        /// Devuelve el mesh con el nombre indicado
        /// </summary>
        /// <param name="meshName">Nombre del mesh buscado</param>
        /// <returns>Mesh encontrado o null si no encontró ninguno</returns>
        public TgcMesh getMeshByName(string meshName)
        {
            foreach (TgcMesh mesh in meshes)
            {
                if (mesh.Name == meshName)
                {
                    return mesh;
                }
            }
            return null;
        }


        

    }
}
