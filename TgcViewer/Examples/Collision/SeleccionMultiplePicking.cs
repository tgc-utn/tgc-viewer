using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.Collision
{
    /// <summary>
    /// Ejemplo SeleccionMultiplePicking:
    /// Unidades Involucradas:
    ///     # Unidad 6 - Detección de Colisiones - Picking
    /// 
    /// Muestra como seleccionar múltiples objetos 3D con el mouse, similar a una interfaz de Windows.
    /// Utiliza Picking contra el suelo para generar un rectángulo de selección y detectar que modelos se encuentran dentro.
    /// 
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class SeleccionMultiplePicking : TgcExample
    {
        const float SELECTION_BOX_HEIGHT = 50;

        TgcBox suelo;
        List<TgcMesh> modelos;
        TgcPickingRay pickingRay;
        TgcBox selectionBox;
        bool selecting;
        Vector3 initSelectionPoint;
        List<TgcMesh> modelosSeleccionados;


        public override string getCategory()
        {
            return "Collision";
        }

        public override string getName()
        {
            return "Seleccion Multiple";
        }

        public override string getDescription()
        {
            return "Muestra como seleccionar un objeto con el Mouse creando un rectángulo de selección similar a Windows.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear suelo
            TgcTexture texture = TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\quakeWall3.jpg");
            suelo = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(500, 0.1f, 500), texture);


            //Iniciarlizar PickingRay
            pickingRay = new TgcPickingRay();


            //Cargar modelos que se pueden seleccionar
            modelos = new List<TgcMesh>();
            modelosSeleccionados = new List<TgcMesh>();

            //Modelo 1, original
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\Carretilla\\Carretilla-TgcScene.xml");
            TgcMesh modeloOrignal = scene.Meshes[0];
            modelos.Add(modeloOrignal);

            //Modelos instancias del original
            modelos.Add(modeloOrignal.createMeshInstance("Carretilla2", new Vector3(100, 0, 0), Vector3.Empty, new Vector3(1, 1 ,1)));
            modelos.Add(modeloOrignal.createMeshInstance("Carretilla3", new Vector3(50, 0, -70), Vector3.Empty, new Vector3(1, 1, 1)));
            modelos.Add(modeloOrignal.createMeshInstance("Carretilla4", new Vector3(-100, 0, -30), Vector3.Empty, new Vector3(1, 1, 1)));
            modelos.Add(modeloOrignal.createMeshInstance("Carretilla5", new Vector3(-70, 0, -80), Vector3.Empty, new Vector3(1, 1, 1)));

            //Crear caja para marcar en que lugar hubo colision
            selectionBox = TgcBox.fromSize(new Vector3(3, SELECTION_BOX_HEIGHT, 3), Color.Red);
            selectionBox.BoundingBox.setRenderColor(Color.Red);
            selecting = false;

            //Camara fija
            GuiController.Instance.RotCamera.Enable = false;
            GuiController.Instance.setCamera(new Vector3(-4.4715f, 239.1167f, 179.248f), new Vector3(-4.4742f, 238.3456f, 178.6113f));
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Si hacen clic con el mouse, ver si hay colision con el suelo
            if (GuiController.Instance.D3dInput.buttonDown(TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_LEFT))
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
                        Vector3 min = Vector3.Minimize(initSelectionPoint, collisionPoint);
                        Vector3 max = Vector3.Maximize(initSelectionPoint, collisionPoint);
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
            if (GuiController.Instance.D3dInput.buttonUp(TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                selecting = false;

                //Ver que modelos quedaron dentro del area de selección seleccionados
                foreach (TgcMesh mesh in modelos)
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
            foreach (TgcMesh mesh in modelos)
            {
                mesh.render();
            }

            //Renderizar BB de los modelos seleccionados
            foreach (TgcMesh mesh in modelosSeleccionados)
            {
                mesh.BoundingBox.render();
            }
            

            
        }

        public override void close()
        {
            suelo.dispose();
            foreach (TgcMesh mesh in modelos)
            {
                mesh.dispose();
            }
            selectionBox.dispose();
        }

    }
}
