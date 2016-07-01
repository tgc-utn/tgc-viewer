using Microsoft.DirectX;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

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
    public class SeleccionMultiplePicking : TGCExampleViewer
    {
        private const float SELECTION_BOX_HEIGHT = 50;
        private Vector3 initSelectionPoint;
        private List<TgcMesh> modelos;
        private List<TgcMesh> modelosSeleccionados;
        private TgcPickingRay pickingRay;
        private bool selecting;
        private TgcBox selectionBox;

        private TgcBox suelo;

        public SeleccionMultiplePicking(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Collision";
            Name = "Seleccion Multiple";
            Description = "Muestra como seleccionar un objeto con el Mouse creando un rectángulo de selección.";
        }

        public override void Init()
        {
            //Crear suelo
            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                MediaDir + "Texturas\\Quake\\quakeWall3.jpg");
            suelo = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(500, 0.1f, 500), texture);

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
            modelos.Add(modeloOrignal.createMeshInstance("Carretilla2", new Vector3(100, 0, 0), Vector3.Empty,
                new Vector3(1, 1, 1)));
            modelos.Add(modeloOrignal.createMeshInstance("Carretilla3", new Vector3(50, 0, -70), Vector3.Empty,
                new Vector3(1, 1, 1)));
            modelos.Add(modeloOrignal.createMeshInstance("Carretilla4", new Vector3(-100, 0, -30), Vector3.Empty,
                new Vector3(1, 1, 1)));
            modelos.Add(modeloOrignal.createMeshInstance("Carretilla5", new Vector3(-70, 0, -80), Vector3.Empty,
                new Vector3(1, 1, 1)));

            //Crear caja para marcar en que lugar hubo colision
            selectionBox = TgcBox.fromSize(new Vector3(3, SELECTION_BOX_HEIGHT, 3), Color.Red);
            selectionBox.BoundingBox.setRenderColor(Color.Red);
            selecting = false;

            Camara = new TgcRotationalCamera(new Vector3(0f, 100f, 0f), 300f, Input);
            //FIXME esta camara deberia ser estatica y no rotacional, ya que sino trae problemas con el picking.
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

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
                    Vector3 collisionPoint;
                    if (TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, suelo.BoundingBox, out collisionPoint))
                    {
                        //Obtener extremos del rectángulo de selección
                        var min = Vector3.Minimize(initSelectionPoint, collisionPoint);
                        var max = Vector3.Maximize(initSelectionPoint, collisionPoint);
                        min.Y = 0;
                        max.Y = SELECTION_BOX_HEIGHT;

                        //Configurar BOX
                        selectionBox.setExtremes(min, max);
                        selectionBox.updateValues();

                        selectionBox.BoundingBox.render();
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

            //Render
            suelo.render();
            foreach (var mesh in modelos)
            {
                mesh.render();
            }

            //Renderizar BB de los modelos seleccionados
            foreach (var mesh in modelosSeleccionados)
            {
                mesh.BoundingBox.render();
            }

            PostRender();
        }

        public override void Dispose()
        {
            suelo.dispose();
            foreach (var mesh in modelos)
            {
                mesh.dispose();
            }
            selectionBox.dispose();
        }
    }
}