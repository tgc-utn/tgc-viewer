using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Collision;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Ejemplo EjemploPicking:
    ///     Unidades Involucradas:
    ///     # Unidad 6 - Deteccion de Colisiones - Picking
    ///     Permite seleccionar un objeto de la escena haciendo clic con el mouse sobre la pantalla.
    ///     Utiliza la tecnica de Picking para hacer un testing Ray-AABB contra cada Mesh.
    ///     Los objetos de la escena son creados con TgcBox.
    ///     Se utiliza la utiliadad intersectRayAABB() de TgcCollisionUtils para detectar colisiones
    ///     entre un Ray y un BoundingBox.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploPicking : TGCExampleViewer
    {
        private List<TGCBox> boxes;
        private TGCVector3 collisionPoint;
        private TGCBox collisionPointMesh;
        private TgcPickingRay pickingRay;
        private bool selected;
        private TGCBox selectedMesh;

        public EjemploPicking(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Collision";
            Name = "Colisiones con mouse (Picking)";
            Description =
                "Permite seleccionar un objeto de la escena haciendo clic con el mouse sobre la pantalla, utilizando Picking sobre el AABB de cada Mesh";
        }

        public override void Init()
        {
            //Cargar 25 cajas formando una matriz
            var loader = new TgcSceneLoader();
            boxes = new List<TGCBox>();
            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                MediaDir + "Texturas\\granito.jpg");
            var boxSize = new TGCVector3(30, 30, 30);
            for (var i = 0; i < 5; i++)
            {
                for (var j = 0; j < 5; j++)
                {
                    var center = new TGCVector3((boxSize.X + boxSize.X / 2) * i, (boxSize.Y + boxSize.Y / 2) * j, 0);
                    var box = TGCBox.fromSize(center, boxSize, texture);
                    box.AutoTransformEnable = true;
                    boxes.Add(box);
                }
            }

            //Iniciarlizar PickingRay
            pickingRay = new TgcPickingRay(Input);

            Camara.SetCamera(new TGCVector3(100f, 100f, -500f), new TGCVector3(100f, 100f, -250f));

            //Crear caja para marcar en que lugar hubo colision
            collisionPointMesh = TGCBox.fromSize(new TGCVector3(3, 3, 3), Color.Red);
            collisionPointMesh.AutoTransformEnable = true;
            selected = false;

            //UserVars para mostrar en que punto hubo colision
            UserVars.addVar("CollP-X:");
            UserVars.addVar("CollP-Y:");
            UserVars.addVar("CollP-Z:");
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Si hacen clic con el mouse, ver si hay colision RayAABB
            if (Input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Actualizar Ray de colision en base a posicion del mouse
                pickingRay.updateRay();

                //Testear Ray contra el AABB de todos los meshes
                foreach (var box in boxes)
                {
                    var aabb = box.BoundingBox;

                    //Ejecutar test, si devuelve true se carga el punto de colision collisionPoint
                    selected = TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, aabb, out collisionPoint);
                    if (selected)
                    {
                        selectedMesh = box;
                        break;
                    }
                }
            }

            //Renderizar modelos
            foreach (var box in boxes)
            {
                box.Render();
            }

            //Renderizar BoundingBox del mesh seleccionado
            if (selected)
            {
                //Render de AABB
                selectedMesh.BoundingBox.Render();

                //Cargar punto de colision
                UserVars.setValue("CollP-X:", collisionPoint.X);
                UserVars.setValue("CollP-Y:", collisionPoint.Y);
                UserVars.setValue("CollP-Z:", collisionPoint.Z);

                //Dibujar caja que representa el punto de colision
                collisionPointMesh.Position = collisionPoint;
                collisionPointMesh.Render();
            }
            else
            {
                //Reset de valores
                UserVars.setValue("CollP-X:", 0);
                UserVars.setValue("CollP-Y:", 0);
                UserVars.setValue("CollP-Z:", 0);
            }

            PostRender();
        }

        public override void Dispose()
        {
            foreach (var box in boxes)
            {
                box.Dispose();
            }
            collisionPointMesh.Dispose();
        }
    }
}