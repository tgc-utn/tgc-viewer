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
    ///     Ejemplo SeleccionMultiplePicking:
    ///     Unidades Involucradas:
    ///     # Unidad 6 - Detección de Colisiones - Picking
    ///     Muestra como seleccionar múltiples objetos 3D con el mouse, similar a una interfaz de Windows.
    ///     Utiliza Picking contra el suelo para generar un rectángulo de selección y detectar que modelos se encuentran
    ///     dentro.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploMultipleSelectPicking : TGCExampleViewer
    {
        private const float SELECTION_BOX_HEIGHT = 50;
        private TGCVector3 initSelectionPoint;
        private List<TgcMesh> modelos;
        private List<TgcMesh> modelosSeleccionados;
        private TgcPickingRay pickingRay;
        private bool selecting;
        private TGCBox selectionBox;

        private TgcPlane suelo;

        public EjemploMultipleSelectPicking(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Collision";
            Name = "Colisiones con mouse seleccion multiple";
            Description = "Muestra como seleccionar un objeto con el Mouse creando un rectángulo de selección.";
        }

        public override void Init()
        {
            //Crear suelo
            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                MediaDir + "Texturas\\Quake\\quakeWall3.jpg");
            suelo = new TgcPlane(new TGCVector3(-500, 0, -500), new TGCVector3(1000, 0f, 1000), TgcPlane.Orientations.XZplane, texture);

            //Iniciarlizar PickingRay
            pickingRay = new TgcPickingRay(Input);

            //Cargar modelos que se pueden seleccionar
            modelos = new List<TgcMesh>();
            modelosSeleccionados = new List<TgcMesh>();

            //Modelo 1, original
            var loader = new TgcSceneLoader();
            var scene =
                loader.loadSceneFromFile(MediaDir +
                                         "MeshCreator\\Meshes\\Vehiculos\\Carretilla\\Carretilla-TgcScene.xml");
            var modeloOrignal = scene.Meshes[0];
            modelos.Add(modeloOrignal);

            //Modelos instancias del original
            modelos.Add(modeloOrignal.createMeshInstance("Carretilla2", new TGCVector3(100, 0, 0), TGCVector3.Empty,
                TGCVector3.One));
            modelos.Add(modeloOrignal.createMeshInstance("Carretilla3", new TGCVector3(50, 0, -70), TGCVector3.Empty,
                TGCVector3.One));
            modelos.Add(modeloOrignal.createMeshInstance("Carretilla4", new TGCVector3(-100, 0, -30), TGCVector3.Empty,
                TGCVector3.One));
            modelos.Add(modeloOrignal.createMeshInstance("Carretilla5", new TGCVector3(-70, 0, -80), TGCVector3.Empty,
                TGCVector3.One));

            //Crear caja para marcar en que lugar hubo colision
            selectionBox = TGCBox.fromSize(new TGCVector3(3, SELECTION_BOX_HEIGHT, 3), Color.Red);
            selectionBox.BoundingBox.setRenderColor(Color.Red);
            selecting = false;

            Camera.SetCamera(new TGCVector3(250f, 250f, 250f), TGCVector3.Empty);
        }

        public override void Update()
        {
            //Si hacen clic con el mouse, ver si hay colision con el suelo
            if (Input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //primera vez
                if (!selecting)
                {
                    //Actualizar Ray de colisión en base a posición del mouse
                    pickingRay.updateRay();

                    //Detectar colisión Ray-AABB
                    if (TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, suelo.BoundingBox, out initSelectionPoint))
                    {
                        selecting = true;
                        modelosSeleccionados.Clear();
                    }
                }

                //Si se está seleccionado, generar box de seleccion
                else
                {
                    //Detectar nuevo punto de colision con el piso
                    pickingRay.updateRay();
                    TGCVector3 collisionPoint;
                    if (TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, suelo.BoundingBox, out collisionPoint))
                    {
                        //Obtener extremos del rectángulo de selección
                        var min = TGCVector3.Minimize(initSelectionPoint, collisionPoint);
                        var max = TGCVector3.Maximize(initSelectionPoint, collisionPoint);
                        min.Y = 0;
                        max.Y = SELECTION_BOX_HEIGHT;

                        //Configurar BOX
                        selectionBox.setExtremes(min, max);
                        selectionBox.updateValues();
                    }
                }
            }

            //Solto el clic del mouse, terminar la selección
            if (Input.buttonUp(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                selecting = false;

                //Ver que modelos quedaron dentro del area de selección seleccionados
                foreach (var mesh in modelos)
                {
                    //Colisión de AABB entre área de selección y el modelo
                    if (TgcCollisionUtils.testAABBAABB(selectionBox.BoundingBox, mesh.BoundingBox))
                    {
                        modelosSeleccionados.Add(mesh);
                    }
                }
            }
        }

        public override void Render()
        {
            PreRender();

            //FIX IT SOLO CON COLISION.
            if (selecting)
                selectionBox.BoundingBox.Render();

            //Render
            suelo.Render();
            foreach (var mesh in modelos)
            {
                mesh.Transform =
                TGCMatrix.Scaling(mesh.Scale) * TGCMatrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) * TGCMatrix.Translation(mesh.Position);
                mesh.Render();
            }

            //Renderizar BB de los modelos seleccionados
            foreach (var mesh in modelosSeleccionados)
            {
                mesh.BoundingBox.Render();
            }

            PostRender();
        }

        public override void Dispose()
        {
            suelo.Dispose();
            foreach (var mesh in modelos)
            {
                mesh.Dispose();
            }
            selectionBox.Dispose();
        }
    }
}