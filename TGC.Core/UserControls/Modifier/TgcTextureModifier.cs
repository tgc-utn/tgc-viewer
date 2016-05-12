using System;
using System.Drawing;
using System.Windows.Forms;

namespace TGC.Core.UserControls.Modifier
{
    /// <summary>
    ///     Modificador para elegir una textura
    /// </summary>
    public class TgcTextureModifier : TgcModifierPanel
    {
        private readonly string defaultPath;
        private readonly PictureBox textureBox;
        private readonly TgcTextureBrowser textureBrowser = new TgcTextureBrowser();
        private string selectedPath;

        public TgcTextureModifier(string varName, string defaultPath)
            : base(varName)
        {
            this.defaultPath = defaultPath;
            selectedPath = defaultPath;

            textureBox = new PictureBox();
            textureBox.Margin = new Padding(0);
            textureBox.Size = new Size(100, 100);
            textureBox.Image = getImage(defaultPath);
            textureBox.BorderStyle = BorderStyle.FixedSingle;
            textureBox.SizeMode = PictureBoxSizeMode.Zoom;
            textureBox.Click += textureButton_click;

            contentPanel.Controls.Add(textureBox);

            textureBrowser.setSelectedImage(defaultPath);
        }

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

        private void textureButton_click(object sender, EventArgs e)
        {
            if (textureBrowser.ShowDialog() == DialogResult.OK)
            {
                var img = getImage(textureBrowser.SelectedImage);
                selectedPath = textureBrowser.SelectedImage;
                textureBox.Image = img;
            }
            else
            {
                selectedPath = defaultPath;
                textureBox.Image = getImage(defaultPath);
            }
        }

        public override object getValue()
        {
            return selectedPath;
        }
    }
}