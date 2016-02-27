namespace TGC.Core.SceneLoader
{
    public class TgcMaterialData
    {
        //Material Types
        public static readonly string StandardMaterial = "Standardmaterial";

        public static readonly string MultiMaterial = "Multimaterial";
        public bool alphaBlendEnable;

        //Material
        public float[] ambientColor;

        public float[] diffuseColor;

        //Bitmap
        public string fileName;

        public string name;
        public float opacity;
        public float[] specularColor;

        //Submaterials
        public TgcMaterialData[] subMaterials;
        public string type;
        public float[] uvOffset;

        public float[] uvTiling;
    }
}