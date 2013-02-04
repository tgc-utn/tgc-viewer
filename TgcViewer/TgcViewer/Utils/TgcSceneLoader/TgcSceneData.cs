using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.PortalRendering;

namespace TgcViewer.Utils.TgcSceneLoader
{
    public class TgcSceneData
    {
        public string name;
        public string texturesDir;
        
        //Info de LightMaps
        public string lightmapsDir;
        public bool lightmapsEnabled;

        public TgcMeshData[] meshesData;
        public TgcMaterialData[] materialsData;

        //BoundingBox de la escena
        public float[] pMin;
        public float[] pMax;

        //Datos de PortalRendering
        public TgcPortalRenderingData portalData;
    }
}
