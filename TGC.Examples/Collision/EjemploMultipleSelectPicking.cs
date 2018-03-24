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
    ///     # Unidad 6 - Detecci�n de Colisiones - Picking
    ///     Muestra como seleccionar m�ltiples objetos 3D con el mouse, similar a una interfaz de Windows.
    ///     Utiliza Picking contra el suelo para generar un rect�ngulo de selecci�n y detectar que modelos se encuentran
    ///     dentro.
    ///     Autor: Mat�as Leone, Leandro Barbagallo
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
            Description = "Muestra como seleccionar un objeto con el Mouse creando un rect�ngulo de selecci�n.";
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

            Camara.SetCamera(new TGCVector3(250f, 250f, 250f), new TGCVector3(0f, 0f, 0f));
        }

        public override void Update()
        {
            PreUpdate();

            //Si hacen clic con el mouse, ver si hay colision con el suelo
            if (Input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //primera vez
                if (!selecting)
                {
                    //Actualizar Ray de colisi�n en base a posici�n del mouse
                    pickingRay.updateRay();

                    //Detectar colisi�n Ray-AABB
                    if (TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, suelo.BoundingBox, out initSelectionPoint))
                    {
                        selecting = true;
                        modelosSeleccionados.Clear();
                    }
                }

                //Si se est� seleccionado, generar box de seleccion
                else
                {
                    //Detectar nuevo punto de colision con el piso
                    pickingRay.updateRay();
                    TGCVector3 collisionPoint;
                    if (TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, suelo.BoundingBox, out collisionPoint))
                    {
                        //Obtener extremos del rect�ngulo de selecci�n
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

            //Solto el clic del mouse, terminar la selecci�n
            if (Input.buttonUp(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                selecting = false;

                //Ver que modelos quedaron dentro del area de selecci�n seleccionados
                foreach (var mesh in modelos)
                {
                    //Colisi�n de AABB entre �rea de selecci�n y el modelo
                    if (TgcCollisionUtils.testAABBAABB(selectionBox.BoundingBox, mesh.BoundingBox))
                    {
                        modelosSeleccionados.Add(mesh);
                    }
                }
            }

            PostUpdate();
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