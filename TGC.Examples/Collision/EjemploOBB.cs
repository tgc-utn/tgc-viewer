using Microsoft.DirectX;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Core.SceneLoader;
using TGC.Core.Utils;
using TGC.Viewer;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Ejemplo EjemploOBB:
    ///     Unidades Involucradas:
    ///     # Unidad 6 - Detección de Colisiones - Oriented BoundingBox (OBB)
    ///     Muestra como crear un Oriented BoundingBox a partir de un mesh.
    ///     El mesh se puede rotar el OBB acompaña esta rotacion (cosa que el AABB no puede hacer)
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploOBB : TgcExample
    {
        private TgcMesh mesh;
        private TgcObb obb;

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
            //Cargar modelo
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(
                GuiController.Instance.ExamplesMediaDir +
                "MeshCreator\\Meshes\\Vehiculos\\StarWars-ATST\\StarWars-ATST-TgcScene.xml");
            mesh = scene.Meshes[0];

            //Computar OBB a partir del AABB del mesh. Inicialmente genera el mismo volumen que el AABB, pero luego te permite rotarlo (cosa que el AABB no puede)
            obb = TgcObb.computeFromAABB(mesh.BoundingBox);

            //Otra alternativa es computar OBB a partir de sus vertices. Esto genera un OBB lo mas apretado posible pero es una operacion costosa
            //obb = TgcObb.computeFromPoints(mesh.getVertexPositions());

            //Alejar camara rotacional segun tamaño del BoundingBox del objeto
            GuiController.Instance.RotCamera.targetObject(mesh.BoundingBox);

            //Modifier para poder rotar y mover el mesh
            GuiController.Instance.Modifiers.addFloat("rotation", 0, 360, 0);
            GuiController.Instance.Modifiers.addVertex3f("position", new Vector3(0, 0, 0), new Vector3(50, 50, 50),
                new Vector3(0, 0, 0));
        }

        public override void render(float elapsedTime)
        {
            //Obtener rotacion de mesh (pasar a radianes)
            var rotation = FastMath.ToRad((float)GuiController.Instance.Modifiers["rotation"]);

            //Rotar mesh y rotar OBB. A diferencia del AABB, nosotros tenemos que mantener el OBB actualizado segun cada movimiento del mesh
            var lastRot = mesh.Rotation;
            var rotationDiff = rotation - lastRot.Y;
            mesh.rotateY(rotationDiff);
            obb.rotate(new Vector3(0, rotationDiff, 0));

            //Actualizar posicion
            var position = (Vector3)GuiController.Instance.Modifiers["position"];
            var lastPos = mesh.Position;
            var posDiff = position - lastPos;
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