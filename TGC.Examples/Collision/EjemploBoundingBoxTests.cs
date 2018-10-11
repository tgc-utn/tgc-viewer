using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Ejemplo EjemploBoundingBoxTests:
    ///     Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Ciclo acoplado vs Ciclo desacoplado
    ///     # Unidad 5 - Animaciones - Skeletal Animation
    ///     # Unidad 6 - Deteccion de Colisiones - BoundingBox
    ///     Muestra como hacer colisiones entre un BoundingBox y distintas figuras geometricas:
    ///     - BoundingBox vs Triangulo
    ///     - BoundingBox vs otro BoundingBox
    ///     - BoundingBox vs Esfera
    ///     - BoundingBox vs OBB
    ///     Ademas muestra como desplazar un modelo animado en base a la entrada de teclado. Usando move pero no autotransform.
    ///     El modelo animado utiliza la herramienta TgcKeyFrameLoader.
    ///     tambien se crear un Oriented BoundingBox a partir de un mesh.
    ///     El mesh se puede rotar el OBB acompana esta rotacion (cosa que el AABB no puede hacer)
    ///     Autor: Matias Leone, Leandro Barbagallo, Rodrigo Garcia
    /// </summary>
    public class EjemploBoundingBoxTests : TGCExampleViewer
    {
        //Velocidad de desplazamiento
        private const float VELOCIDAD_DESPLAZAMIENTO = 50f;

        private TgcMesh mesh;
        private TgcMesh meshObb;
        private TGCBox box2;
        private TgcThirdPersonCamera camaraInterna;
        private TgcBoundingOrientedBox obb;
        private TgcBoundingSphere boundingSphere;
        private TgcBoundingAxisAlignBox triagleAABB;
        private CustomVertex.PositionColored[] triangle;

        public EjemploBoundingBoxTests(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Collision";
            Name = "Bounding Box Tests";
            Description =
                "Muestra como hacer colisiones entre un BoundingBox y distintas figuras geom�tricas (AABB, Triangle, Sphere, OBB). Movimiento con W, A, S, D.";
        }

        public override void Init()
        {
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\Patrullero\\Patrullero-TgcScene.xml");
            mesh = scene.Meshes[0];
            mesh.Scale = new TGCVector3(0.25f, 0.25f, 0.25f);

            //triangulo
            triangle = new CustomVertex.PositionColored[3];
            triangle[0] = new CustomVertex.PositionColored(-100, 0, 0, Color.Green.ToArgb());
            triangle[1] = new CustomVertex.PositionColored(0, 0, 50, Color.Green.ToArgb());
            triangle[2] = new CustomVertex.PositionColored(0, 100, 0, Color.Green.ToArgb());
            triagleAABB = TgcBoundingAxisAlignBox.computeFromPoints(new[]
                {TGCVector3.FromVector3(triangle[0].Position), TGCVector3.FromVector3(triangle[1].Position), TGCVector3.FromVector3(triangle[2].Position)});

            //box2
            box2 = TGCBox.fromSize(new TGCVector3(-50, 10, -20), new TGCVector3(15, 15, 15), Color.Violet);

            //sphere
            boundingSphere = new TgcBoundingSphere(new TGCVector3(30, 20, -20), 15);

            //OBB: computar OBB a partir del AABB del mesh.
            meshObb =
               loader.loadSceneFromFile(MediaDir +
               "MeshCreator\\Meshes\\Objetos\\Catapulta\\Catapulta-TgcScene.xml")
                       .Meshes[0];
            meshObb.Scale = new TGCVector3(0.1f, 0.1f, 0.1f);
            meshObb.Position = new TGCVector3(100, 0, 30);
            meshObb.updateBoundingBox();
            //Computar OBB a partir del AABB del mesh. Inicialmente genera el mismo volumen que el AABB, pero luego te permite rotarlo (cosa que el AABB no puede)
            obb = TgcBoundingOrientedBox.computeFromAABB(meshObb.BoundingBox);
            //Otra alternativa es computar OBB a partir de sus vertices. Esto genera un OBB lo mas apretado posible pero es una operacion costosa
            //obb = TgcBoundingOrientedBox.computeFromPoints(mesh.getVertexPositions());

            //Rotar mesh y rotar OBB. A diferencia del AABB, nosotros tenemos que mantener el OBB actualizado segun cada movimiento del mesh
            meshObb.Rotation = new TGCVector3(0, FastMath.PI / 4, 0);
            //Los obb tienen una especie de autotransform aun.
            obb.rotate(new TGCVector3(0, FastMath.PI / 4, 0));

            //Configurar camara en Tercer Persona
            camaraInterna = new TgcThirdPersonCamera(mesh.Position, 30, -75);
            Camara = camaraInterna;
        }

        public override void Update()
        {
            PreUpdate();
            var velocidadCaminar = VELOCIDAD_DESPLAZAMIENTO * ElapsedTime;

            //Calcular proxima posicion de personaje segun Input
            var moving = false;
            var movement = TGCVector3.Empty;

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
                //Aplicar movimiento, internamente suma valores a la posicion actual del mesh.
                mesh.Position = mesh.Position + movement;
                mesh.Transform = TGCMatrix.Translation(mesh.Position);
                mesh.updateBoundingBox();
            }
            //Hacer que la camara siga al personaje en su nueva posicion
            camaraInterna.Target = mesh.Position;

            //Detectar colision con triangulo
            if (TgcCollisionUtils.testTriangleAABB(TGCVector3.FromVector3(triangle[0].Position), TGCVector3.FromVector3(triangle[1].Position), TGCVector3.FromVector3(triangle[2].Position),
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
            PostUpdate();
        }

        public override void Render()
        {
            PreRender();
            //Dibujar todo.
            //Una vez actualizadas las diferentes posiciones internas solo debemos asignar la matriz de world.
            mesh.Transform = TGCMatrix.Scaling(mesh.Scale) *
                             TGCMatrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) *
                             TGCMatrix.Translation(mesh.Position);
            mesh.Render();
            //Actualmente los bounding box se actualizan solos en momento de hacer render, esto no es recomendado ya que trae overhead
            //Igualmente aqui podemos tener un objeto debug de nuestro mesh.
            mesh.BoundingBox.Render();

            box2.Transform = TGCMatrix.Scaling(box2.Scale) *
                             TGCMatrix.RotationYawPitchRoll(box2.Rotation.Y, box2.Rotation.X, box2.Rotation.Z) *
                             TGCMatrix.Translation(box2.Position);
            box2.Render();

            boundingSphere.Render();

            //Las mesh por defecto tienen el metodo UpdateMeshTransform que realiza el set por defecto.
            meshObb.Transform = TGCMatrix.Scaling(meshObb.Scale) *
                                TGCMatrix.RotationYawPitchRoll(meshObb.Rotation.Y, meshObb.Rotation.X, meshObb.Rotation.Z) *
                                TGCMatrix.Translation(meshObb.Position);
            meshObb.Render();

            //La implementacion de Obb por el momento reconstruye el obb debug siempre. Practica no recomendada.
            obb.Render();

            //triangulo
            D3DDevice.Instance.Device.Transform.World = TGCMatrix.Identity.ToMatrix();
            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionColored.Format;
            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.TriangleList, 1, triangle);

            PostRender();
        }

        public override void Dispose()
        {
            mesh.Dispose();
            box2.Dispose();
            boundingSphere.Dispose();
            meshObb.Dispose();
            obb.Dispose();
            triangle = null;
        }
    }
}