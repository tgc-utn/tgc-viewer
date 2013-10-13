using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.MeshCreator.Primitives
{
    /// <summary>
    /// Primitiva generica para construir objetos
    /// </summary>
    public abstract class EditorPrimitive
    {
        public static int PRIMITIVE_COUNT = 1;

        protected bool selected;
        /// <summary>
        /// Indica si esta seleccionado
        /// </summary>
        public bool Selected
        {
            get { return selected; }
        }

        string name;
        /// <summary>
        /// Nombre
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        string layer;
        /// <summary>
        /// Layer
        /// </summary>
        public string Layer
        {
            get { return layer; }
            set { layer = value; }
        }

        Dictionary<string, string> userProperties;
        /// <summary>
        /// User props
        /// </summary>
        public Dictionary<string, string> UserProperties
        {
            get { return userProperties; }
            set { userProperties = value; }
        }

        /// <summary>
        /// AlphaBlending
        /// </summary>
        public abstract bool AlphaBlendEnable
        {
            get;
            set;
        }

        /// <summary>
        /// BoundingBox
        /// </summary>
        public abstract TgcBoundingBox BoundingBox
        {
            get;
        }


        MeshCreatorControl control;
        /// <summary>
        /// Control
        /// </summary>
        public MeshCreatorControl Control
        {
            get { return control; }
        }

        protected bool visible;
        /// <summary>
        /// Indica si esta visible
        /// </summary>
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        /// <summary>
        /// Estructura que indica que cosas se pueden alterar de esta primitiva
        /// en el panel de Modify.
        /// Por default todo activado.
        /// </summary>
        public class ModifyCapabilities
        {
            public bool ChangeTexture = true;
            public bool ChangeOffsetUV = true;
            public bool ChangeTilingUV = true;
            public bool ChangePosition = true;
            public bool ChangeRotation = true;
            public bool ChangeScale = true;
            public int TextureNumbers = 1;
        }

        ModifyCapabilities modifyCaps;
        /// <summary>
        /// Capacidades de edición en el panel de Modify
        /// </summary>
        public ModifyCapabilities ModifyCaps
        {
            get { return modifyCaps; }
        }

        public EditorPrimitive(MeshCreatorControl control)
        {
            this.control = control;
            this.selected = false;
            this.userProperties = new Dictionary<string, string>();
            this.layer = control.CurrentLayer;
            this.modifyCaps = new ModifyCapabilities();
            this.visible = true;
        }


        /// <summary>
        /// Selecciona o deselecciona el objeto
        /// </summary>
        public abstract void setSelected(bool selected);

        /// <summary>
        /// Dibujar primitiva ya creada
        /// </summary>
        public abstract void render();


        /// <summary>
        /// Liberar recursos
        /// </summary>
        public abstract void dispose();


        /// <summary>
        /// Iniciar creacion de primitiva
        /// </summary>
        public abstract void initCreation(Vector3 gridPoint);

        /// <summary>
        /// Ejecutar la creacion de la primitiva
        /// </summary>
        public abstract void doCreation();

        /// <summary>
        /// Mover objeto
        /// </summary>
        public abstract void move(Vector3 move);

        /// <summary>
        /// Cambiar textura del objeto
        /// </summary>
        public abstract void setTexture(TgcTexture texture, int slot);

        /// <summary>
        /// Obtener textura del objeto
        /// </summary>
        public abstract TgcTexture getTexture(int slot);

        /// <summary>
        /// Offset de la textura del objeto
        /// </summary>
        public abstract Vector2 TextureOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Tiling de la textura del objeto
        /// </summary>
        public abstract Vector2 TextureTiling
        {
            get;
            set;
        }

        /// <summary>
        /// Posicion del objeto
        /// </summary>
        public abstract Vector3 Position
        {
            get;
            set;
        }

        /// <summary>
        /// Rotacion del objeto
        /// </summary>
        public abstract Vector3 Rotation
        {
            get;
        }

        /// <summary>
        /// Aplicar rotacion absoluta del objeto respecto de un pivote
        /// </summary>
        public abstract void setRotationFromPivot(Vector3 rotation, Vector3 pivot);

        /// <summary>
        /// Escala del objeto. Viene de la forma (1, 1, 1)
        /// </summary>
        public abstract Vector3 Scale
        {
            get;
            set;
        }

        /// <summary>
        /// Crear un TgcMesh para exportar la escena
        /// </summary>
        public abstract TgcMesh createMeshToExport();

        /// <summary>
        /// Clonar primitiva
        /// </summary>
        public abstract EditorPrimitive clone();

        /// <summary>
        /// Actualizar el AABB
        /// </summary>
        public abstract void updateBoundingBox();

    }
}
