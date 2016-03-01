using System.Windows.Forms;

namespace TGC.Viewer.Utils.Ui
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
    }
}