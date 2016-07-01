using TGC.Core.Camara;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Ejemplo EjemploBoundingBox:
    ///     Unidades Involucradas:
    ///     # Unidad 6 - Deteccion de Colisiones - BoundingBox
    ///     Carga un modelo 3D estatico mediante la herramienta TgcSceneLoader
    ///     y muestra como renderizar su BoundingBox.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploBoundingBox : TGCExampleViewer
    {
        private TgcMesh mesh;

        public EjemploBoundingBox(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Collision";
            Name = "BoundingBox";
            Description =
                "Carga un modelo 3D estatico mediante la herramienta TgcSceneLoader y muestra como renderizar su BoundingBox. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Cargar modelo estatico
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\Buggy\\Buggy-TgcScene.xml");
            mesh = scene.Meshes[0];

            //Alejar camara rotacional segun tamano del BoundingBox del objeto
            Camara = new TgcRotationalCamera(mesh.BoundingBox.calculateBoxCenter(),
                mesh.BoundingBox.calculateBoxRadius() * 2, Input);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Renderizar modelo
            mesh.render();

            //Renderizar BoundingBox
            mesh.BoundingBox.render();

            PostRender();
        }

        public override void Dispose()
        {
            mesh.dispose();
        }
    }
}