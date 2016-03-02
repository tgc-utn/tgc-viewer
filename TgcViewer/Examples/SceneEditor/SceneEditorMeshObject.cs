using TGC.Core.SceneLoader;

namespace TGC.Examples.SceneEditor
{
    /// <summary>
    ///     Representa un modelo del escenario
    /// </summary>
    public class SceneEditorMeshObject
    {
        public string fileName;
        public string folderName;
        public int groupIndex;
        public int index;
        public TgcMesh mesh;
        public string name;
        public string userInfo;
        public bool visible;
    }
}