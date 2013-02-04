using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace TgcViewer.Utils.Modifiers
{
    /// <summary>
    /// Modificador para elegir un archivo del FileSystem
    /// </summary>
    public class TgcFileModifier : TgcModifierPanel
    {

        TextBox fileName;
        TextBox fileTextbox;
        Button fileButton;
        OpenFileDialog openDialog;
        string defaultPath;

        public TgcFileModifier(string varName, string defaultPath, string fileFilter)
            : base(varName)
        {
            this.defaultPath = defaultPath;

            fileName = new TextBox();
            fileName.ReadOnly = true;
            fileName.Margin = new Padding(0);
            fileName.Size = new Size(150, 10);
            fileName.Text = new FileInfo(defaultPath).Name;

            fileTextbox = new TextBox();
            fileTextbox.ReadOnly = true;
            fileTextbox.Margin = new Padding(0);
            fileTextbox.Size = new Size(150, 10);
            fileTextbox.Text = defaultPath;

            fileButton = new Button();
            fileButton.Margin = new Padding(0);
            fileButton.Size = new Size(30, 20);
            fileButton.Text = "F";
            fileButton.Click += new EventHandler(this.fileButton_click);

            contentPanel.Controls.Add(fileName);
            contentPanel.Controls.Add(fileTextbox);
            contentPanel.Controls.Add(fileButton);

            openDialog = new OpenFileDialog();
            openDialog.CheckFileExists = true;
            openDialog.Title = "Seleccionar archivo";
            openDialog.Filter = fileFilter;
            openDialog.Multiselect = false;
            openDialog.FileName = defaultPath;

        }

        private void fileButton_click(object sender, EventArgs e)
        {
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                fileTextbox.Text = openDialog.FileName;
            }
            else
            {
                fileTextbox.Text = defaultPath;
            }
            GuiController.Instance.focus3dPanel();
        }

        public override object getValue()
        {
            return (string)fileTextbox.Text;
        }
    }
}
