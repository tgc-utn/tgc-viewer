using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using TGC.Core.Example;

namespace TGC.Util.Example
{
    /// <summary>
    ///     Utilidad para cargar dinamicamente las DLL de los ejemplos
    /// </summary>
    public class ExampleLoader
    {
        private readonly Dictionary<TreeNode, TgcExample> treeExamplesDict;

        public ExampleLoader()
        {
            treeExamplesDict = new Dictionary<TreeNode, TgcExample>();
        }

        /// <summary>
        ///     Ejemplos actualmente cargados
        /// </summary>
        public List<TgcExample> CurrentExamples { get; private set; }

        /// <summary>
        ///     Carga los ejemplos dinámicamente en el TreeView de Ejemplo
        /// </summary>
        public void loadExamplesInGui(TreeView treeView)
        {
            //Cargar ejemplos dinamicamente
            CurrentExamples = new List<TgcExample>();

            var exampleDir = Environment.CurrentDirectory;
            CurrentExamples.AddRange(loadExamples(exampleDir));

            //Cargar el TreeView, agrupando ejemplos por categoria
            treeExamplesDict.Clear();
            foreach (var example in CurrentExamples)
            {
                var node = new TreeNode();
                var exampleName = example.getName();
                var exampleCategory = example.getCategory();
                node.Text = exampleName;
                node.Name = exampleName;
                node.ToolTipText = example.getDescription();

                //Crear nodo padre si no existe
                var parent = treeNodeExists(treeView, exampleCategory);
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
        ///     Devuelve el Ejemplo correspondiente a un TreeNode
        /// </summary>
        public TgcExample getExampleByTreeNode(TreeNode treeNode)
        {
            return treeExamplesDict[treeNode];
        }

        /// <summary>
        ///     Indica si ya existe un nodo en el arbol de ejemplos bajo esa categoria.
        ///     Devuelve el nodo encontrado o Null.
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
        ///     Carga dinamicamente todas las dll de ejemplos de la carpeta de ejemplos
        /// </summary>
        /// <param name="exampleDir"></param>
        /// <returns></returns>
        public List<TgcExample> loadExamples(string exampleDir)
        {
            //Buscar todas las dll que esten en el directorio de ejemplos, evitando las creadas de DEBUG Y BIN por parte del IDE
            var exampleFiles = getExampleFiles(exampleDir);

            var examples = new List<TgcExample>();
            foreach (var file in exampleFiles)
            {
                try
                {
                    var assembly = Assembly.LoadFile(file);
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!type.IsClass || type.IsNotPublic || type.IsAbstract)
                            continue;

                        if (type.BaseType.Equals(typeof(TgcExample)))
                        {
                            var obj = Activator.CreateInstance(type);
                            var example = (TgcExample)obj;
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
        ///     Busca recursivamente en un directorio todos los archivos .DLL
        /// </summary>
        public List<string> getExampleFiles(string rootDir)
        {
            var exampleFiles = new List<string>();
            var dllArray = Directory.GetFiles(rootDir, "*.dll", SearchOption.TopDirectoryOnly);
            foreach (var dll in dllArray)
            {
                exampleFiles.Add(dll);
            }

            return exampleFiles;
        }

        /// <summary>
        ///     Devuelve el primer TgcExample con el name y category especificados (de los metodos getName y getCategory)
        /// </summary>
        public TgcExample getExampleByName(string name, string category)
        {
            foreach (var example in CurrentExamples)
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