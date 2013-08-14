using Microsoft.DirectX;
using Examples.TerrainEditor.Vegetation;
using TgcViewer;

namespace Examples.TerrainEditor.Brushes.Vegetation
{
 
    public class VegetationBrush:VegetationPicker
    {

        private string vegetationName;

        /// <summary>
        /// Setea el nombre del mesh que le va a pedir a InstanceManager
        /// </summary>
        /// <param name="name"></param>
        public void setVegetation(string name)
        {
            vegetationName = name;
            removeFloatingVegetation();
        }

      

        public override bool mouseMove(TgcTerrainEditor editor)
        {
            if (mesh == null) 
                mesh = InstancesManager.Instance.newMeshInstanceOf(vegetationName);
            Vector3 pos = Position;
            base.mouseMove(editor);
            if (pos == Position) Enabled = false;
            return false;
        }


        protected override void addVegetation(TgcTerrainEditor editor)
        {
            Vector3 scale = mesh.Scale;
            Vector3 rotation = mesh.Rotation;
            base.addVegetation(editor);

            mesh = InstancesManager.Instance.newMeshInstanceOf(vegetationName);
            mesh.Scale = scale;
            mesh.Rotation = rotation;
            mesh.Position = Position;

        } 
       
    }
}
