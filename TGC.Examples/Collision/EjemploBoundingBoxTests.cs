using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Core.SceneLoader;
using TGC.Core.Utils;
using TGC.Util;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Ejemplo EjemploBoundingBoxTests:
    ///     Unidades Involucradas:
    ///     # Unidad 6 - Detección de Colisiones - BoundingBox
    ///     Muestra como hacer colisiones entre un BoundingBox y distintas figuras geométricas:
    ///     - BoundingBox vs Triangulo
    ///     - BoundingBox vs otro BoundingBox
    ///     - BoundingBox vs Esfera
    ///     - BoundingBox vs OBB
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploBoundingBoxTests : TgcExample
    {
        private TgcBox box;
        private TgcBox box2;
        private TgcObb obb;
        private TgcBoundingSphere sphere;
        private TgcBoundingBox triagleAABB;
        private CustomVertex.PositionColored[] triangle;

        public override string getCategory()
        {
            return "Collision";
        }

        public override string getName()
        {
            return "BoundingBox-Tests";
        }

        public override string getDescription()
        {
            return
                "Muestra como hacer colisiones entre un BoundingBox y distintas figuras geométricas (AABB, Triangle, Sphere, OBB). Movimiento con W, A, S, D.";
        }

        public override void init()
        {
            //Cuerpo principal que se controla con el teclado
            box = TgcBox.fromSize(new Vector3(0, 10, 0), new Vector3(10, 10, 10), Color.Blue);

            //triangulo
            triangle = new CustomVertex.PositionColored[3];
            triangle[0] = new CustomVertex.PositionColored(-100, 0, 0, Color.Red.ToArgb());
            triangle[1] = new CustomVertex.PositionColored(0, 0, 50, Color.Green.ToArgb());
            triangle[2] = new CustomVertex.PositionColored(0, 100, 0, Color.Blue.ToArgb());
            triagleAABB =
                TgcBoundingBox.computeFromPoints(new[]
                {triangle[0].Position, triangle[1].Position, triangle[2].Position});

            //box2
            box2 = TgcBox.fromSize(new Vector3(-50, 10, -20), new Vector3(15, 15, 15), Color.Violet);

            //sphere
            sphere = new TgcBoundingSphere(new Vector3(30, 20, -20), 15);

            //OBB: computar OBB a partir del AABB del mesh.
            var loader = new TgcSceneLoader();
            var meshObb =
                loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir +
                                         "MeshCreator\\Meshes\\Vehiculos\\StarWars-ATST\\StarWars-ATST-TgcScene.xml")
                    .Meshes[0];
            obb = TgcObb.computeFromAABB(meshObb.BoundingBox);
            meshObb.dispose();
            obb.move(new Vector3(100, 0, 30));
            obb.setRotation(new Vector3(0, FastMath.PI / 4, 0));

            //Configurar camara en Tercer Persona
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(box.Position, 30, -75);
        }

        public override void render(float elapsedTime)
        {
            var velocidadCaminar = 50f * elapsedTime;

            //Calcular proxima posicion de personaje segun Input
            var d3dInput = GuiController.Instance.D3dInput;
            var moving = false;
            var movement = new Vector3(0, 0, 0);

            //Adelante
            if (d3dInput.keyDown(Key.W))
            {
                movement.Z = velocidadCaminar;
                moving = true;
            }

            //Atras
            if (d3dInput.keyDown(Key.S))
            {
                movement.Z = -velocidadCaminar;
                moving = true;
            }

            //Derecha
            if (d3dInput.keyDown(Key.D))
            {
                movement.X = velocidadCaminar;
                moving = true;
            }

            //Izquierda
            if (d3dInput.keyDown(Key.A))
            {
                movement.X = -velocidadCaminar;
                moving = true;
            }
            //Salto
            if (d3dInput.keyDown(Key.Space))
            {
                movement.Y = velocidadCaminar;
                moving = true;
            }
            //Agachar
            if (d3dInput.keyDown(Key.LeftControl))
            {
                movement.Y = -velocidadCaminar;
                moving = true;
            }
            //Si hubo desplazamiento
            if (moving)
            {
                //Aplicar movimiento
                box.move(movement);
            }
            //Hacer que la camara siga al personaje en su nueva posicion
            GuiController.Instance.ThirdPersonCamera.Target = box.Position;

            //Detectar colision con triangulo
            if (TgcCollisionUtils.testTriangleAABB(triangle[0].Position, triangle[1].Position, triangle[2].Position,
                box.BoundingBox))
            {
                box.BoundingBox.render();
                triagleAABB.render();
            }
            //Detectar colision con el otro AABB
            if (TgcCollisionUtils.testAABBAABB(box.BoundingBox, box2.BoundingBox))
            {
                box.BoundingBox.render();
                box2.BoundingBox.render();
            }
            //Detectar colision con la esfera
            if (TgcCollisionUtils.testSphereAABB(sphere, box.BoundingBox))
            {
                box.BoundingBox.render();
                sphere.setRenderColor(Color.Red);
            }
            else
            {
                sphere.setRenderColor(Color.Yellow);
            }

            //Detectar colision con la obb
            if (TgcCollisionUtils.testObbAABB(obb, box.BoundingBox))
            {
                box.BoundingBox.render();
                obb.setRenderColor(Color.Red);
            }
            else
            {
                obb.setRenderColor(Color.Yellow);
            }

            //Dibujar
            box.render();
            box2.render();
            sphere.render();
            obb.render();

            //triangulo
            D3DDevice.Instance.Device.Transform.World = Matrix.Identity;
            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionColored.Format;
            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.TriangleList, 1, triangle);
        }

        public override void close()
        {
            box.dispose();
            box2.dispose();
            sphere.dispose();
            triagleAABB.dispose();
            obb.dispose();
        }
    }
}