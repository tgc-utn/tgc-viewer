using System;
using System.Windows.Forms;

namespace TGC.Viewer.Utils.Modifiers
{
    /// <summary>
    ///     Modificador con un Boton
    /// </summary>
    public class TgcButtonModifier : TgcModifierPanel
    {
        private readonly Button button;

        public TgcButtonModifier(string varName, string text, EventHandler clickEventHandler)
            : base(varName)
        {
            button = new Button();
            button.Text = text;
            button.Margin = new Padding(0);
            button.Click += clickEventHandler;

            contentPanel.Controls.Add(button);
        }

        public override object getValue()
        {
            throw new Exception("El TgcButtonModifier no soporta getValue()");
        }
    }
}