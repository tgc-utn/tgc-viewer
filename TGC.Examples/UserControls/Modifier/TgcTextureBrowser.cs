using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TGC.Examples.UserControls.Modifier
{
    /// <summary>
    ///     Ventana visualizadora de im�genes de un directorio.
    ///     Ideal para seleccionar texturas de un directorio.
    /// </summary>
    public partial class TgcTextureBrowser : Form
    {
        private PictureBox pictureBoxDirIcon;

        /// <summary>
        ///     Delegate para cuando se cierra el popup (para AsyncModeEnable = true)
        /// </summary>
        public delegate void CloseHandler(TgcTextureBrowser textureBrowser);

        /// <summary>
        ///     Delegate para cuando seleccionan una imagen (para AsyncModeEnable = true)
        /// </summary>
        public delegate void SelectImageHandler(TgcTextureBrowser textureBrowser);

        private readonly FolderBrowserDialog browseDialog;

        /// <summary>
        ///     Extensiones soportadas
        /// </summary>
        private readonly string[] FILE_EXT_SEARCH = { "*.jpg", "*.jpeg", "*.gif", "*.png", "*.bmp" };

        private ImageControl selectedImage;

        public TgcTextureBrowser(string homeDirPath)
        {
            InitializeComponent();

            pictureBoxDirIcon = new PictureBox();
            pictureBoxDirIcon.Image = Properties.Resources.folder;
            pictureBoxDirIcon.Name = "pictureBoxDirIcon";
            pictureBoxDirIcon.Size = new Size(192, 197);
            pictureBoxDirIcon.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxDirIcon.TabStop = false;
            pictureBoxDirIcon.Visible = false;

            browseDialog = new FolderBrowserDialog();
            browseDialog.Description = "Select folder";
            browseDialog.ShowNewFolderButton = false;
            ShowFolders = true;
            HomeDirPath = HomeDirPath;
            //+ "MeshCreator\\Textures";
            AsyncModeEnable = false;
            OnSelectImage = null;
            OnClose = null;
        }

        /// <summary>
        ///     En True carga tambien las carpetas de un directorio y permite navegar por ellas
        /// </summary>
        public bool ShowFolders { get; set; }

        /// <summary>
        ///     Path del directorio Home
        /// </summary>
        public string HomeDirPath { get; set; }

        /// <summary>
        ///     En True el popup se prepara para ser usada como no-bloqueante
        ///     y usa el evento OnSelectImage para avisar que se hizo un cambio en la textura.
        ///     Por default es False
        /// </summary>
        public bool AsyncModeEnable { get; set; }

        /// <summary>
        ///     Path de la imagen seleccionada
        /// </summary>
        public string SelectedImage => selectedImage.filePath;

        /// <summary>
        ///     Directorio actual del cual se van a cargar imagenes.
        ///     Al especificar un directorio se cargan todas las im�genes que haya en el mismo.
        /// </summary>
        public string CurrentDir
        {
            get
            {
                return browseDialog.SelectedPath;
            }
            set
            {
                //browseDialog.RootFolder = Environment.SpecialFolder.MyPictures;
                browseDialog.SelectedPath = value;
                loadImages(browseDialog.SelectedPath);
            }
        }

        /// <summary>
        ///     Evento para cuando seleccionan una imagen (para AsyncModeEnable = true)
        /// </summary>
        public event SelectImageHandler OnSelectImage;

        /// <summary>
        ///     Evento para cuando se cierra el popup (para AsyncModeEnable = true)
        /// </summary>
        public event CloseHandler OnClose;

        /// <summary>
        ///     Carga todas las im�genes del directorio en donde se encuentra la im�gen especificada,
        ///     y la marca como la im�gen seleccionada de la ventana.
        /// </summary>
        public void setSelectedImage(string imagePath)
        {
            //Si no cambio la imagen seleccionada, evitamos hacer la carga de todas las imagenes del directorio
            if (selectedImage != null && imagePath == selectedImage.filePath) return;

            //Cargar imagenes de ese directorio
            CurrentDir = imagePath.Substring(0, imagePath.LastIndexOf("\\"));

            //Buscar la imagen pedida para seleccionarla, si es que est�
            foreach (ImageControl imgControl in panelImages.Controls)
                if (imgControl.filePath == imagePath)
                {
                    imgControl.selectImage();
                    break;
                }
        }

        /// <summary>
        ///     Cargar imagenes del directorio especificado
        /// </summary>
        public void loadImages(string folderPath)
        {
            toolStripTextBoxPath.Text = browseDialog.SelectedPath;

            //Limpiar controles anteriores
            foreach (ImageControl c in panelImages.Controls) c.dispose();
            panelImages.Controls.Clear();

            if (Directory.Exists(folderPath))
            {
                //Cargar carpetas del directorio
                if (ShowFolders)
                {
                    //Obtener subdirectorios
                    var allDirs = Directory.GetDirectories(folderPath);

                    //Excluir subdirectorios innecesarios
                    var dirs = new List<string>();
                    for (var i = 0; i < allDirs.Length; i++)
                        if (!allDirs[i].Contains(".svn"))
                            dirs.Add(allDirs[i]);

                    //Crear control para cada directorio
                    foreach (var dir in dirs)
                    {
                        var imageControl = new ImageControl(dir, this, true);
                        panelImages.Controls.Add(imageControl);
                    }
                }

                //Cargar imagenes del directorio
                var imageFiles = new List<string>();
                for (var i = 0; i < FILE_EXT_SEARCH.Length; i++)
                    imageFiles.AddRange(Directory.GetFiles(folderPath, FILE_EXT_SEARCH[i],
                        SearchOption.TopDirectoryOnly));
                imageFiles.Sort();

                //Crear control para cada imagen
                for (var i = 0; i < imageFiles.Count; i++)
                {
                    var imageFile = imageFiles[i];

                    var imageControl = new ImageControl(imageFile, this, false);
                    panelImages.Controls.Add(imageControl);
                }

                //Seleccionar primer elemento
                if (panelImages.Controls.Count > 0) ((ImageControl)panelImages.Controls[0]).selectImage();
            }
        }

        private void TgcTextureBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (selectedImage != null && !selectedImage.isDirectory)
                DialogResult = DialogResult.OK;
            else
                DialogResult = DialogResult.Cancel;

            //En Async mode evitar cerrar el form, solo esconderlo
            if (AsyncModeEnable)
            {
                e.Cancel = true;
                Parent = null;
                Hide();

                //Avisar al evento de cierre
                if (OnClose != null) OnClose.Invoke(this);
            }
        }

        /// <summary>
        ///     Control para una imagen o directorio
        /// </summary>
        public class ImageControl : FlowLayoutPanel
        {
            private readonly TgcTextureBrowser textureBrowser;
            public Label filenameLabel;
            public string filePath;
            public bool isDirectory;
            public PictureBox pictureBox;

            public ImageControl(string imageFile, TgcTextureBrowser textureBrowser, bool isDirectory)
            {
                filePath = imageFile;
                this.textureBrowser = textureBrowser;
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
                    pictureBox.Size = new Size(128, 128);
                    pictureBox.Image = textureBrowser.pictureBoxDirIcon.Image;
                }
                //cargar imagen de archivo
                else
                {
                    pictureBox.Size = new Size(128, 128);
                    pictureBox.Image = Image.FromFile(imageFile);
                }

                Controls.Add(pictureBox);

                filenameLabel = new Label();
                filenameLabel.AutoSize = false;
                filenameLabel.Size = new Size(pictureBox.Width, 20);
                if (isDirectory) filenameLabel.Font = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
                var pathArray = imageFile.Split('\\');
                filenameLabel.Text = pathArray[pathArray.Length - 1];
                filenameLabel.Click += filenameLabel_Click;
                filenameLabel.TextAlign = ContentAlignment.MiddleCenter;
                Controls.Add(filenameLabel);
            }

            /// <summary>
            ///     Seleccionar imagen
            /// </summary>
            public void selectImage()
            {
                if (textureBrowser.selectedImage != null) textureBrowser.selectedImage.BackColor = Color.White;

                textureBrowser.selectedImage = this;
                BackColor = Color.Yellow;

                //Invocar evento de seleccion
                if (textureBrowser.AsyncModeEnable && textureBrowser.OnSelectImage != null && !isDirectory)
                    textureBrowser.OnSelectImage.Invoke(textureBrowser);
            }

            private void pictureBox_DoubleClick(object sender, EventArgs e)
            {
                selectImage();

                //Si es directorio, entrar a esa carpeta y cargar su contenido
                if (isDirectory)
                    textureBrowser.CurrentDir = filePath;
                //Si es imagen, cerrar ventana de texturas
                else
                    textureBrowser.Close();
            }

            private void filenameLabel_Click(object sender, EventArgs e)
            {
                selectImage();
            }

            private void pictureBox_Click(object sender, EventArgs e)
            {
                selectImage();
            }

            private void ImageControl_Click(object sender, EventArgs e)
            {
                selectImage();
            }

            public void dispose()
            {
                if (!isDirectory)
                    if (pictureBox.Image != null)
                        pictureBox.Image.Dispose();
            }
        }

        private void toolStripButtonUp_Click(object sender, EventArgs e)
        {
            var info = new FileInfo(CurrentDir);
            if (info.Directory != null) CurrentDir = info.Directory.FullName;
        }

        private void toolStripButtonHome_Click(object sender, EventArgs e)
        {
            CurrentDir = HomeDirPath;
        }

        private void toolStripButtonBrowse_Click(object sender, EventArgs e)
        {
            var r = browseDialog.ShowDialog(this);
            if (r == DialogResult.OK)
            {
                toolStripTextBoxPath.Text = browseDialog.SelectedPath;
                loadImages(browseDialog.SelectedPath);
            }
        }
    }
}