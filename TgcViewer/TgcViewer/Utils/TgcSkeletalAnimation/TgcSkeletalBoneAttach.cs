using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX;

namespace TgcViewer.Utils.TgcSkeletalAnimation
{
    /// <summary>
    /// Representa un modelo que se adjunta a un hueso del esqueleto, para que se modifique
    /// su ubicicación en el espacio en base a las transformaciones del hueso durante la animación.
    /// El modelo no debe ser transformado por afuera una vez que es adjuntado a un hueso.
    /// El renderizado del modelo debe hacerse por afuera de la animación esquelética.
    /// </summary>
    public class TgcSkeletalBoneAttach
    {

        private TgcMesh mesh;
        /// <summary>
        /// Modelo que se adjunta al hueso
        /// </summary>
        public TgcMesh Mesh
        {
            get { return mesh; }
            set { mesh = value; }
        }

        private TgcSkeletalBone bone;
        /// <summary>
        /// Hueso al cual se le adjunta un modelo
        /// </summary>
        public TgcSkeletalBone Bone
        {
            get { return bone; }
            set { bone = value; }
        }

        private Matrix offset;
        /// <summary>
        /// Desplazamiento desde el cual el modelo sigue al hueso
        /// </summary>
        public Matrix Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        /// <summary>
        /// Crear un modelo adjunto a un hueso, vacio
        /// </summary>
        public TgcSkeletalBoneAttach()
        {
        }

        /// <summary>
        /// Crear un modelo adjunto a un hueso
        /// </summary>
        /// <param name="model">Modelo a adjuntar</param>
        /// <param name="bone">Hueso al cual adjuntarse</param>
        /// <param name="offset">Offset desde el cual el modelo sigue al hueso</param>
        public TgcSkeletalBoneAttach(TgcMesh mesh, TgcSkeletalBone bone, Matrix offset)
        {
            this.bone = bone;
            this.mesh = mesh;
            this.offset = offset;
            updateValues();
        }

        /// <summary>
        /// Configurar modelo
        /// </summary>
        private void setMesh(TgcMesh mesh)
        {
            this.mesh = mesh;
            this.mesh.AutoTransformEnable = false;
            this.mesh.Transform = bone.MatFinal;
        }

        /// <summary>
        /// Actualiza los valores del Attachment en base a los parámetros configurados.
        /// Debe ejecutarse para que las modificaciones realmente se utilicen.
        /// </summary>
        public void updateValues()
        {
            this.mesh.AutoTransformEnable = false;
            updateMeshTransform(Matrix.Identity);
        }

        /// <summary>
        /// Actualiza la transformacion del modelo en base al a transformacion actual
        /// del hueso y el offset configurado
        /// </summary>
        internal void updateMeshTransform(Matrix meshTransform)
        {
            this.mesh.Transform = offset * bone.MatFinal * meshTransform;
        }


        

    }
}
