using System;
using System.Collections.Generic;
using System.Text;

namespace TgcViewer.Utils.TgcSceneLoader
{

    public class TgcMaterialData
    {
        //Material Types
        public static readonly string StandardMaterial = "Standardmaterial";
        public static readonly string MultiMaterial = "Multimaterial";


        public string name;
        public string type;

        //Submaterials
        public TgcMaterialData[] subMaterials;
            
        //Material
        public float[] ambientColor;
        public float[] diffuseColor;
        public float[] specularColor;
        public float opacity;
        public bool alphaBlendEnable;

        //Bitmap
        public string fileName;
        public float[] uvTiling;
        public float[] uvOffset;

    }
}
