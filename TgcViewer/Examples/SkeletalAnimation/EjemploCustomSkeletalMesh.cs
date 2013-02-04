using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.SkeletalAnimation
{
    /// <summary>
    /// Ejemplo EjemploCustomSkeletalMesh:
    /// Unidades Involucradas:
    ///     # Unidad 5 - Animación - Skeletal Animation
    /// 
    /// Muestra como extender la clase TgcSkeletalMesh para agregarle comportamiento personalizado.
    /// En este ejemplo se redefine el método executeRender() de TgcSkeletalMesh para renderizar
    /// el modelo en Wireframe.
    /// 
    /// 
    /// Autor: Leandro Barbagallo, Matías Leone
    /// 
    /// </summary>
    public class EjemploCustomSkeletalMesh : TgcExample
    {
        MyCustomMesh mesh;

        public override string getCategory()
        {
            return "SkeletalAnimation";
        }

        public override string getName()
        {
            return "CustomMesh";
        }

        public override string getDescription()
        {
            return "Muestra como extender la clase TgcSkeletalMesh para agregarle comportamiento personalizado. En este ejemplo se renderiza en Wireframe";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear loader
            TgcSkeletalLoader loader = new TgcSkeletalLoader();

            //Configurar MeshFactory customizado
            loader.MeshFactory = new MyCustomMeshFactory();

            //Cargar modelo con una animación
            string pathMesh = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\WomanJeans-TgcSkeletalMesh.xml";
            string[] animationsPath = new string[] { GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\Push-TgcSkeletalAnim.xml" };
            mesh = (MyCustomMesh)loader.loadMeshAndAnimationsFromFile(pathMesh, animationsPath);

            //Ejecutar animacion
            mesh.playAnimation("Push");

            //Centrar camara rotacional respecto a este mesh
            GuiController.Instance.RotCamera.targetObject(mesh.BoundingBox);
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            mesh.animateAndRender();

        }

        public override void close()
        {
            mesh.dispose();
        }

    }

    /// <summary>
    /// Mesh customizado de ejemplo. Lo único que altera es que renderiza en Wireframe en lugar de renderizar en modo solido.
    /// Extiende de TgcSkeletalMesh. Implementa la interfaz IRenderQueueElement para poder redefinir el metodo executeRender().
    /// Tiene que tener el mismo constructor que tiene la clase TgcSkeletalMesh
    /// 
    /// </summary>
    public class MyCustomMesh : TgcSkeletalMesh
    {
        /// <summary>
        /// Primer constructor de TgcSkeletalMesh.
        /// No se hace nada, solo se llama al constructor del padre.
        /// </summary>
        public MyCustomMesh(Mesh mesh, string name, MeshRenderType renderType, TgcSkeletalBone[] bones)
            : base(mesh, name, renderType, bones)
        {
        }

        /// <summary>
        /// Se redefine tal cual, para que llame a nuestro render
        /// </summary>
        public new void animateAndRender()
        {
            if (!enabled)
                return;

            updateAnimation();
            render();
        }

        /// <summary>
        /// Se redefine este método para customizar el renderizado de este modelo.
        /// Se agrega la palabra "new" al método para indiciar que está redefinido.
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
    /// En el método se crea una instancia de MyCustomMesh.
    /// </summary>
    public class MyCustomMeshFactory : TgcSkeletalLoader.IMeshFactory
    {
        public TgcSkeletalMesh createNewMesh(Mesh d3dMesh, string meshName, TgcSkeletalMesh.MeshRenderType renderType, TgcSkeletalBone[] bones)
        {
            return new MyCustomMesh(d3dMesh, meshName, renderType, bones);
        }
    }

}
