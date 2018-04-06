using Microsoft.DirectX.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Core.MeshFactory
{
    /// <summary>
    ///     Factory default que crea una instancia de la clase TgcMesh
    /// </summary>
    public class DefaultMeshFactory : IMeshFactory
    {
        public TgcMesh createNewMesh(Mesh d3dMesh, string meshName, TgcMesh.MeshRenderType renderType)
        {
            return new TgcMesh(d3dMesh, meshName, renderType);
        }

        public TgcMesh createNewMeshInstance(string meshName, TgcMesh originalMesh, TGCVector3 translation,
            TGCVector3 rotation, TGCVector3 scale)
        {
            return new TgcMesh(meshName, originalMesh, translation, rotation, scale);
        }
    }
}