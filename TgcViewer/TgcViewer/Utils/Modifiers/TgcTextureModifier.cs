using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace TgcViewer.Utils.Modifiers
{
    /// <summary>
    /// Modificador para elegir una textura
    /// </summary>
    public class TgcTextureModifier : TgcModifierPanel
    {
        PictureBox textureBox;
        TgcTextureBrowser textureBrowser = new TgcTextureBrowser();
        string defaultPath;
        string selectedPath;

        public TgcTextureModifier(string varName, string defaultPath)
            : base(varName)
        {
            this.defaultPath = defaultPath;
            this.selectedPath = defaultPath;

            textureBox = new PictureBox();
            textureBox.Margin = new Padding(0);
            textureBox.Size = new Size(100, 100);
            textureBox.Image = getImage(defaultPath);
            textureBox.BorderStyle = BorderStyle.FixedSingle;
            textureBox.SizeMode = PictureBoxSizeMode.Zoom;
            textureBox.Click += new EventHandler(this.textureButton_click);

            contentPanel.Controls.Add(textureBox);

            textureBrowser.setSelectedImage(defaultPath);
        }

        /// <summary>
        /// Obtener la imagen pedida o devolver null
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
                Image img = getImage(textureBrowser.SelectedImage);
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
            return (string)selectedPath;
        }
    }
}
