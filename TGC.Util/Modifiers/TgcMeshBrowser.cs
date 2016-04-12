using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TGC.Util.Modifiers
{
    /// <summary>
    ///     Ventana visualizadora de TgcMesh de un directorio.
    ///     Muestra la imagen de preview.jpg del TgcMesh (si tiene)
    /// </summary>
    public partial class TgcMeshBrowser : Form
    {
        private readonly OpenFileDialog browseDialog;

        private MeshItemControl selectedItem;

        public TgcMeshBrowser()
        {
            InitializeComponent();

            browseDialog = new OpenFileDialog();
            browseDialog.CheckFileExists = true;
            browseDialog.Title = "Select -TgcScene.xml mesh file";
            browseDialog.Filter = "-TgcScene.xml |*-TgcScene.xml";
            browseDialog.Multiselect = false;
            HomeDirPath = GuiController.Instance.ExamplesMediaDir + "MeshCreator";
        }

        /// <summary>
        ///     Path del directorio Home
        /// </summary>
        public string HomeDirPath { get; set; }

        /// <summary>
        ///     Path del mesh seleccionado
        /// </summary>
        public string SelectedMesh
        {
            get { return selectedItem.filePath; }
        }

        /// <summary>
        ///     Directorio actual del cual se van a cargar meshes.
        ///     Al especificar un directorio se cargan todas los meshes que haya en el mismo.
        /// </summary>
        public string CurrentDir
        {
            get { return browseDialog.FileName; }
            set
            {
                browseDialog.FileName = value;
                loadFolderContent(browseDialog.FileName);
            }
        }

        /// <summary>
        ///     Carga todas los meshes del directorio padre en donde se encuentra el mesh especificada,
        ///     y lo marca como mesh seleccionado de la ventana.
        /// </summary>
        public void setSelectedMesh(string meshPath)
        {
            //Si no cambio el mesh seleccionado, evitamos hacer la carga de todas los mesh del directorio padre
            if (selectedItem != null && meshPath == selectedItem.filePath)
            {
                return;
            }

            //Cargar meshes del directorio padre
            var fi = new FileInfo(meshPath);
            if (fi.Directory.Parent != null)
            {
                CurrentDir = fi.Directory.Parent.FullName;
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
        ///     Cargar meshes del directorio especificado
        /// </summary>
        public void loadFolderContent(string folderPath)
        {
            textBoxFolderPath.Text = browseDialog.FileName;

            //Limpiar controles anteriores
            foreach (MeshItemControl c in panelItems.Controls)
            {
                c.dispose();
            }
            panelItems.Controls.Clear();

            if (Directory.Exists(folderPath))
            {
                //Obtener todas las carpetas del directorio
                var allDirs = Directory.GetDirectories(folderPath);

                //Excluir subdirectorios innecesarios
                var dirs = new List<string>();
                for (var i = 0; i < allDirs.Length; i++)
                {
                    if (!allDirs[i].Contains(".svn"))
                    {
                        dirs.Add(allDirs[i]);
                    }
                }

                //Ver cuales son directorios simples y cuales tienen un -TgcScene.xml adentro
                var meshes = new List<MeshItemControl>();
                foreach (var dirPath in dirs)
                {
                    //Buscar si hay algun mesh adentro del directorio
                    var files = Directory.GetFiles(dirPath, "*-TgcScene.xml", SearchOption.TopDirectoryOnly);

                    //Si hay al menos uno en el directorio
                    if (files.Length > 0)
                    {
                        //Ver si tiene imagen de preview
                        var fi = new FileInfo(files[0]);
                        var previewImagePath = fi.Directory.FullName + "\\preview.jpg";
                        if (!File.Exists(previewImagePath))
                        {
                            previewImagePath = null;
                        }

                        //Crear un item de mesh para cada uno
                        foreach (var file in files)
                        {
                            var item = new MeshItemControl(file, previewImagePath, this, false);
                            meshes.Add(item);
                        }
                    }
                    //No tiene mesh, es un directorio comun
                    else
                    {
                        //Crear item de directorio
                        var item = new MeshItemControl(dirPath, null, this, true);
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
        ///     Clic en "Browse"
        /// </summary>
        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            var r = browseDialog.ShowDialog(this);
            if (r == DialogResult.OK)
            {
                setSelectedMesh(browseDialog.FileName);
                Close();
            }
        }

        private void TgcMeshBrowser_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (selectedItem != null && !selectedItem.isDirectory)
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void pictureBoxUpDir_Click(object sender, EventArgs e)
        {
            var info = new FileInfo(CurrentDir);
            if (info.Directory != null)
            {
                CurrentDir = info.Directory.FullName;
            }
        }

        private void pictureBoxHomeDir_Click(object sender, EventArgs e)
        {
            CurrentDir = HomeDirPath;
        }

        /// <summary>
        ///     Control para una imagen o directorio
        /// </summary>
        public class MeshItemControl : FlowLayoutPanel
        {
            private readonly TgcMeshBrowser browser;
            public Label filenameLabel;
            public string filePath;
            public bool isDirectory;
            public PictureBox pictureBox;

            public MeshItemControl(string filePath, string previewImagePath, TgcMeshBrowser browser, bool isDirectory)
            {
                this.filePath = filePath;
                this.browser = browser;
                this.isDirectory = isDirectory;

                BorderStyle = BorderStyle.FixedSingle;
                BackColor = Color.White;
                AutoSize = true;
                FlowDirection = FlowDirection.TopDown;
                Click += ImageControl_Click;

                pictureBox = new PictureBox();

                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox.Click += pictureBox_Click;
                pictureBox.DoubleClick += pictureBox_DoubleClick;
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
                Controls.Add(pictureBox);

                filenameLabel = new Label();
                filenameLabel.AutoSize = false;
                filenameLabel.Size = new Size(pictureBox.Width, 20);
                if (isDirectory)
                {
                    filenameLabel.Font = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
                }
                var pathArray = filePath.Split('\\');
                filenameLabel.Text = pathArray[pathArray.Length - 1];
                filenameLabel.Click += filenameLabel_Click;
                filenameLabel.TextAlign = ContentAlignment.MiddleCenter;
                Controls.Add(filenameLabel);
            }

            /// <summary>
            ///     Seleccionar item
            /// </summary>
            public void selectItem()
            {
                if (browser.selectedItem != null)
                {
                    browser.selectedItem.BackColor = Color.White;
                }

                browser.selectedItem = this;
                BackColor = Color.Yellow;
            }

            public void pictureBox_DoubleClick(object sender, EventArgs e)
            {
                selectItem();

                //Si es directorio, entrar a esa carpeta y cargar su contenido
                if (isDirectory)
                {
                    browser.CurrentDir = filePath;
                }
                //Si es mesh, cerrar ventana de meshes
                else
                {
                    browser.Close();
                }
            }

            public void filenameLabel_Click(object sender, EventArgs e)
            {
                selectItem();
            }

            public void pictureBox_Click(object sender, EventArgs e)
            {
                selectItem();
            }

            public void ImageControl_Click(object sender, EventArgs e)
            {
                selectItem();
            }

            public void dispose()
            {
                if (!isDirectory && pictureBox.Image != browser.pictureBoxNoImageIcon.Image)
                {
                    if (pictureBox.Image != null)
                    {
                        pictureBox.Image.Dispose();
                    }
                }
            }
        }
    }
}