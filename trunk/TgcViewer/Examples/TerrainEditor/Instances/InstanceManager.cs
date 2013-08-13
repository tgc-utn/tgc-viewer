using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using System.Xml;
using System.IO;
using TgcViewer;
using Microsoft.DirectX;
using System.Drawing;

namespace Examples.TerrainEditor.Vegetation
{
    public class InstancesManager
    {
        private static InstancesManager instance;
        public static InstancesManager Instance { get { if (instance == null) instance = new InstancesManager(); return instance; } }


        private static string location = GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vegetacion\\";

        public static string Location { get { return location; } set { if (!value.EndsWith("\\")) value += "\\"; location = value; } }

        public static void Clear()
        {
            foreach (TgcMesh m in Instance.meshes.Values) m.dispose();
            Instance.meshes.Clear();
        }


        private Dictionary<string, TgcMesh> meshes = new Dictionary<string, TgcMesh>();

        /// <summary>
        /// Recibe el nombre de un mesh original y retorna una instancia. Si ese mesh no esta cargado,
        /// lo busca en [Location]\[name]\[name]-TgcScene.xml
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TgcMesh newVegetationMeshInstance(string name)
        {

            if (!meshes.ContainsKey(name))
            {
                string path = Location + name + "\\" + name + "-TgcScene.xml";
                TgcSceneLoader loader = new TgcSceneLoader();
                TgcScene scene = loader.loadSceneFromFile(path);
                scene.Meshes[0].Name = name;
                meshes.Add(name, scene.Meshes[0]);
            }

            TgcMesh instance = meshes[name].createMeshInstance(meshes[name].Name + meshes[name].MeshInstances.Count.ToString());
            instance.AlphaBlendEnable = meshes[name].AlphaBlendEnable;
            return instance;
        }

        public void export(List<TgcMesh> meshes, string name, string saveFolderPath)
        {
            TgcScene exportScene = new TgcScene(name, saveFolderPath);
            List<TgcMesh> parents = new List<TgcMesh>();

            foreach (TgcMesh m in meshes)
            {
                if (m.ParentInstance != null && !parents.Contains(m.ParentInstance))
                {
                    parents.Add(m.ParentInstance);
                    exportScene.Meshes.Add(m.ParentInstance);
                }
                exportScene.Meshes.Add(m);
            }

            TgcSceneExporter exporter = new TgcSceneExporter();

            exporter.exportSceneToXml(exportScene, saveFolderPath);

        }


        public List<TgcMesh> import(string path)
        {
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(path);
            List<TgcMesh> instances = new List<TgcMesh>();
            foreach (TgcMesh m in scene.Meshes)
            {
                if (m.ParentInstance == null)//Si es un mesh original
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
                        TgcMesh i = m.createMeshInstance(m.Name + "0");
                        i.AlphaBlendEnable = m.AlphaBlendEnable;
                        instances.Add(i);

                    }
                }
                else instances.Add(m);
            }
            return instances;
        }

        private Vector3 parseVector3(string s)
        {
            float[] f3 = TgcParserUtils.parseFloat3Array(s);
            return new Vector3(f3[0], f3[1], f3[2]);

        }



    }
}
