using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Windows.Forms;

namespace TgcViewer.Example
{
    /// <summary>
    /// Utilidad para cargar dinamicamente las DLL de los ejemplos
    /// </summary>
    class ExampleLoader
    {
        public const string EXAMPLES_DIR = "Examples";
        public static string[] DIR_SKIP_LISP = new string[] { "\\bin", "\\obj"};

        private Dictionary<TreeNode, TgcExample> treeExamplesDict;

        List<TgcExample> currentExamples;
        /// <summary>
        /// Ejemplos actualmente cargados
        /// </summary>
        public List<TgcExample> CurrentExamples
        {
            get { return currentExamples; }
        }

        public ExampleLoader()
        {
            treeExamplesDict = new Dictionary<TreeNode, TgcExample>();
        }

        /// <summary>
        /// Carga los ejemplos dinámicamente en el TreeView de Ejemplo
        /// </summary>
        public void loadExamplesInGui(TreeView treeView, string[] exampleDirs)
        {
            //Cargar ejemplos dinamicamente
            currentExamples = new List<TgcExample>();
            foreach (string exampleDir in exampleDirs)
            {
                currentExamples.AddRange(loadExamples(exampleDir));
            }

            //Cargar el TreeView, agrupando ejemplos por categoria
            treeExamplesDict.Clear();
            foreach (TgcExample example in currentExamples)
            {
                TreeNode node = new TreeNode();
                string exampleName = example.getName();
                string exampleCategory = example.getCategory();
                node.Text = exampleName;
                node.Name = exampleName;
                node.ToolTipText = example.getDescription();

                //Crear nodo padre si no existe
                TreeNode parent = treeNodeExists(treeView, exampleCategory);
                if (parent == null)
                {
                    parent = new TreeNode();
                    parent.Name = exampleCategory;
                    parent.Text = exampleCategory;
                    parent.ToolTipText = exampleCategory;
                    treeView.Nodes.Add(parent);
                }

                parent.Nodes.Add(node);
                treeExamplesDict[node] = example;
            }

            //Ordenar en forma ascendente los nodos del arbol
            treeView.Sort();
        }

        

        /// <summary>
        /// Devuelve el Ejemplo correspondiente a un TreeNode
        /// </summary>
        public TgcExample getExampleByTreeNode(TreeNode treeNode)
        {
            return treeExamplesDict[treeNode];
        }

        /// <summary>
        /// Indica si ya existe un nodo en el arbol de ejemplos bajo esa categoria.
        /// Devuelve el nodo encontrado o Null.
        /// </summary>
        private TreeNode treeNodeExists(TreeView treeView, string category)
        {
            foreach (TreeNode n in treeView.Nodes)
	        {
                if (n.Name == category)
                {
                    return n;
                }
	        }

            return null;
        }

        /// <summary>
        /// Carga dinamicamente todas las dll de ejemplos de la carpeta de ejemplos
        /// </summary>
        /// <param name="exampleDir"></param>
        /// <returns></returns>
        public List<TgcExample> loadExamples(string exampleDir)
        {
            //Buscar todas las dll que esten en el directorio de ejemplos, evitando las creadas de DEBUG Y BIN por parte del IDE
            List<string> exampleFiles = getExampleFiles(exampleDir, DIR_SKIP_LISP);

            List<TgcExample> examples = new List<TgcExample>();
            foreach (string file in exampleFiles)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(file);
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (!type.IsClass || type.IsNotPublic || type.IsAbstract)
                            continue;

                        /*
                        Type[] interfaces = type.GetInterfaces();
                        if (((IList)interfaces).Contains(typeof(TgcExample)))
                        {
                        */

                        if(type.BaseType.Equals(typeof(TgcExample)))
                        {
                            object obj = Activator.CreateInstance(type);
                            TgcExample example = (TgcExample)obj;
                            examples.Add(example);

                            //cargar path del ejemplo
                            example.setExampleDir(file.Substring(0, file.LastIndexOf("\\") + 1));
                        }
                    }

                }
                catch (Exception e)
                {
                    GuiController.Instance.Logger.logError("No se pudo cargar la dll: " + file, e);
                }

            }

            return examples;
        }

        /// <summary>
        /// Busca recursivamente en un directorio todos los archivos .DLL, evitando los directorios
        /// especificados en la skipList
        /// </summary>
        public List<string> getExampleFiles(string rootDir, string[] skipList)
        {
            List<string> exampleFiles = new List<string>();
            string[] dllArray = Directory.GetFiles(rootDir, "*.dll", SearchOption.TopDirectoryOnly);
            foreach (string dll in dllArray)
            {
                if (!isInSkipList(dll, skipList))
                {
                    exampleFiles.Add(dll);
                }
            }

            return exampleFiles;
        }

        /// <summary>
        /// Informa si un directorio es parte de la skipList
        /// </summary>
        public bool isInSkipList(string dir, string[] skipList)
        {
            foreach (string skipDir in skipList)
            {
                if (dir.IndexOf(skipDir) > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Devuelve el primer TgcExample con el name y category especificados (de los metodos getName y getCategory)
        /// </summary>
        public TgcExample getExampleByName(string name, string category)
        {
            foreach (TgcExample example in this.currentExamples)
            {
                if (example.getName() == name && example.getCategory() == category)
                {
                    return example;
                }
            }
            throw new Exception("Example not found. Name: " + name + ", Category: " + category);
        }


    }
}
