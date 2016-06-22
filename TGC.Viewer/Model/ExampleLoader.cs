using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Viewer.Model
{
    /// <summary>
    ///     Utilidad para cargar dinamicamente las DLL de los ejemplos
    /// </summary>
    public class ExampleLoader
    {
        private readonly TgcAxisLines axisLines;
        private readonly TgcCamera camara;
        private readonly string mediaDirectory;
        private readonly TgcModifiers modifiers;
        private readonly string shadersDirectory;
        private readonly Dictionary<TreeNode, TgcExample> treeExamplesDict;
        private readonly TgcUserVars userVars;

        public ExampleLoader(string mediaDirectory, string shadersDirectory, DataGridView dataGridUserVars,
            FlowLayoutPanel flowLayoutPanelModifiers)
        {
            treeExamplesDict = new Dictionary<TreeNode, TgcExample>();
            this.mediaDirectory = mediaDirectory;
            this.shadersDirectory = shadersDirectory;
            userVars = new TgcUserVars(dataGridUserVars);
            modifiers = new TgcModifiers(flowLayoutPanelModifiers);
            axisLines = new TgcAxisLines();
            //Es la camara que ponemos por default
            camara = new TgcCamera();
        }

        /// <summary>
        ///     Ejemplos actualmente cargados
        /// </summary>
        public List<TgcExample> CurrentExamples { get; set; }

        /// <summary>
        ///     Ejemplo actualmente cargado
        /// </summary>
        public TgcExample CurrentExample { get; set; }

        /// <summary>
        ///     Carga los ejemplos dinamicamente en el TreeView de Ejemplo
        /// </summary>
        public void LoadExamplesInGui(TreeView treeView, string exampleDir)
        {
            //Cargar ejemplos dinamicamente
            CurrentExamples = new List<TgcExample>();
            CurrentExamples.AddRange(LoadExamples(exampleDir));

            //Cargar el TreeView, agrupando ejemplos por categoria
            treeExamplesDict.Clear();

            foreach (var example in CurrentExamples)
            {
                var node = new TreeNode();
                var exampleName = example.Name;
                var exampleCategory = example.Category;
                node.Text = exampleName;
                node.Name = exampleName;
                node.ToolTipText = example.Description;

                //Crear nodo padre si no existe
                var parent = TreeNodeExists(treeView, exampleCategory);

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
        public TgcExample GetExampleByTreeNode(TreeNode treeNode)
        {
            return treeExamplesDict[treeNode];
        }

        /// <summary>
        ///     Indica si ya existe un nodo en el arbol de ejemplos bajo esa categoria.
        ///     Devuelve el nodo encontrado o Null.
        /// </summary>
        private TreeNode TreeNodeExists(TreeView treeView, string category)
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
        public List<TgcExample> LoadExamples(string exampleDir)
        {
            var examples = new List<TgcExample>();

            //Buscar todas las dll que esten en el directorio de ejemplos, evitando las creadas de DEBUG Y BIN por parte del IDE
            var exampleFiles = GetExampleFiles(exampleDir);

            foreach (var file in exampleFiles)
            {
                try
                {
                    var assembly = Assembly.LoadFile(file);
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!type.IsClass || type.IsNotPublic || type.IsAbstract)
                        {
                            continue;
                        }

                        if (type.BaseType.Equals(typeof(TgcExample)))
                        {
                            var obj = Activator.CreateInstance(type, mediaDirectory, shadersDirectory, userVars,
                                modifiers, axisLines, camara);
                            var example = (TgcExample)obj;
                            examples.Add(example);
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "No se pudo cargar la dll: " + file, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }

            return examples;
        }

        /// <summary>
        ///     Busca recursivamente en un directorio todos los archivos .DLL
        /// </summary>
        public List<string> GetExampleFiles(string rootDir)
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
        ///     Devuelve el primer TgcExample con el name y category especificados (de los metodos GetName y GetCategory)
        /// </summary>
        public TgcExample GetExampleByName(string name, string category)
        {
            foreach (var example in CurrentExamples)
            {
                if (example.Name == name && example.Category == category)
                {
                    return example;
                }
            }

            throw new Exception("Example not found. Name: " + name + ", Category: " + category);
        }
    }
}