using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace TgcViewer.Utils.Modifiers
{
    /// <summary>
    /// Ventana visualizadora de TgcMesh de un directorio.
    /// Muestra la imagen de preview.jpg del TgcMesh (si tiene)
    /// </summary>
    public partial class TgcMeshBrowser : Form
    {
        OpenFileDialog browseDialog;
        MeshItemControl selectedItem;


        public TgcMeshBrowser()
        {
            InitializeComponent();

            browseDialog = new OpenFileDialog();
            browseDialog.CheckFileExists = true;
            browseDialog.Title = "Select -TgcScene.xml mesh file";
            browseDialog.Filter = "-TgcScene.xml |*-TgcScene.xml";
            browseDialog.Multiselect = false;
        }

        /// <summary>
        /// Path del mesh seleccionado
        /// </summary>
        public string SelectedMesh
        {
            get { return selectedItem.filePath; }
        }


        /// <summary>
        /// Directorio actual del cual se van a cargar meshes.
        /// Al especificar un directorio se cargan todas los meshes que haya en el mismo.
        /// </summary>
        public string CurrentDir
        {
            get { return browseDialog.FileName; }
            set {
                browseDialog.FileName = value;
                loadFolderContent(browseDialog.FileName);
            }
        }

        /// <summary>
        /// Carga todas los meshes del directorio padre en donde se encuentra el mesh especificada,
        /// y lo marca como mesh seleccionado de la ventana.
        /// </summary>
        public void setSelectedMesh(string meshPath)
        {
            //Si no cambio el mesh seleccionado, evitamos hacer la carga de todas los mesh del directorio padre
            if (selectedItem != null && meshPath == selectedItem.filePath)
            {
                return;
            }

            //Cargar meshes del directorio padre
            FileInfo fi = new FileInfo(meshPath);
            if (fi.Directory.Parent != null)
            {
                this.CurrentDir = fi.Directory.Parent.FullName;
            }
            

            //Buscar el mesh pedido para seleccionarlo, si es que está
            foreach (MeshItemControl item in panelItems.Controls)
            {
                if (item.filePath == meshPath)
                {
                    item.selectItem();
                }
            }
        }

        /// <summary>
        /// Cargar meshes del directorio especificado
        /// </summary>
        public void loadFolderContent(string folderPath)
        {
            textBoxFolderPath.Text = browseDialog.FileName;
            panelItems.Controls.Clear();

            if (Directory.Exists(folderPath))
            {
                //Obtener todas las carpetas del directorio
                string[] allDirs = Directory.GetDirectories(folderPath);

                //Excluir subdirectorios innecesarios
                List<string> dirs = new List<string>();
                for (int i = 0; i < allDirs.Length; i++)
                {
                    if (!allDirs[i].Contains(".svn"))
                    {
                        dirs.Add(allDirs[i]);
                    }
                }

                //Ver cuales son directorios simples y cuales tienen un -TgcScene.xml adentro
                List<MeshItemControl> meshes = new List<MeshItemControl>();
                foreach (string  dirPath in dirs)
                {
                    //Buscar si hay algun mesh adentro del directorio
                    string[] files = Directory.GetFiles(dirPath, "*-TgcScene.xml", SearchOption.TopDirectoryOnly);

                    //Tiene que haber uno solo
                    if (files.Length == 1)
                    {
                        //Ver si tiene imagen de preview
                        FileInfo fi = new FileInfo(files[0]);
                        string previewImagePath = fi.Directory.FullName + "\\preview.jpg";
                        if (!File.Exists(previewImagePath))
                        {
                            previewImagePath = null;
                        }

                        //Crear item de mesh
                        MeshItemControl item = new MeshItemControl(files[0], previewImagePath, this, false);
                        meshes.Add(item);
                    }
                    //No tiene mesh, es un directorio comun
                    else
                    {
                        //Crear item de directorio
                        MeshItemControl item = new MeshItemControl(dirPath, null, this, true);
                        panelItems.Controls.Add(item);
                    }

                }

                //Agregar todos los items de mesh al final
                panelItems.Controls.AddRange(meshes.ToArray());
                


                //Seleccionar primer elemento
                if (panelItems.Controls.Count > 0)
                {
                    ((MeshItemControl)panelItems.Controls[0]).selectItem();
                }
            }

        }

         
        /// <summary>
        /// Clic en "Browse"
        /// </summary>
        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            DialogResult r = browseDialog.ShowDialog(this);
            if (r == DialogResult.OK)
            {
                setSelectedMesh(browseDialog.FileName);
                this.Close();
            }
        }

        private void TgcMeshBrowser_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.selectedItem != null && !this.selectedItem.isDirectory)
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                this.DialogResult = DialogResult.Cancel;
            }
            
        }

        private void pictureBoxUpDir_Click(object sender, EventArgs e)
        {
            FileInfo info = new FileInfo(this.CurrentDir);
            if (info.Directory != null)
            {
                this.CurrentDir = info.Directory.FullName;
            }
        }


        /// <summary>
        /// Control para una imagen o directorio
        /// </summary>
        public class MeshItemControl : FlowLayoutPanel
        {
            TgcMeshBrowser browser;
            public PictureBox pictureBox;
            public Label filenameLabel;
            public string filePath;
            public bool isDirectory;

            public MeshItemControl(string filePath, string previewImagePath, TgcMeshBrowser browser, bool isDirectory)
            {
                this.filePath = filePath;
                this.browser = browser;
                this.isDirectory = isDirectory;

                this.BorderStyle = BorderStyle.FixedSingle;
                this.BackColor = Color.White;
                this.AutoSize = true;
                this.FlowDirection = FlowDirection.TopDown;
                this.Click += new EventHandler(ImageControl_Click);

                pictureBox = new PictureBox();
                
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox.Click += new EventHandler(pictureBox_Click);
                pictureBox.DoubleClick += new EventHandler(pictureBox_DoubleClick);
                //cargar imagen default para directorios
                if (this.isDirectory)
                {
                    pictureBox.Size = new Size(100, 100);
                    pictureBox.Image = browser.pictureBoxDirIcon.Image;
                }
                //cargar imagen de preview
                else
                {
                    pictureBox.Size = new Size(200, 200);
                    if (previewImagePath != null)
                    {
                        pictureBox.Image = Image.FromFile(previewImagePath);
                    }
                    else
                    {
                        pictureBox.Image = browser.pictureBoxNoImageIcon.Image;
                    }
                }
                this.Controls.Add(pictureBox);

                filenameLabel = new Label();
                filenameLabel.AutoSize = false;
                filenameLabel.Size = new Size(pictureBox.Width, 20);
                if (isDirectory)
                {
                    filenameLabel.Font = new Font(FontFamily.GenericSansSerif, 8,FontStyle.Bold);
                }
                string[] pathArray = filePath.Split('\\');
                filenameLabel.Text = pathArray[pathArray.Length - 1];
                filenameLabel.Click += new EventHandler(filenameLabel_Click);
                filenameLabel.TextAlign = ContentAlignment.MiddleCenter;
                this.Controls.Add(filenameLabel);
            }

            /// <summary>
            /// Seleccionar item
            /// </summary>
            public void selectItem()
            {
                if (browser.selectedItem != null)
                {
                    browser.selectedItem.BackColor = Color.White;
                }

                browser.selectedItem = this;
                this.BackColor = Color.Yellow;
            }

            void pictureBox_DoubleClick(object sender, EventArgs e)
            {
                selectItem();

                //Si es directorio, entrar a esa carpeta y cargar su contenido
                if (this.isDirectory)
                {
                    browser.CurrentDir = filePath;
                }
                //Si es imagen, cerrar ventana de texturas
                else
                {
                    browser.Close();
                }
            }

            void filenameLabel_Click(object sender, EventArgs e)
            {
                selectItem();
            }

            void pictureBox_Click(object sender, EventArgs e)
            {
                selectItem();
            }

            void ImageControl_Click(object sender, EventArgs e)
            {
                selectItem();
            }
        }


        

        

        
    }
}