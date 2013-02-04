using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TgcViewer.Utils.Ui
{
    /// <summary>
    /// Panel para modo FullScreen
    /// </summary>
    public partial class FullScreenPanel : Form
    {
        public FullScreenPanel()
        {
            InitializeComponent();

            Rectangle screenRect = Screen.FromControl(this).WorkingArea;
            Size fullScreenSize = screenRect.Size;
            this.Size = fullScreenSize;
            this.Location = screenRect.Location;
            this.MaximumSize = fullScreenSize;
            this.MinimumSize = fullScreenSize;
        }
    }
}