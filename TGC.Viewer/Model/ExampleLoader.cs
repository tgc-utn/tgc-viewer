using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using TGC.Core.Example;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Viewer.Model
{
    /// <summary>
    /// Utilidad para cargar dinamicamente las DLL de los ejemplos.
    /// </summary>
    public class ExampleLoader
    {
        private const string TGC = "TGC";
        private readonly Dictionary<TreeNode, TGCExample> treeExamplesDict;
        private readonly TgcUserVars userVars;

        public ExampleLoader(string mediaDirectory, string shadersDirectory, DataGridView dataGridUserVars, Panel modifiersPanel)
        {
            treeExamplesDict = new Dictionary<TreeNode, TGCExample>();
            MediaDirectory = mediaDirectory;
            ShadersDirectory = shadersDirectory;
            userVars = new TgcUserVars(dataGridUserVars);
            ModifiersPanel = modifiersPanel;
        }

        /// <summary>
        /// Ejemplos actualmente cargados.
        /// </summary>
        public List<TGCExample> CurrentExamples { get; set; }

        /// <summary>
        /// Ejemplo actualmente cargado.
        /// </summary>
        public TGCExample CurrentExample { get; set; }

        /// <summary>
        /// Path de la carpeta Media que contiene todo el contenido visual de los ejemplos, como texturas, modelos 3D, etc.
        /// </summary>
        public string MediaDirectory { get; set; }

        /// <summary>
        /// Path de la carpeta Shaders que contiene todo los shaders genericos.
        /// </summary>
        public string ShadersDirectory { get; set; }

        /// <summary>
        /// Panel donde van los modificadores.
        /// </summary>
        public Panel ModifiersPanel { get; set; }

        /// <summary>
        /// Carga los ejemplos dinamicamente en el TreeView de Ejemplo.
        /// </summary>
        /// <param name="treeView"> Arbol donde cargar los ejemplos.</param>
        /// <param name="exampleDir"> Carpeta donde estan las DLLs con los ejemplos.</param>
        public void LoadExamplesInGui(TreeView treeView, string exampleDir)
        {
            //Cargar ejemplos dinamicamente
            CurrentExamples = new List<TGCExample>();
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
        /// Devuelve el Ejemplo correspondiente a un TreeNode.
        /// </summary>
        /// <param name="treeNode"> Nodo donde buscar el ejemplo.</param>
        /// <returns> Ejemplo buscado.</returns>
        public TGCExample GetExampleByTreeNode(TreeNode treeNode)
        {
            return treeExamplesDict[treeNode];
        }

        /// <summary>
        /// Indica si ya existe un nodo en el arbol de ejemplos bajo esa categoria.
        /// Devuelve el nodo encontrado o Null.
        /// </summary>
        /// <param name="treeView"> Nodo donde buscar.</param>
        /// <param name="category"> Categoria a buscar.</param>
        /// <returns>Nodo buscado o null.</returns>
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
        /// Carga dinamicamente todas las dll de ejemplos de la carpeta de ejemplos.
        /// </summary>
        /// <param name="exampleDir"> Ruta del directorio que contiene los ejemplos.</param>
        /// <returns> Lista con todos los ejemplos instanciados a partir de las DLLs encontrdas.</returns>
        public List<TGCExample> LoadExamples(string exampleDir)
        {
            var examples = new List<TGCExample>();

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

                        if (type.BaseType == typeof(TGCExampleViewer) || type.BaseType == typeof(TGCExampleViewerNetworking))
                        {
                            var obj = Activator.CreateInstance(type, MediaDirectory, ShadersDirectory, userVars, ModifiersPanel);
                            var example = (TGCExampleViewer)obj;
                            examples.Add(example);
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "No se pudo cargar la dll: " + file, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return examples;
        }

        /// <summary>
        /// Actualiza los directorios donde estan los media y los shaders.
        /// </summary>
        /// <param name="mediaDirectory">Directorio de la media.</param>
        /// <param name="shadersDirectory">Directorio de los shaders.</param>
        public void UpdateMediaAndShaderDirectories(string mediaDirectory, string shadersDirectory)
        {
            MediaDirectory = mediaDirectory;
            ShadersDirectory = shadersDirectory;

            foreach (TGCExample example in CurrentExamples)
            {
                example.MediaDir = mediaDirectory;
                example.ShadersDir = shadersDirectory;
            }
        }

        /// <summary>
        /// Busca recursivamente en un directorio todos los archivos .DLL.
        /// </summary>
        /// <param name="rootDir"> Donde se empieza a buscar ejemplos.</param>
        /// <returns> Lista con todas las DLLs encontradas</returns>
        public List<string> GetExampleFiles(string rootDir)
        {
            var exampleFiles = new List<string>();
            var dllArray = Directory.GetFiles(rootDir, "*.dll", SearchOption.TopDirectoryOnly);

            foreach (var dll in dllArray)
            {
                // De esta forma no cargo DLLs que no sean de TGC
                if (Path.GetFileName(dll).StartsWith(TGC, StringComparison.Ordinal))
                {
                    exampleFiles.Add(dll);
                }
            }

            return exampleFiles;
        }

        /// <summary>
        /// Devuelve el primer TGCExample con el name y category especificados (de los metodos GetName y GetCategory).
        /// </summary>
        /// <param name="name"> Nombre del ejemplo.</param>
        /// <param name="category"> Categoria del ejemplo.</param>
        /// <returns> Ejemplo encontrado con los datos dados.</returns>
        public TGCExample GetExampleByName(string name, string category)
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