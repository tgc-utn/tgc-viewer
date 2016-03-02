using Microsoft.DirectX;
using System.Collections.Generic;
using TGC.Core.SceneLoader;
using TGC.Core.Utils;
using TGC.Viewer;

namespace TGC.Examples.TerrainEditor.Instances
{
    public class InstancesManager
    {
        protected static InstancesManager instance;

        private static string location = GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vegetacion\\";

        protected Dictionary<string, TgcMesh> meshes = new Dictionary<string, TgcMesh>();

        public static InstancesManager Instance
        {
            get
            {
                if (instance == null) instance = new InstancesManager();
                return instance;
            }
        }

        public static string Location
        {
            get { return location; }
            set
            {
                if (!value.EndsWith("\\")) value += "\\";
                location = value;
            }
        }

        public static void Clear()
        {
            foreach (var m in Instance.meshes.Values) m.dispose();
            Instance.meshes.Clear();
        }

        /// <summary>
        ///     Recibe el nombre de un mesh original y retorna una instancia. Si ese mesh no esta cargado,
        ///     lo busca en [Location]\[name]\[name]-TgcScene.xml
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TgcMesh newMeshInstanceOf(string name)
        {
            if (!meshes.ContainsKey(name))
            {
                var path = Location + name + "\\" + name + "-TgcScene.xml";
                var loader = new TgcSceneLoader();
                var scene = loader.loadSceneFromFile(path);
                scene.Meshes[0].Name = name;
                meshes.Add(name, scene.Meshes[0]);
            }

            return instanceOf(meshes[name]);
        }

        protected TgcMesh instanceOf(TgcMesh m)
        {
            var i = m.createMeshInstance(m.Name + m.MeshInstances.Count);
            i.AlphaBlendEnable = m.AlphaBlendEnable;
            return i;
        }

        public void export(List<TgcMesh> meshes, string name, string saveFolderPath)
        {
            var exportScene = new TgcScene(name, saveFolderPath);
            var parents = new List<TgcMesh>();

            foreach (var m in meshes)
            {
                if (m.ParentInstance != null && !parents.Contains(m.ParentInstance))
                {
                    parents.Add(m.ParentInstance);
                    exportScene.Meshes.Add(m.ParentInstance);
                }
                exportScene.Meshes.Add(m);
            }

            var exporter = new TgcSceneExporter();

            exporter.exportSceneToXml(exportScene, saveFolderPath);
        }

        public List<TgcMesh> import(string path)
        {
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(path);
            var instances = new List<TgcMesh>();
            foreach (var m in scene.Meshes)
            {
                if (m.ParentInstance == null) //Si es un mesh original
                {
                    //Lo agrego al diccionario
                    if (!meshes.ContainsKey(m.Name)) meshes.Add(m.Name, m);
                    else
                    {
                        //Por ahora no se puede cargar mas de un mesh original con el mismo nombre.
                        //TODO: Ver de que otro modo indexarlas y como unificar instancias de un mismo mesh que se cargo dos veces.

                        meshes[m.Name].dispose();
                        meshes.Remove(m.Name);
                    }

                    //Si no tenia hijos, creo una instancia para que se vea.
                    if (m.MeshInstances.Count == 0)
                    {
                        instances.Add(instanceOf(m));
                    }
                }
                else instances.Add(m);
            }
            return instances;
        }

        protected Vector3 parseVector3(string s)
        {
            var f3 = TgcParserUtils.parseFloat3Array(s);
            return new Vector3(f3[0], f3[1], f3[2]);
        }
    }
}