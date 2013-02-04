using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.Modifiers;

namespace Examples.RoomsEditor
{
    public class RoomsEditorModifier : TgcModifierPanel
    {
        TgcRoomsEditor editor;

        RoomsEditorControl editorControl;
        public RoomsEditorControl EditorControl
        {
            get { return editorControl; }
        }

        /// <summary>
        /// Rooms creados
        /// </summary>
        public List<RoomsEditorRoom> Rooms
        {
            get { return editorControl.Rooms; }
        }

        public RoomsEditorModifier(string varName, TgcRoomsEditor editor)
            : base(varName)
        {
            editorControl = new RoomsEditorControl(editor);
            contentPanel.Controls.Add(editorControl);
        }


        public override object getValue()
        {
            return null;
        }


        public void dispose()
        {
            foreach (RoomsEditorRoom room in Rooms)
            {
                room.dispose();
            }
        }
    }
}
