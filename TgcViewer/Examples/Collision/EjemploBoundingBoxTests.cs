using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Input;
using Microsoft.DirectX.DirectInput;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.Collision
{
    /// <summary>
    /// Ejemplo EjemploBoundingBoxTests:
    /// Unidades Involucradas:
    ///     # Unidad 6 - Detección de Colisiones - BoundingBox
    /// 
    /// Muestra como hacer colisiones entre un BoundingBox y distintas figuras geométricas:
    /// - BoundingBox vs Triangulo
    /// - BoundingBox vs otro BoundingBox
    /// - BoundingBox vs Esfera
    /// - BoundingBox vs OBB
    /// 
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploBoundingBoxTests : TgcExample
    {
        TgcBox box;
        TgcBox box2;
        CustomVertex.PositionColored[] triangle;
        TgcBoundingBox triagleAABB;
        TgcBoundingSphere sphere;
        TgcObb obb;

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
            return "Muestra como hacer colisiones entre un BoundingBox y distintas figuras geométricas (AABB, Triangle, Sphere, OBB). Movimiento con W, A, S, D.";
        }

        public override void init()
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //Cuerpo principal que se controla con el teclado
            box = TgcBox.fromSize(new Vector3(0, 10, 0), new Vector3(10, 10, 10), Color.Blue);

            //triangulo
            triangle = new CustomVertex.PositionColored[3];
            triangle[0] = new CustomVertex.PositionColored(-100, 0, 0, Color.Red.ToArgb());
            triangle[1] = new CustomVertex.PositionColored(0, 0, 50, Color.Green.ToArgb());
            triangle[2] = new CustomVertex.PositionColored(0, 100, 0, Color.Blue.ToArgb());
            triagleAABB = TgcBoundingBox.computeFromPoints(new Vector3[] { triangle[0].Position, triangle[1].Position, triangle[2].Position });

            //box2
            box2 = TgcBox.fromSize(new Vector3(-50, 10, -20), new Vector3(15, 15, 15), Color.Violet);

            //sphere
            sphere = new TgcBoundingSphere(new Vector3(30, 20, -20), 15);

            //OBB: computar OBB a partir del AABB del mesh.
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcMesh meshObb = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\StarWars-ATST\\StarWars-ATST-TgcScene.xml").Meshes[0];
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
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            float velocidadCaminar = 50f * elapsedTime;

            //Calcular proxima posicion de personaje segun Input
            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;
            bool moving = false;
            Vector3 movement = new Vector3(0, 0, 0);

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
            if (TgcCollisionUtils.testTriangleAABB(triangle[0].Position, triangle[1].Position, triangle[2].Position, box.BoundingBox))
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
            d3dDevice.Transform.World = Matrix.Identity;
            d3dDevice.VertexFormat = CustomVertex.PositionColored.Format;
            d3dDevice.DrawUserPrimitives(PrimitiveType.TriangleList, 1, triangle);
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
