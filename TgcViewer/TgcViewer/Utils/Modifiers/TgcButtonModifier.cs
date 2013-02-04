using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace TgcViewer.Utils.Modifiers
{
    /// <summary>
    /// Modificador con un Boton
    /// </summary>
    public class TgcButtonModifier : TgcModifierPanel
    {

        Button button;

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
