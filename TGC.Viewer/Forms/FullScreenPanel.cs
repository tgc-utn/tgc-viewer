using System.Windows.Forms;

namespace TGC.Viewer.Forms
{
    /// <summary>
    ///     Panel para modo FullScreen
    /// </summary>
    public partial class FullScreenPanel : Form
    {
        public FullScreenPanel()
        {
            InitializeComponent();

            var screenRect = Screen.FromControl(this).WorkingArea;
            var fullScreenSize = screenRect.Size;
            Size = fullScreenSize;
            Location = screenRect.Location;
            MaximumSize = fullScreenSize;
            MinimumSize = fullScreenSize;
        }

        private void FullScreenPanel_FormClosed(object sender, FormClosedEventArgs e)
        {
            //FIXME esto no deberia ser asi pero como no esta escuchando esc para cerrar cuando se cierra esta ventana el Viewer muere jeje
            Application.Exit();
        }
    }
}