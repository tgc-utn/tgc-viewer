using Microsoft.DirectX.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.SkeletalAnimation;

namespace TGC.Core.MeshFactory
{
    public interface ISkeletalMeshFactory
    {
        /// <summary>
        ///     Crear una nueva instancia de la clase TgcMesh o derivados
        /// </summary>
        /// <param name="d3DMesh">Mesh de Direct3D</param>
        /// <param name="meshName">Nombre de la malla</param>
        /// <param name="renderType">Tipo de renderizado de la malla</param>
        /// <returns>Instancia de TgcMesh creada</returns>
        TgcMesh createNewMesh(Mesh d3DMesh, string meshName, TgcMesh.MeshRenderType renderType);

        /// <summary>
        ///     Crear una nueva instancia de la clase TgcSkeletalMesh o derivados
        /// </summary>
        /// <param name="d3DMesh">Mesh de Direct3D</param>
        /// <param name="meshName">Nombre de la malla</param>
        /// <param name="renderType">Tipo de renderizado de la malla</param>
        /// <param name="bones">Huesos de la malla</param>
        /// <returns>Instancia de TgcMesh creada</returns>
        TgcSkeletalMesh createNewSkeletalMesh(Mesh d3DMesh, string meshName, TgcSkeletalMesh.MeshRenderType renderType,
            TgcSkeletalBone[] bones);
    }
}
