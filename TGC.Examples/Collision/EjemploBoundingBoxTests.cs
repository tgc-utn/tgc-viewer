using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Drawing;
using TGC.Core.BoundingVolumes;
using TGC.Core.Camara;
using TGC.Core.Collision;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;
using TGC.Examples.Example;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Ejemplo EjemploBoundingBoxTests:
    ///     Unidades Involucradas:
    ///     # Unidad 6 - Deteccion de Colisiones - BoundingBox
    ///     Muestra como hacer colisiones entre un BoundingBox y distintas figuras geometricas:
    ///     - BoundingBox vs Triangulo
    ///     - BoundingBox vs otro BoundingBox
    ///     - BoundingBox vs Esfera
    ///     - BoundingBox vs OBB
    ///     Autor: Matias Leone, Leandro Barbagallo, Rodrigo Garcia
    /// </summary>
    public class EjemploBoundingBoxTests : TGCExampleViewer
    {
        private TgcMesh mesh;
        private TgcMesh meshObb;
        private TgcBox box2;
        private TgcThirdPersonCamera camaraInterna;
        private TgcBoundingOrientedBox obb;
        private TgcBoundingSphere boundingSphere;
        private TgcBoundingAxisAlignBox triagleAABB;
        private CustomVertex.PositionColored[] triangle;

        public EjemploBoundingBoxTests(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Collision";
            Name = "Bounding Box Tests";
            Description =
                "Muestra como hacer colisiones entre un BoundingBox y distintas figuras geométricas (AABB, Triangle, Sphere, OBB). Movimiento con W, A, S, D.";
        }

        public override void Init()
        {
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\Patrullero\\Patrullero-TgcScene.xml");
            mesh = scene.Meshes[0];
            mesh.Scale = new Vector3(0.25f, 0.25f, 0.25f);

            //Cuerpo principal que se controla con el teclado
            //mesh = TgcBox.fromSize(new Vector3(0, 10, 0), new Vector3(10, 10, 10), Color.Blue).toMesh("box");

            //triangulo
            triangle = new CustomVertex.PositionColored[3];
            triangle[0] = new CustomVertex.PositionColored(-100, 0, 0, Color.Green.ToArgb());
            triangle[1] = new CustomVertex.PositionColored(0, 0, 50, Color.Green.ToArgb());
            triangle[2] = new CustomVertex.PositionColored(0, 100, 0, Color.Green.ToArgb());
            triagleAABB =
                TgcBoundingAxisAlignBox.computeFromPoints(new[]
                {triangle[0].Position, triangle[1].Position, triangle[2].Position});

            //box2
            box2 = TgcBox.fromSize(new Vector3(-50, 10, -20), new Vector3(15, 15, 15), Color.Violet);

            //sphere
            boundingSphere = new TgcBoundingSphere(new Vector3(30, 20, -20), 15);

            //OBB: computar OBB a partir del AABB del mesh.
             meshObb =
                loader.loadSceneFromFile(MediaDir +
                "MeshCreator\\Meshes\\Objetos\\Catapulta\\Catapulta-TgcScene.xml")
                        .Meshes[0];
            meshObb.Scale = new Vector3(0.1f, 0.1f, 0.1f);
            meshObb.Position = new Vector3(100, 0, 30);
            meshObb.updateBoundingBox();
            obb = TgcBoundingOrientedBox.computeFromAABB(meshObb.BoundingBox);
            meshObb.Rotation = new Vector3(0, FastMath.PI / 4, 0);
            
            //Los obb tienen una especie de autotransform aun.
            obb.rotate(new Vector3(0, FastMath.PI / 4, 0));            

            //Configurar camara en Tercer Persona
            camaraInterna = new TgcThirdPersonCamera(mesh.Position, 30, -75);
            Camara = camaraInterna;
        }

        public override void Update()
        {
            PreUpdate();
            var velocidadCaminar = 50f * ElapsedTime;

            //Calcular proxima posicion de personaje segun Input
            var moving = false;
            var movement = new Vector3(0, 0, 0);

            //Adelante
            if (Input.keyDown(Key.W))
            {
                movement.Z = velocidadCaminar;
                moving = true;
            }

            //Atras
            if (Input.keyDown(Key.S))
            {
                movement.Z = -velocidadCaminar;
                moving = true;
            }

            //Derecha
            if (Input.keyDown(Key.D))
            {
                movement.X = velocidadCaminar;
                moving = true;
            }

            //Izquierda
            if (Input.keyDown(Key.A))
            {
                movement.X = -velocidadCaminar;
                moving = true;
            }
            //Salto
            if (Input.keyDown(Key.Space))
            {
                movement.Y = velocidadCaminar;
                moving = true;
            }
            //Agachar
            if (Input.keyDown(Key.LeftControl))
            {
                movement.Y = -velocidadCaminar;
                moving = true;
            }
            //Si hubo desplazamiento
            if (moving)
            {
                //Aplicar movimiento
                mesh.move(movement);
            }
            //Hacer que la camara siga al personaje en su nueva posicion
            camaraInterna.Target = mesh.Position;

            //Detectar colision con triangulo
            if (TgcCollisionUtils.testTriangleAABB(triangle[0].Position, triangle[1].Position, triangle[2].Position,
                mesh.BoundingBox))
            {
                triangle[0].Color = Color.Red.ToArgb();
                triangle[1].Color = Color.Red.ToArgb();
                triangle[2].Color = Color.Red.ToArgb();
            }
            else
            {
                triangle[0].Color = Color.Green.ToArgb();
                triangle[1].Color = Color.Green.ToArgb();
                triangle[2].Color = Color.Green.ToArgb();
            }
            //Detectar colision con el otro AABB
            if (TgcCollisionUtils.testAABBAABB(mesh.BoundingBox, box2.BoundingBox))
            {
                box2.Color = Color.Red;
                box2.updateValues();
            }
            else
            {
                box2.Color = Color.Violet;
                box2.updateValues();
            }
            //Detectar colision con la esfera
            if (TgcCollisionUtils.testSphereAABB(boundingSphere, mesh.BoundingBox))
            {
               boundingSphere.setRenderColor(Color.Red);
            }
            else
            {
                boundingSphere.setRenderColor(Color.Yellow);
            }

            //Detectar colision con la obb
            if (TgcCollisionUtils.testObbAABB(obb, mesh.BoundingBox))
            {
                obb.setRenderColor(Color.Red);
            }
            else
            {
                obb.setRenderColor(Color.Yellow);
            }
        }

        public override void Render()
        {
            PreRender();
            //Dibujar todo.
            //Una vez actualizadas las diferentes posiciones internas solo debemos asignar la matriz de world.
            mesh.Transform =
                Matrix.Scaling(mesh.Scale)
                            * Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z)
                            * Matrix.Translation(mesh.Position);
            mesh.render();
            //Actualmente los bounding box se actualizan solos en momento de hacer render, esto no es recomendado ya que trae overhead
            //Igualmente aqui podemos tener un objeto debug de nuestro mesh.
            mesh.BoundingBox.render();

            box2.Transform =
                Matrix.Scaling(box2.Scale)
                            * Matrix.RotationYawPitchRoll(box2.Rotation.Y, box2.Rotation.X, box2.Rotation.Z)
                            * Matrix.Translation(box2.Position);
            box2.render();
                        
            //Los bounding volume por la forma actual de framework no realizan transformaciones entonces podemos hacer esto:
            //D3DDevice.Instance.Device.Transform.World =
            //    Matrix.Scaling(new Vector3(sphere.Radius, sphere.Radius, sphere.Radius))
            //                * Matrix.Identity //No tienen sentido las rotaciones con la esfera.
            //                * Matrix.Translation(sphere.Position);
            boundingSphere.render();
            //Las mesh por defecto tienen el metodo updateMeshTransform que realiza el set por defecto.
            //Esto es igual que utilizar AutoTransform en true, con lo cual no es recomendado para casos complejos.
            meshObb.updateMeshTransform();
            meshObb.render();

            //La implementacion de Obb por el momento reconstruye el obb debug siempre. Practica no recomendada.
            obb.render();

            //triangulo
            D3DDevice.Instance.Device.Transform.World = Matrix.Identity;
            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionColored.Format;
            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.TriangleList, 1, triangle);

            PostRender();
        }

        public override void Dispose()
        {
            mesh.dispose();
            box2.dispose();
            boundingSphere.dispose();
            meshObb.dispose();
            obb.dispose();
            triangle = null;
        }
    }
}