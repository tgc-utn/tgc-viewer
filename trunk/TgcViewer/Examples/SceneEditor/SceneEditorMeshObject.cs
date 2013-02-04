using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.SceneEditor
{
    /// <summary>
    /// Representa un modelo del escenario
    /// </summary>
    public class SceneEditorMeshObject
    {
        public TgcMesh mesh;
        public string name;
        public string userInfo;
        public int index;
        public int groupIndex;
        public string fileName;
        public string folderName;
        public bool visible;
    }
}
