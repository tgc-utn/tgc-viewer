using System.Collections.Generic;
using TGC.Util.Modifiers;

namespace TGC.Examples.RoomsEditor
{
    public class RoomsEditorModifier : TgcModifierPanel
    {
        //private TgcRoomsEditor editor;

        public RoomsEditorModifier(string varName, TgcRoomsEditor editor)
            : base(varName)
        {
            EditorControl = new RoomsEditorControl(editor);
            contentPanel.Controls.Add(EditorControl);
        }

        public RoomsEditorControl EditorControl { get; }

        /// <summary>
        ///     Rooms creados
        /// </summary>
        public List<RoomsEditorRoom> Rooms
        {
            get { return EditorControl.Rooms; }
        }

        public override object getValue()
        {
            return null;
        }

        public void dispose()
        {
            foreach (var room in Rooms)
            {
                room.dispose();
            }
        }
    }
}