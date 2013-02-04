using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.SceneLoader
{
    /// <summary>
    /// Ejemplo EjemploCustomMesh:
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos B�sicos de 3D - Mesh
    /// 
    /// Muestra como extender la clase TgcMesh para agregarle comportamiento personalizado.
    /// En este ejemplo se redefine el m�todo executeRender() de TgcMesh para renderizar
    /// el modelo en Wireframe.
    /// 
    /// Autor: Mat�as Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploCustomMesh : TgcExample
    {
        MyCustomMesh mesh;

        public override string getCategory()
        {
            return "SceneLoader";
        }

        public override string getName()
        {
            return "CustomMesh";
        }

        public override string getDescription()
        {
            return "Muestra como extender la clase TgcMesh para agregarle comportamiento personalizado. En este ejemplo se renderiza en Wireframe";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            //Configurar MeshFactory customizado
            loader.MeshFactory = new MyCustomMeshFactory();

            //Cargar mesh
            TgcScene sceneOriginal = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\Buggy\\" + "Buggy-TgcScene.xml");
            mesh = (MyCustomMesh)sceneOriginal.Meshes[0];


            //Centrar camara rotacional respecto a este mesh
            GuiController.Instance.RotCamera.targetObject(mesh.BoundingBox);
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            mesh.render();
        }

        public override void close()
        {
            mesh.dispose();
        }

    }

    /// <summary>
    /// Mesh customizado de ejemplo. Lo �nico que altera es que renderiza en Wireframe en lugar de renderizar en modo solido.
    /// Extiende de TgcMesh. Implementa la interfaz IRenderQueueElement para poder redefinir el metodo executeRender().
    /// Tiene que tener los dos mismos constructores que tiene la clase TgcMesh
    /// 
    /// </summary>
    public class MyCustomMesh : TgcMesh
    {
        /// <summary>
        /// Primer constructor de TgcMesh.
        /// No se hace nada, solo se llama al constructor del padre.
        /// </summary>
        public MyCustomMesh(Mesh mesh, string name, MeshRenderType renderType) 
            : base(mesh, name, renderType)
        {
        }

        /// <summary>
        /// Segundo constructor de TgcMesh.
        /// No se hace nada, solo se llama al constructor del padre.
        /// </summary>
        public MyCustomMesh(string name, TgcMesh parentInstance, Vector3 translation, Vector3 rotation, Vector3 scale)
            : base(name, parentInstance, translation, rotation, scale)
        {
        }

        /// <summary>
        /// Se redefine este m�todo para customizar el renderizado de este modelo.
        /// Se agrega la palabra "new" al m�todo para indiciar que est� redefinido.
        /// </summary>
        public new void render()
        {
            Device device = GuiController.Instance.D3dDevice;

            //Cambiamos a modo WireFrame
            device.RenderState.FillMode = FillMode.WireFrame;

            //Llamamos al metodo original del padre
            base.render();

            //Restrablecemos modo solido
            device.RenderState.FillMode = FillMode.Solid;
        }

    }

    /// <summary>
    /// Factory customizado que crea una instancia de la clase MyCustomMesh.
    /// Debe implementar la interfaz TgcSceneLoader.IMeshFactory
    /// En ambos m�todos crea una instancia de MyCustomMesh.
    /// </summary>
    public class MyCustomMeshFactory : TgcSceneLoader.IMeshFactory
    {
        public TgcMesh createNewMesh(Mesh d3dMesh, string meshName, TgcMesh.MeshRenderType renderType)
        {
            return new MyCustomMesh(d3dMesh, meshName, renderType);
        }

        public TgcMesh createNewMeshInstance(string meshName, TgcMesh originalMesh, Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            return new MyCustomMesh(meshName, originalMesh, translation, rotation, scale);
        }
    }

}
