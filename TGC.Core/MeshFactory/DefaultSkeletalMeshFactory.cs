using Microsoft.DirectX.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.SkeletalAnimation;

namespace TGC.Core.MeshFactory
{
    /// <summary>
    ///     Factory default que crea una instancia de la clase TgcMesh
    /// </summary>
    public class DefaultSkeletalMeshFactory: ISkeletalMeshFactory
    {
        public TgcMesh createNewMesh(Mesh d3dMesh, string meshName, TgcMesh.MeshRenderType renderType)
        {
            return new TgcMesh(d3dMesh, meshName, renderType);
        }

        public TgcSkeletalMesh createNewSkeletalMesh(Mesh d3dMesh, string meshName,
            TgcSkeletalMesh.MeshRenderType renderType, TgcSkeletalBone[] bones)
        {
            return new TgcSkeletalMesh(d3dMesh, meshName, renderType, bones);
        }
    }
}
