using System;
using System.Drawing;
using System.Windows.Forms;

namespace TGC.Examples.UserControls.Modifier
{
    /// <summary>
    ///     Modificador para elegir una textura
    /// </summary>
    public partial class TGCTextureModifier : UserControl
    {
        private TGCTextureModifier()
        {
            InitializeComponent();
        }

        public TGCTextureModifier(string modifierName, string defaultPath) : this()
        {
            tgcModifierTitleBar.setModifierName(modifierName);
            tgcModifierTitleBar.setContentPanel(contentPanel);

            DefaultPath = defaultPath;
            SelectedPath = defaultPath;
            textureBox.Image = getImage(defaultPath);
            TextureBrowser = new TgcTextureBrowser(defaultPath);
            TextureBrowser.setSelectedImage(defaultPath);
        }

        private string DefaultPath { get; }
        private string SelectedPath { get; set; }
        private TgcTextureBrowser TextureBrowser { get; }
        public string Value => SelectedPath;

        /// <summary>
        ///     Obtener la imagen pedida o devolver null
        /// </summary>
        private Image getImage(string path)
        {
            try
            {
                return Image.FromFile(path);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void textureBox_Click(object sender, EventArgs e)
        {
            if (TextureBrowser.ShowDialog() == DialogResult.OK)
            {
                var img = getImage(TextureBrowser.SelectedImage);
                SelectedPath = TextureBrowser.SelectedImage;
                textureBox.Image = img;
            }
            else
            {
                SelectedPath = DefaultPath;
                textureBox.Image = getImage(DefaultPath);
            }
        }
    }
}