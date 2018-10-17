using BulletSharp;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Core.BulletPhysics
{
    /// <summary>
    /// Clase encargada de generear todos los rigid bodies que necesitemos para añadir a la simulacion de BulletSharp.
    /// </summary>
    public abstract class BulletRigidBodyConstructor
    {
        /// <summary>
        ///  Se crea una caja con una masa (si se quiere que sea estatica la masa debe ser 0),
        ///  con dimensiones x(ancho), y(alto), z(profundidad), Rotacion de ejes Yaw, Pitch, Roll y un coeficiente de rozamiento.
        /// </summary>
        /// <param name="size">Tamaño de la Cajas</param>
        /// <param name="mass">Masa de la Caja</param>
        /// <param name="position">Posicion de la Caja</param>
        /// <param name="yaw">Rotacion de la Caja respecto del eje x</param>
        /// <param name="pitch">Rotacion de la Caja respecto del eje z</param>
        /// <param name="roll">Rotacion de la Caja respecto del eje y</param>
        /// <param name="friction">Coeficiente de rozamiento de la Caja</param>
        /// <param name="inertia">Booleano para calcular inercia o no</param>
        /// <returns>Rigid Body de la caja</returns>
        public static RigidBody CreateBox(TGCVector3 size, float mass, TGCVector3 position, float yaw, float pitch, float roll, float friction, bool inertia)
        {
            var boxShape = new BoxShape(size.X, size.Y, size.Z);
            var boxTransform = TGCMatrix.RotationYawPitchRoll(yaw, pitch, roll).ToBsMatrix;
            boxTransform.Origin = position.ToBulletVector3();
            DefaultMotionState boxMotionState = new DefaultMotionState(boxTransform);
            var boxLocalInertia = TGCVector3.Empty.ToBulletVector3();

            if (inertia)
            {
                //Es importante calcular la inercia caso contrario el objeto no rotara.
                boxLocalInertia = boxShape.CalculateLocalInertia(mass);
            }
            
            var boxInfo = new RigidBodyConstructionInfo(mass, boxMotionState, boxShape, boxLocalInertia);
            var boxBody = new RigidBody(boxInfo);
            boxBody.LinearFactor = TGCVector3.One.ToBulletVector3();
            boxBody.Friction = friction;
            return boxBody;
        }

        /// <summary>
        /// Se crea una capsula a partir de un radio, altura, posicion, masa y si se dedea o no calcular
        /// la inercia. Esto es importante ya que sin inercia no se generan rotaciones que no se
        /// controlen en forma particular.
        /// </summary>
        /// <param name="radius">Radio de la Capsula</param>
        /// <param name="height">Altura de la Capsula</param>
        /// <param name="position">Posicion de la Capsula</param>
        /// <param name="mass">Masa de la Capsula</param>
        /// <param name="needInertia">Booleano para el momento de inercia de la Capsula</param>
        /// <returns>Rigid Body de una Capsula</returns>
        public static RigidBody CreateCapsule(float radius, float height, TGCVector3 position, float mass, bool needInertia)
        {
            //Creamos el shape de la Capsula a partir de un radio y una altura.
            var capsuleShape = new CapsuleShape(radius, height);

            //Armamos las transformaciones que luego formaran parte del cuerpo rigido de la capsula.
            var capsuleTransform = TGCMatrix.Identity;
            capsuleTransform.Origin = position;
            var capsuleMotionState = new DefaultMotionState(capsuleTransform.ToBsMatrix);
            RigidBodyConstructionInfo capsuleRigidBodyInfo;

            //Calculamos o no el momento de inercia dependiendo de que comportamiento
            //queremos que tenga la capsula.
            if (!needInertia)
            {
                capsuleRigidBodyInfo = new RigidBodyConstructionInfo(mass, capsuleMotionState, capsuleShape);
            }
            else
            {
                var capsuleInertia = capsuleShape.CalculateLocalInertia(mass);
                capsuleRigidBodyInfo = new RigidBodyConstructionInfo(mass, capsuleMotionState, capsuleShape, capsuleInertia);
            }

            var localCapsuleRigidBody = new RigidBody(capsuleRigidBodyInfo);
            localCapsuleRigidBody.LinearFactor = TGCVector3.One.ToBulletVector3();
            //Dado que hay muchos parametros a configurar el RigidBody lo ideal es que
            //cada caso se configure segun lo que se necesite.

            return localCapsuleRigidBody;
        }

        /// <summary>
        ///     Se crea una esfera a partir de un radio, masa y posicion devolviendo el cuerpo rigido de una
        ///     esfera.
        /// </summary>
        /// <param name="radius">Radio de una esfera</param>
        /// <param name="mass">Masa de la esfera</param>
        /// <param name="position">Posicion de la Esfera</param>
        /// <returns>Rigid Body de la Esfera</returns>
        public static RigidBody CreateBall(float radius, float mass, TGCVector3 position)
        {
            //Creamos la forma de la esfera a partir de un radio
            var ballShape = new SphereShape(radius);

            //Armamos las matrices de transformacion de la esfera a partir de la posicion con la que queremos ubicarla
            //y el estado de movimiento de la misma.
            var ballTransform = TGCMatrix.Identity;
            ballTransform.Origin = position;
            var ballMotionState = new DefaultMotionState(ballTransform.ToBsMatrix);

            //Se calcula el momento de inercia de la esfera a partir de la masa.
            var ballLocalInertia = ballShape.CalculateLocalInertia(mass);
            var ballInfo = new RigidBodyConstructionInfo(mass, ballMotionState, ballShape, ballLocalInertia);

            //Creamos el cuerpo rigido de la esfera a partir de la info.
            var ballBody = new RigidBody(ballInfo);
            ballBody.LinearFactor = TGCVector3.One.ToBulletVector3();
            return ballBody;
        }

        /// <summary>
        ///     Crea una coleccion de triangulos para Bullet a partir de los triangulos generados por un heighmap
        ///     o una coleccion de triangulos a partir de un Custom Vertex Buffer con vertices del tipo Position Texured.
        ///     Se utilizo el codigo de un snippet de Bullet http://www.bulletphysics.org/mediawiki-1.5.8/index.php?title=Code_Snippets
        /// </summary>
        /// <param name="triangleDataVB">Custom Vertex Buffer que puede ser de un Heightmap</param>
        /// <returns>Rigid Body del terreno</returns>
        public static RigidBody CreateSurfaceFromHeighMap(CustomVertex.PositionTextured[] triangleDataVB)
        {
            //Triangulos
            var triangleMesh = new TriangleMesh();
            int i = 0;
            TGCVector3 vector0;
            TGCVector3 vector1;
            TGCVector3 vector2;

            while (i < triangleDataVB.Length)
            {
                var triangle = new Triangle();
                vector0 = new TGCVector3(triangleDataVB[i].X, triangleDataVB[i].Y, triangleDataVB[i].Z);
                vector1 = new TGCVector3(triangleDataVB[i + 1].X, triangleDataVB[i + 1].Y, triangleDataVB[i + 1].Z);
                vector2 = new TGCVector3(triangleDataVB[i + 2].X, triangleDataVB[i + 2].Y, triangleDataVB[i + 2].Z);

                i = i + 3;

                triangleMesh.AddTriangle(vector0.ToBulletVector3(), vector1.ToBulletVector3(), vector2.ToBulletVector3(), false);
            }

            CollisionShape meshCollisionShape = new BvhTriangleMeshShape(triangleMesh, true);
            var meshMotionState = new DefaultMotionState();
            var meshRigidBodyInfo = new RigidBodyConstructionInfo(0, meshMotionState, meshCollisionShape);
            RigidBody meshRigidBody = new RigidBody(meshRigidBodyInfo);

            return meshRigidBody;
        }

        /// <summary>
        ///     Se arma un cilindro a partir de las dimensiones, una posicion y su masa
        /// </summary>
        /// <param name="dimensions">Dimensiones en x,y,z del Cilindro</param>
        /// <param name="position">Posicion del Cilindro</param>
        /// <param name="mass">Masa del Cilindro</param>
        /// <returns>Cuerpo rigido de un Cilindro</returns>
        public static RigidBody CreateCylinder(TGCVector3 dimensions, TGCVector3 position, float mass)
        {
            //Creamos el Shape de un Cilindro
            var cylinderShape = new CylinderShape(dimensions.X, dimensions.Y, dimensions.Z);

            //Armamos la matrix asociada al Cilindro y el estado de movimiento de la misma.
            var cylinderTransform = TGCMatrix.Identity;
            cylinderTransform.Origin = position;
            var cylinderMotionState = new DefaultMotionState(cylinderTransform.ToBsMatrix);

            //Calculamos el momento de inercia
            var cylinderLocalInertia = cylinderShape.CalculateLocalInertia(mass);
            var cylinderInfo = new RigidBodyConstructionInfo(mass, cylinderMotionState, cylinderShape, cylinderLocalInertia);

            //Creamos el cuerpo rigido a partir del de la informacion de cuerpo rigido.
            RigidBody cylinderBody = new RigidBody(cylinderInfo);
            return cylinderBody;
        }

        /// <summary>
        ///     Se crea uncuerpo rigido a partir de un TgcMesh, pero no tiene masa 
        ///     por lo que va a ser estatico.
        /// </summary>
        /// <param name="mesh">TgcMesh</param>
        /// <returns>Cuerpo rigido de un Mesh</returns>
        public static RigidBody CreateRigidBodyFromTgcMesh(TgcMesh mesh)
        {
            var vertexCoords = mesh.getVertexPositions();

            TriangleMesh triangleMesh = new TriangleMesh();
            for (int i = 0; i < vertexCoords.Length; i = i + 3)
            {
                triangleMesh.AddTriangle(vertexCoords[i].ToBulletVector3(), vertexCoords[i + 1].ToBulletVector3(), vertexCoords[i + 2].ToBulletVector3());
            }

            var transformationMatrix = TGCMatrix.RotationYawPitchRoll(0, 0, 0).ToBsMatrix;
            DefaultMotionState motionState = new DefaultMotionState(transformationMatrix);

            var bulletShape = new BvhTriangleMeshShape(triangleMesh, false);
            var boxLocalInertia = bulletShape.CalculateLocalInertia(0);

            var bodyInfo = new RigidBodyConstructionInfo(0, motionState, bulletShape, boxLocalInertia);
            var rigidBody = new RigidBody(bodyInfo);
            rigidBody.Friction = 0.4f;
            rigidBody.RollingFriction = 1;
            rigidBody.Restitution = 1f;

            return rigidBody;
        }

     }
}