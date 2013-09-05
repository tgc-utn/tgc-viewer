using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.Modifiers;

namespace Examples.MeshCreator
{
    /// <summary>
    /// Modifier customizado que se utiliza en el MeshCreator
    /// </summary>
    public class MeshCreatorModifier : TgcModifierPanel
    {

        MeshCreatorControl control;
        public MeshCreatorControl Control
        {
            get { return control; }
        }

        public MeshCreatorModifier(string varName, TgcMeshCreator creator)
            : base(varName)
        {
            control = new MeshCreatorControl(creator);
            contentPanel.Controls.Add(control);
        }


        public override object getValue()
        {
            return null;
        }

        public void dispose()
        {
            control.close();
        }
    }
}
