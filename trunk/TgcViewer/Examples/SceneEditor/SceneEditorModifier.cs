using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.Modifiers;

namespace Examples.SceneEditor
{
    /// <summary>
    /// Modifier customizado que se utiliza en el SceneEditor
    /// </summary>
    public class SceneEditorModifier : TgcModifierPanel
    {
        TgcSceneEditor editor;

        SceneEditorControl editorControl;
        public SceneEditorControl EditorControl
        {
            get { return editorControl; }
        }

        public SceneEditorModifier(string varName, TgcSceneEditor editor)
            : base(varName)
        {
            editorControl = new SceneEditorControl(editor);
            contentPanel.Controls.Add(editorControl);
        }


        public override object getValue()
        {
            return null;
        }
    }
}
