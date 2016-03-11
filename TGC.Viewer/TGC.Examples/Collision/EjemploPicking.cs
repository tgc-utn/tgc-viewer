using Microsoft.DirectX;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Viewer;
using TGC.Viewer.Utils.Input;
using TGC.Viewer.Utils.TgcGeometry;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Ejemplo EjemploPicking:
    ///     Unidades Involucradas:
    ///     # Unidad 6 - Detección de Colisiones - Picking
    ///     Permite seleccionar un objeto de la escena haciendo clic con el mouse sobre la pantalla.
    ///     Utiliza la técnica de Picking para hacer un testing Ray-AABB contra cada Mesh.
    ///     Los objetos de la escena son creados con TgcBox.
    ///     Se utiliza la utiliadad intersectRayAABB() de TgcCollisionUtils para detectar colisiones
    ///     entre un Ray y un BoundingBox.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploPicking : TgcExample
    {
        private List<TgcBox> boxes;
        private Vector3 collisionPoint;
        private TgcBox collisionPointMesh;
        private TgcPickingRay pickingRay;
        private bool selected;
        private TgcBox selectedMesh;

        public override string getCategory()
        {
            return "Collision";
        }

        public override string getName()
        {
            return "Picking";
        }

        public override string getDescription()
        {
            return "Permite seleccionar un objeto de la escena haciendo clic con el mouse sobre la pantalla, " +
                   "utilizando Picking sobre el AABB de cada Mesh";
        }

        public override void init()
        {
            //Cargar 25 cajas formando una matriz
            var loader = new TgcSceneLoader();
            boxes = new List<TgcBox>();
            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                GuiController.Instance.ExamplesMediaDir + "Texturas\\granito.jpg");
            var boxSize = new Vector3(30, 30, 30);
            for (var i = 0; i < 5; i++)
            {
                for (var j = 0; j < 5; j++)
                {
                    var center = new Vector3((boxSize.X + boxSize.X / 2) * i, (boxSize.Y + boxSize.Y / 2) * j, 0);
                    var box = TgcBox.fromSize(center, boxSize, texture);
                    boxes.Add(box);
                }
            }

            //Iniciarlizar PickingRay
            pickingRay = new TgcPickingRay();

            //Camara fija
            GuiController.Instance.RotCamera.Enable = false;
            GuiController.Instance.setCamera(new Vector3(94.9854f, 138.4992f, -284.3344f),
                new Vector3(86.4563f, -15.4191f, 703.7123f));

            //Crear caja para marcar en que lugar hubo colision
            collisionPointMesh = TgcBox.fromSize(new Vector3(3, 3, 3), Color.Red);
            selected = false;

            //UserVars para mostrar en que punto hubo colision
            GuiController.Instance.UserVars.addVar("CollP-X:");
            GuiController.Instance.UserVars.addVar("CollP-Y:");
            GuiController.Instance.UserVars.addVar("CollP-Z:");
        }

        public override void render(float elapsedTime)
        {
            //Si hacen clic con el mouse, ver si hay colision RayAABB
            if (GuiController.Instance.D3dInput.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Actualizar Ray de colisión en base a posición del mouse
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
                box.render();
            }

            //Renderizar BoundingBox del mesh seleccionado
            if (selected)
            {
                //Render de AABB
                selectedMesh.BoundingBox.render();

                //Cargar punto de colision
                GuiController.Instance.UserVars.setValue("CollP-X:", collisionPoint.X);
                GuiController.Instance.UserVars.setValue("CollP-Y:", collisionPoint.Y);
                GuiController.Instance.UserVars.setValue("CollP-Z:", collisionPoint.Z);

                //Dibujar caja que representa el punto de colision
                collisionPointMesh.Position = collisionPoint;
                collisionPointMesh.render();
            }
            else
            {
                //Reset de valores
                GuiController.Instance.UserVars.setValue("CollP-X:", 0);
                GuiController.Instance.UserVars.setValue("CollP-Y:", 0);
                GuiController.Instance.UserVars.setValue("CollP-Z:", 0);
            }
        }

        public override void close()
        {
            foreach (var box in boxes)
            {
                box.dispose();
            }
            collisionPointMesh.dispose();
        }
    }
}