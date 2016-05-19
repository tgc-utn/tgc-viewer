using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.SceneLoader
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
    public class EjemploCustomMesh : TgcExample
    {
        private MyCustomMesh mesh;

        public EjemploCustomMesh(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "SceneLoader";
            Name = "CustomMesh";
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
            ((TgcRotationalCamera)Camara).targetObject(mesh.BoundingBox);
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            mesh.render();

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            mesh.dispose();
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
        public MyCustomMesh(string name, TgcMesh parentInstance, Vector3 translation, Vector3 rotation, Vector3 scale)
            : base(name, parentInstance, translation, rotation, scale)
        {
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
            base.render();

            //Restrablecemos modo solido
            D3DDevice.Instance.Device.RenderState.FillMode = FillMode.Solid;
        }
    }

    /// <summary>
    ///     Factory customizado que crea una instancia de la clase MyCustomMesh.
    ///     Debe implementar la interfaz TgcSceneLoader.IMeshFactory
    ///     En ambos métodos crea una instancia de MyCustomMesh.
    /// </summary>
    public class MyCustomMeshFactory : TgcSceneLoader.IMeshFactory
    {
        public TgcMesh createNewMesh(Mesh d3dMesh, string meshName, TgcMesh.MeshRenderType renderType)
        {
            return new MyCustomMesh(d3dMesh, meshName, renderType);
        }

        public TgcMesh createNewMeshInstance(string meshName, TgcMesh originalMesh, Vector3 translation,
            Vector3 rotation, Vector3 scale)
        {
            return new MyCustomMesh(meshName, originalMesh, translation, rotation, scale);
        }
    }
}