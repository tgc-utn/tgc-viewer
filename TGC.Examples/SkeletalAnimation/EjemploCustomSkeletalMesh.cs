using Microsoft.DirectX.Direct3D;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.SkeletalAnimation;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.SkeletalAnimation
{
    /// <summary>
    ///     Ejemplo EjemploCustomSkeletalMesh:
    ///     Unidades Involucradas:
    ///     # Unidad 5 - Animación - Skeletal Animation
    ///     Muestra como extender la clase TgcSkeletalMesh para agregarle comportamiento personalizado.
    ///     En este ejemplo se redefine el método executeRender() de TgcSkeletalMesh para renderizar
    ///     el modelo en Wireframe.
    ///     Autor: Leandro Barbagallo, Matías Leone
    /// </summary>
    public class EjemploCustomSkeletalMesh : TGCExampleViewer
    {
        private MyCustomMesh mesh;

        public EjemploCustomSkeletalMesh(string mediaDir, string shadersDir, TgcUserVars userVars,
            TgcModifiers modifiers) : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "SkeletalAnimation";
            Name = "CustomMesh";
            Description =
                "Muestra como extender la clase TgcSkeletalMesh para agregarle comportamiento personalizado. En este ejemplo se renderiza en Wireframe.";
        }

        public override void Init()
        {
            //Crear loader
            var loader = new TgcSkeletalLoader();

            //Configurar MeshFactory customizado
            loader.MeshFactory = new MyCustomMeshFactory();

            //Cargar modelo con una animación
            var pathMesh = MediaDir + "SkeletalAnimations\\BasicHuman\\WomanJeans-TgcSkeletalMesh.xml";
            string[] animationsPath =
            {
                MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\Push-TgcSkeletalAnim.xml"
            };
            mesh = (MyCustomMesh)loader.loadMeshAndAnimationsFromFile(pathMesh, animationsPath);

            //Ejecutar animacion
            mesh.playAnimation("Push");

            //Centrar camara rotacional respecto a este mesh
            Camara = new TgcRotationalCamera(mesh.BoundingBox.calculateBoxCenter(),
                mesh.BoundingBox.calculateBoxRadius() * 2);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            mesh.animateAndRender(ElapsedTime);

            PostRender();
        }

        public override void Dispose()
        {
            mesh.dispose();
        }
    }

    /// <summary>
    ///     Mesh customizado de ejemplo. Lo único que altera es que renderiza en Wireframe en lugar de renderizar en modo
    ///     solido.
    ///     Extiende de TgcSkeletalMesh. Implementa la interfaz IRenderQueueElement para poder redefinir el metodo
    ///     executeRender().
    ///     Tiene que tener el mismo constructor que tiene la clase TgcSkeletalMesh
    /// </summary>
    public class MyCustomMesh : TgcSkeletalMesh
    {
        /// <summary>
        ///     Primer constructor de TgcSkeletalMesh.
        ///     No se hace nada, solo se llama al constructor del padre.
        /// </summary>
        public MyCustomMesh(Mesh mesh, string name, MeshRenderType renderType, TgcSkeletalBone[] bones)
            : base(mesh, name, renderType, bones)
        {
        }

        /// <summary>
        ///     Se redefine tal cual, para que llame a nuestro render
        /// </summary>
        public new void animateAndRender(float elapsedTime)
        {
            if (!enabled)
                return;

            updateAnimation(elapsedTime);
            render();
        }

        /// <summary>
        ///     Se redefine este método para customizar el renderizado de este modelo.
        ///     Se agrega la palabra "new" al método para indiciar que está redefinido.
        /// </summary>
        public new void render()
        {
            //Cambiamos a modo WireFrame
            D3DDevice.Instance.Device.RenderState.FillMode = FillMode.WireFrame;

            //Llamamos al metodo original del padre

            //Restrablecemos modo solido
            D3DDevice.Instance.Device.RenderState.FillMode = FillMode.Solid;
        }
    }

    /// <summary>
    ///     Factory customizado que crea una instancia de la clase MyCustomMesh.
    ///     Debe implementar la interfaz TgcSceneLoader.IMeshFactory
    ///     En el método se crea una instancia de MyCustomMesh.
    /// </summary>
    public class MyCustomMeshFactory : TgcSkeletalLoader.IMeshFactory
    {
        public TgcSkeletalMesh createNewMesh(Mesh d3dMesh, string meshName, TgcSkeletalMesh.MeshRenderType renderType,
            TgcSkeletalBone[] bones)
        {
            return new MyCustomMesh(d3dMesh, meshName, renderType, bones);
        }
    }
}