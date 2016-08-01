using System.Collections.Generic;
using TGC.Core.BoundingVolumes;
using TGC.Core.Geometry;
using TGC.Core.PortalRendering;

namespace TGC.Core.SceneLoader
{
    /// <summary>
    ///     Escena compuesta por un conjunto de Meshes estáticos
    /// </summary>
    public class TgcScene
    {
        /// <summary>
        ///     Crea una nueva escena
        /// </summary>
        /// <param name="sceneName">Nombre de la escena</param>
        /// <param name="filePath">Path del archivo XML</param>
        public TgcScene(string sceneName, string filePath)
        {
            SceneName = sceneName;
            FilePath = filePath;
            Meshes = new List<TgcMesh>();
        }

        /// <summary>
        ///     Nombre de la escena
        /// </summary>
        public string SceneName { get; }

        /// <summary>
        ///     Path del archivo XML de la escena
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        ///     Mallas cargadas en la escena
        /// </summary>
        public List<TgcMesh> Meshes { get; }

        /// <summary>
        ///     BoundingBox de toda la escena, englobando todos los modelos.
        ///     No se actualiza con los movimientos particulares de cada modelo.
        /// </summary>
        public TgcBoundingAxisAlignBox BoundingBox { get; set; }

        /// <summary>
        ///     Herramienta de PortalRendering.
        ///     Puede estar en null si no hay información cargada para esto.
        /// </summary>
        public TgcPortalRenderingManager PortalRendering { get; set; }

        /// <summary>
        ///     Habilita o deshabilita todas las mallas
        /// </summary>
        /// <param name="flag"></param>
        public void setMeshesEnabled(bool flag)
        {
            foreach (var mesh in Meshes)
            {
                mesh.Enabled = flag;
            }
        }

        /// <summary>
        ///     Renderiza todas las mallas que se encuentran habilitadas
        /// </summary>
        public void renderAll()
        {
            renderAll(false);
        }

        /// <summary>
        ///     Renderiza todas las mallas que se encuentran habilitadas, indicando
        ///     si se debe mostrar el BoundingBox de las mismas.
        /// </summary>
        /// <param name="showBoundingBox">True para renderizar el BoundingBox de cada malla</param>
        public void renderAll(bool showBoundingBox)
        {
            foreach (var mesh in Meshes)
            {
                mesh.render();
            }

            if (showBoundingBox)
            {
                foreach (var mesh in Meshes)
                {
                    mesh.BoundingBox.render();
                }
            }
        }

        /// <summary>
        ///     Libera los recursos de todas las mallas
        /// </summary>
        public void disposeAll()
        {
            foreach (var mesh in Meshes)
            {
                mesh.dispose();
            }
        }

        /// <summary>
        ///     Devuelve dos listas de meshes utilizando el criterio establecido.
        ///     Todos los meshes cuyo nombre está en el array list1Criteria se cargan en la list1.
        ///     El resto se cargan en la list2.
        /// </summary>
        /// <param name="list1Criteria">Nombre de meshes a filtrar</param>
        /// <param name="list1">Lista con los meshes que cumplen con list1Criteria</param>
        /// <param name="list2">Lista con el resto de los meshes</param>
        public void separeteMeshList(string[] list1Criteria, out List<TgcMesh> list1, out List<TgcMesh> list2)
        {
            list1 = new List<TgcMesh>();
            list2 = new List<TgcMesh>();

            foreach (var mesh in Meshes)
            {
                for (var i = 0; i < list1Criteria.Length; i++)
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
        ///     Devuelve el mesh con el nombre indicado
        /// </summary>
        /// <param name="meshName">Nombre del mesh buscado</param>
        /// <returns>Mesh encontrado o null si no encontró ninguno</returns>
        public TgcMesh getMeshByName(string meshName)
        {
            foreach (var mesh in Meshes)
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