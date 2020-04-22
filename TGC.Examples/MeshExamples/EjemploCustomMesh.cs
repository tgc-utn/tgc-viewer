using Microsoft.DirectX.Direct3D;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.MeshFactory;
using TGC.Core.SceneLoader;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.MeshExamples
{
    /// <summary>
    ///     Ejemplo EjemploCustomMesh:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Muestra como extender la clase TgcMesh para agregarle comportamiento personalizado.
    ///     En este ejemplo se redefine el método executeRender() de TgcMesh para renderizar
    ///     el modelo en Wireframe.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploCustomMesh : TGCExampleViewer
    {
        private MyCustomMesh mesh;

        public EjemploCustomMesh(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Mesh Examples";
            Name = "Custom Mesh";
            Description =
                "Muestra como extender la clase TgcMesh para agregarle comportamiento personalizado. En este ejemplo se renderiza en Wireframe.";
        }

        public override void Init()
        {
            //Crear loader
            var loader = new TgcSceneLoader();

            //Configurar MeshFactory customizado
            loader.MeshFactory = new MyCustomMeshFactory();

            //Cargar mesh
            var sceneOriginal =
                loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\Buggy\\" + "Buggy-TgcScene.xml");
            mesh = (MyCustomMesh)sceneOriginal.Meshes[0];

            //Centrar camara rotacional respecto a este mesh
            Camera = new TgcRotationalCamera(mesh.BoundingBox.calculateBoxCenter(), mesh.BoundingBox.calculateBoxRadius() * 2, Input);
        }

        public override void Update()
        {
            //  Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones ante ellas.
        }

        public override void Render()
        {
            PreRender();

            mesh.Render();

            PostRender();
        }

        public override void Dispose()
        {
            mesh.Dispose();
        }
    }

    /// <summary>
    ///     Mesh customizado de ejemplo. Lo único que altera es que renderiza en Wireframe en lugar de renderizar en modo
    ///     solido.
    ///     Extiende de TgcMesh. Implementa la interfaz IRenderQueueElement para poder redefinir el metodo executeRender().
    ///     Tiene que tener los dos mismos constructores que tiene la clase TgcMesh
    /// </summary>
    public class MyCustomMesh : TgcMesh
    {
        /// <summary>
        ///     Primer constructor de TgcMesh.
        ///     No se hace nada, solo se llama al constructor del padre.
        /// </summary>
        public MyCustomMesh(Mesh mesh, string name, MeshRenderType renderType)
            : base(mesh, name, renderType)
        {
        }

        /// <summary>
        ///     Segundo constructor de TgcMesh.
        ///     No se hace nada, solo se llama al constructor del padre.
        /// </summary>
        public MyCustomMesh(string name, TgcMesh parentInstance, TGCVector3 translation, TGCVector3 rotation, TGCVector3 scale)
            : base(name, parentInstance, translation, rotation, scale)
        {
        }

        /// <summary>
        ///     Se redefine este método para customizar el renderizado de este modelo.
        ///     Se agrega la palabra "new" al método para indiciar que está redefinido.
        /// </summary>
        public new void Render()
        {
            //Cambiamos a modo WireFrame
            D3DDevice.Instance.Device.RenderState.FillMode = FillMode.WireFrame;

            //Llamamos al metodo original del padre
            base.Render();

            //Restrablecemos modo solido
            D3DDevice.Instance.Device.RenderState.FillMode = FillMode.Solid;
        }
    }

    /// <summary>
    ///     Factory customizado que crea una instancia de la clase MyCustomMesh.
    ///     Debe implementar la interfaz TgcSceneLoader.IMeshFactory
    ///     En ambos métodos crea una instancia de MyCustomMesh.
    /// </summary>
    public class MyCustomMeshFactory : IMeshFactory
    {
        public TgcMesh createNewMesh(Mesh d3dMesh, string meshName, TgcMesh.MeshRenderType renderType)
        {
            return new MyCustomMesh(d3dMesh, meshName, renderType);
        }

        public TgcMesh createNewMeshInstance(string meshName, TgcMesh originalMesh, TGCVector3 translation,
            TGCVector3 rotation, TGCVector3 scale)
        {
            return new MyCustomMesh(meshName, originalMesh, translation, rotation, scale);
        }
    }
}