using TgcViewer.Utils.Modifiers;


namespace Examples.TerrainEditor.Panel
{
    public class TerrainEditorModifier:TgcModifierPanel
    {
        TerrainEditorControl control;
        public TerrainEditorControl Control
        {
            get { return control; }
        }

        public TerrainEditorModifier(string varName, TgcTerrainEditor creator)
            : base(varName)
        {
            control = new TerrainEditorControl(creator);
            contentPanel.Controls.Add(control);
        }


        public override object getValue()
        {
            return null;
        }
    }
}
