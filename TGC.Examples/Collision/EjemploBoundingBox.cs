using System;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Ejemplo EjemploBoundingBox:
    ///     Unidades Involucradas:
    ///     # Unidad 6 - Detección de Colisiones - BoundingBox
    ///     Carga un modelo 3D estático mediante la herramienta TgcSceneLoader
    ///     y muestra como renderizar su BoundingBox.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploBoundingBox : TgcExample
    {
        private TgcMesh mesh;

        public EjemploBoundingBox(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Collision";
            Name = "BoundingBox";
            Description =
                "Carga un modelo 3D estático mediante la herramienta TgcSceneLoader y muestra como renderizar su BoundingBox. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Cargar modelo estatico
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\Buggy\\Buggy-TgcScene.xml");
            mesh = scene.Meshes[0];

            //Alejar camara rotacional segun tamaño del BoundingBox del objeto
            Camara = new TgcRotationalCamera(mesh.BoundingBox.calculateBoxCenter(), mesh.BoundingBox.calculateBoxRadius() * 2);
        }

        public override void Update()
        {
            base.helperPreUpdate();
        }

        public override void Render()
        {
            base.helperPreRender();
            

            //Renderizar modelo
            mesh.render();

            //Renderizar BoundingBox
            mesh.BoundingBox.render();

            helperPostRender();
        }

        public override void Dispose()
        {
            

            mesh.dispose();
        }
    }
}