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
    /// Ejemplo EjemploOBB:
    /// Unidades Involucradas:
    ///     # Unidad 6 - Detecci�n de Colisiones - Oriented BoundingBox (OBB)
    /// 
    /// Muestra como crear un Oriented BoundingBox a partir de un mesh.
    /// El mesh se puede rotar el OBB acompa�a esta rotacion (cosa que el AABB no puede hacer)
    /// 
    /// 
    /// Autor: Mat�as Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploOBB : TgcExample
    {
        TgcMesh mesh;
        TgcObb obb;


        public override string getCategory()
        {
            return "Collision";
        }

        public override string getName()
        {
            return "OBB";
        }

        public override string getDescription()
        {
            return "Muestra como crear un Oriented BoundingBox a partir de un mesh. Movimiento con mouse.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Cargar modelo
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(
                GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\StarWars-ATST\\StarWars-ATST-TgcScene.xml");
            mesh = scene.Meshes[0];

            //Computar OBB a partir del AABB del mesh. Inicialmente genera el mismo volumen que el AABB, pero luego te permite rotarlo (cosa que el AABB no puede)
            obb = TgcObb.computeFromAABB(mesh.BoundingBox);

            //Otra alternativa es computar OBB a partir de sus vertices. Esto genera un OBB lo mas apretado posible pero es una operacion costosa
            //obb = TgcObb.computeFromPoints(mesh.getVertexPositions());

            


            //Alejar camara rotacional segun tama�o del BoundingBox del objeto
            GuiController.Instance.RotCamera.targetObject(mesh.BoundingBox);


            //Modifier para poder rotar y mover el mesh
            GuiController.Instance.Modifiers.addFloat("rotation", 0, 360, 0);
            GuiController.Instance.Modifiers.addVertex3f("position", new Vector3(0, 0, 0), new Vector3(50, 50, 50), new Vector3(0, 0, 0));
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Obtener rotacion de mesh (pasar a radianes)
            float rotation = FastMath.ToRad((float)GuiController.Instance.Modifiers["rotation"]);

            //Rotar mesh y rotar OBB. A diferencia del AABB, nosotros tenemos que mantener el OBB actualizado segun cada movimiento del mesh
            Vector3 lastRot = mesh.Rotation;
            float rotationDiff = rotation - lastRot.Y;
            mesh.rotateY(rotationDiff);
            obb.rotate(new Vector3(0, rotationDiff, 0));

            //Actualizar posicion
            Vector3 position = (Vector3)GuiController.Instance.Modifiers["position"];
            Vector3 lastPos = mesh.Position;
            Vector3 posDiff = position - lastPos;
            mesh.move(posDiff);
            obb.move(posDiff);

            //Renderizar modelo
            mesh.render();

            //Renderizar obb
            obb.render();
        }

        public override void close()
        {
            mesh.dispose();
            obb.dispose();
        }

    }
}
