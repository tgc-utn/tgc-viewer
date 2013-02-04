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
    /// Ventana visualizadora de im�genes de un directorio.
    /// Ideal para seleccionar texturas de un directorio.
    /// </summary>
    public partial class TgcTextureBrowser : Form
    {
        /// <summary>
        /// Extensiones soportadas
        /// </summary>
        readonly string[] FILE_EXT_SEARCH = new string[] {"*.jpg", "*.jpeg", "*.gif", "*.png", "*.bmp" };

        FolderBrowserDialog browseDialog;
        ImageControl selectedImage;

        bool showFolders;
        /// <summary>
        /// En True carga tambien las carpetas de un directorio y permite navegar por ellas
        /// </summary>
        public bool ShowFolders
        {
            get { return showFolders; }
            set { showFolders = value; }
        }

        public TgcTextureBrowser()
        {
            InitializeComponent();

            browseDialog = new FolderBrowserDialog();
            browseDialog.Description = "Select folder";
            browseDialog.ShowNewFolderButton = false;
            showFolders = false;
        }

        /// <summary>
        /// Path de la imagen seleccionada
        /// </summary>
        public string SelectedImage
        {
            get { return selectedImage.filePath; }
        }

        /// <summary>
        /// Directorio actual del cual se van a cargar imagenes.
        /// Al especificar un directorio se cargan todas las im�genes que haya en el mismo.
        /// </summary>
        public string CurrentDir
        {
            get { return browseDialog.SelectedPath; }
            set {
                //browseDialog.RootFolder = Environment.SpecialFolder.MyPictures;
                browseDialog.SelectedPath = value;
                loadImages(browseDialog.SelectedPath);
            }
        }

        /// <summary>
        /// Carga todas las im�genes del directorio en donde se encuentra la im�gen especificada,
        /// y la marca como la im�gen seleccionada de la ventana.
        /// </summary>
        public void setSelectedImage(string imagePath)
        {
            //Si no cambio la imagen seleccionada, evitamos hacer la carga de todas las imagenes del directorio
            if (selectedImage != null && imagePath == selectedImage.filePath)
            {
                return;
            }

            //Cargar imagenes de ese directorio
            this.CurrentDir = imagePath.Substring(0, imagePath.LastIndexOf("\\"));

            //Buscar la imagen pedida para seleccionarla, si es que est�
            foreach (ImageControl imgControl in panelImages.Controls)
            {
                if (imgControl.filePath == imagePath)
                {
                    imgControl.selectImage();
                }
            }
        }

        /// <summary>
        /// Cargar imagenes del directorio especificado
        /// </summary>
        public void loadImages(string folderPath)
        {
            textBoxFolderPath.Text = browseDialog.SelectedPath;
            panelImages.Controls.Clear();

            if (Directory.Exists(folderPath))
            {
                //Cargar carpetas del directorio
                if (showFolders)
                {
                    //Obtener subdirectorios
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

                    //Crear control para cada directorio
                    foreach (string dir in dirs)
	                {
                        ImageControl imageControl = new ImageControl(dir, this, true);
                        panelImages.Controls.Add(imageControl);
                    }
                }

                //Cargar imagenes del directorio
                List<string> imageFiles = new List<string>();
                for (int i = 0; i < FILE_EXT_SEARCH.Length; i++)
                {
                    imageFiles.AddRange(Directory.GetFiles(folderPath, FILE_EXT_SEARCH[i], SearchOption.TopDirectoryOnly));
                }
                imageFiles.Sort();

                //Crear control para cada imagen
                for (int i = 0; i < imageFiles.Count; i++)
                {
                    string imageFile = imageFiles[i];

                    ImageControl imageControl = new ImageControl(imageFile, this, false);
                    panelImages.Controls.Add(imageControl);
                }

                //Seleccionar primer elemento
                if (panelImages.Controls.Count > 0)
                {
                    ((ImageControl)panelImages.Controls[0]).selectImage();
                }
            }

        }

         

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            DialogResult r = browseDialog.ShowDialog(this);
            if (r == DialogResult.OK)
            {
                textBoxFolderPath.Text = browseDialog.SelectedPath;
                loadImages(browseDialog.SelectedPath);
            }
        }

        private void TgcTextureBrowser_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.selectedImage != null && !this.selectedImage.isDirectory)
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
        public class ImageControl : FlowLayoutPanel
        {
            TgcTextureBrowser textureBrowser;
            public PictureBox pictureBox;
            public Label filenameLabel;
            public string filePath;
            public bool isDirectory;

            public ImageControl(string imageFile, TgcTextureBrowser textureBrowser, bool isDirectory)
            {
                this.filePath = imageFile;
                this.textureBrowser = textureBrowser;
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
                    pictureBox.Image = textureBrowser.pictureBoxDirIcon.Image;
                }
                //cargar imagen de archivo
                else
                {
                    pictureBox.Size = new Size(200, 200);
                    pictureBox.Image = Image.FromFile(imageFile);
                }
                this.Controls.Add(pictureBox);

                filenameLabel = new Label();
                filenameLabel.AutoSize = false;
                filenameLabel.Size = new Size(pictureBox.Width, 20);
                if (isDirectory)
                {
                    filenameLabel.Font = new Font(FontFamily.GenericSansSerif, 8,FontStyle.Bold);
                }
                string[] pathArray = imageFile.Split('\\');
                filenameLabel.Text = pathArray[pathArray.Length - 1];
                filenameLabel.Click += new EventHandler(filenameLabel_Click);
                filenameLabel.TextAlign = ContentAlignment.MiddleCenter;
                this.Controls.Add(filenameLabel);
            }

            /// <summary>
            /// Seleccionar imagen
            /// </summary>
            public void selectImage()
            {
                if (textureBrowser.selectedImage != null)
                {
                    textureBrowser.selectedImage.BackColor = Color.White;
                }

                textureBrowser.selectedImage = this;
                this.BackColor = Color.Yellow;
            }

            void pictureBox_DoubleClick(object sender, EventArgs e)
            {
                selectImage();

                //Si es directorio, entrar a esa carpeta y cargar su contenido
                if (this.isDirectory)
                {
                    textureBrowser.CurrentDir = filePath;
                }
                //Si es imagen, cerrar ventana de texturas
                else
                {
                    textureBrowser.Close();
                }
            }

            void filenameLabel_Click(object sender, EventArgs e)
            {
                selectImage();
            }

            void pictureBox_Click(object sender, EventArgs e)
            {
                selectImage();
            }

            void ImageControl_Click(object sender, EventArgs e)
            {
                selectImage();
            }
        }

        

        

        
    }
}