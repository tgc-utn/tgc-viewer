using BulletSharp;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Mathematica;

namespace TGC.Core.BulletPhysics
{
    /// <summary>
    ///
    /// </summary>
    public abstract class BulletRigidBodyConstructor

    {
        /// <summary>
        ///  Se crea una caja con una masa (si se quiere que sea estatica la masa debe ser 0),
        ///  con dimensiones x(ancho) ,y(alto) ,z(profundidad), Rotacion de ejes Yaw, Pitch, Roll y un coeficiente de rozamiento.
        /// </summary>
        /// <param name="size">Tamaño de la Cajas</param>
        /// <param name="mass">Masa de la Caja</param>
        /// <param name="position">Posicion de la Caja</param>
        /// <param name="yaw">Rotacion de la Caja respecto del eje x</param>
        /// <param name="pitch">Rotacion de la Caja respecto del eje z</param>
        /// <param name="roll">Rotacion de la Caja respecto del eje y</param>
        /// <param name="friction">Coeficiente de rozamiento de la Caja</param>
        /// <returns>Rigid Body de la caja</returns>
        public static RigidBody CreateBox(TGCVector3 size, float mass, TGCVector3 position, float yaw, float pitch, float roll, float friction)
        {
            var boxShape = new BoxShape(size.X, size.Y, size.Z);
            var boxTransform = TGCMatrix.RotationYawPitchRoll(yaw, pitch, roll).ToBsMatrix;
            boxTransform.Origin = position.ToBsVector;
            DefaultMotionState boxMotionState = new DefaultMotionState(boxTransform);
            //Es importante calcular la inercia caso contrario el objeto no rotara.
            var boxLocalInertia = boxShape.CalculateLocalInertia(mass);
            var boxInfo = new RigidBodyConstructionInfo(mass, boxMotionState, boxShape, boxLocalInertia);
            var boxBody = new RigidBody(boxInfo);
            boxBody.LinearFactor = TGCVector3.One.ToBsVector;
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
            localCapsuleRigidBody.LinearFactor = TGCVector3.One.ToBsVector;
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
            ballBody.LinearFactor = TGCVector3.One.ToBsVector;
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

                triangleMesh.AddTriangle(vector0.ToBsVector, vector1.ToBsVector, vector2.ToBsVector, false);
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
    }
}