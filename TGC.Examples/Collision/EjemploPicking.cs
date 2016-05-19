using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

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

        public EjemploPicking(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Collision";
            Name = "Picking";
            Description =
                "Permite seleccionar un objeto de la escena haciendo clic con el mouse sobre la pantalla, utilizando Picking sobre el AABB de cada Mesh";
        }

        public override void Init()
        {
            //Cargar 25 cajas formando una matriz
            var loader = new TgcSceneLoader();
            boxes = new List<TgcBox>();
            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                MediaDir + "Texturas\\granito.jpg");
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

            Camara.setCamera(new Vector3(94.9854f, 138.4992f, -284.3344f), new Vector3(86.4563f, -15.4191f, 703.7123f));

            //Crear caja para marcar en que lugar hubo colision
            collisionPointMesh = TgcBox.fromSize(new Vector3(3, 3, 3), Color.Red);
            selected = false;

            //UserVars para mostrar en que punto hubo colision
            UserVars.addVar("CollP-X:");
            UserVars.addVar("CollP-Y:");
            UserVars.addVar("CollP-Z:");
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            //Si hacen clic con el mouse, ver si hay colision RayAABB
            if (TgcD3dInput.Instance.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
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
                UserVars.setValue("CollP-X:", collisionPoint.X);
                UserVars.setValue("CollP-Y:", collisionPoint.Y);
                UserVars.setValue("CollP-Z:", collisionPoint.Z);

                //Dibujar caja que representa el punto de colision
                collisionPointMesh.Position = collisionPoint;
                collisionPointMesh.render();
            }
            else
            {
                //Reset de valores
                UserVars.setValue("CollP-X:", 0);
                UserVars.setValue("CollP-Y:", 0);
                UserVars.setValue("CollP-Z:", 0);
            }

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            foreach (var box in boxes)
            {
                box.dispose();
            }
            collisionPointMesh.dispose();
        }
    }
}