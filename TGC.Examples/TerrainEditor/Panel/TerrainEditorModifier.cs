using TGC.Viewer.Utils.Modifiers;

namespace TGC.Examples.TerrainEditor.Panel
{
    public class TerrainEditorModifier : TgcModifierPanel
    {
        public TerrainEditorModifier(string varName, TgcTerrainEditor creator)
            : base(varName)
        {
            Control = new TerrainEditorControl(creator);
            contentPanel.Controls.Add(Control);
        }

        public TerrainEditorControl Control { get; }

        public override object getValue()
        {
            return null;
        }

        public void dispose()
        {
            Control.dispose();
        }
    }
}