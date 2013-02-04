using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using TgcViewer;

namespace Examples.Collision.SphereCollision
{
    /// <summary>
    /// Herramienta para realizar el movimiento de una Esfera con detecci�n de colisiones,
    /// efecto de Sliding y gravedad.
    /// Basado en el paper de Kasper Fauerby
    /// http://www.peroxide.dk/papers/collision/collision.pdf
    /// Su utiliza una estrategia distinta al paper en el nivel m�s bajo de colisi�n.
    /// No se analizan colisiones a nivel de tr�angulo, sino que todo objeto se descompone
    /// a nivel de un BoundingBox con 6 caras rectangulares.
    /// 
    /// </summary>
    public class SphereCollisionManager
    {
        const float EPSILON = 0.05f;

        private Vector3 gravityForce;
        /// <summary>
        /// Vector que representa la fuerza de gravedad.
        /// Debe tener un valor negativo en Y para que la fuerza atraiga hacia el suelo
        /// </summary>
        public Vector3 GravityForce
        {
            get { return gravityForce; }
            set { gravityForce = value; }
        }

        private bool gravityEnabled;
        /// <summary>
        /// Habilita o deshabilita la aplicaci�n de fuerza de gravedad
        /// </summary>
        public bool GravityEnabled
        {
            get { return gravityEnabled; }
            set { gravityEnabled = value; }
        }

        private float slideFactor;
        /// <summary>
        /// Multiplicador de la fuerza de Sliding
        /// </summary>
        public float SlideFactor
        {
            get { return slideFactor; }
            set { slideFactor = value; }
        }


        List<TgcBoundingBox> objetosCandidatos = new List<TgcBoundingBox>();

        public SphereCollisionManager()
        {
            gravityEnabled = true;
            gravityForce = new Vector3(0, -10, 0);
            slideFactor = 1.3f;
        }

        /// <summary>
        /// Mover BoundingSphere con detecci�n de colisiones, sliding y gravedad.
        /// Se actualiza la posici�n del centrodel BoundingSphere.
        /// </summary>
        /// <param name="characterSphere">BoundingSphere del cuerpo a mover</param>
        /// <param name="movementVector">Movimiento a realizar</param>
        /// <param name="obstaculos">BoundingBox de obst�culos contra los cuales se puede colisionar</param>
        /// <returns>Desplazamiento relativo final efecutado al BoundingSphere</returns> 
        public Vector3 moveCharacter(TgcBoundingSphere characterSphere, Vector3 movementVector, List<TgcBoundingBox> obstaculos)
        {
            Vector3 originalSphereCenter = characterSphere.Center;

            //Realizar movimiento
            collideWithWorld(characterSphere, movementVector, obstaculos);

            //Aplicar gravedad
            if (gravityEnabled)
            {
                collideWithWorld(characterSphere, gravityForce, obstaculos);
            }

            return characterSphere.Center - originalSphereCenter;
        }

        /// <summary>
        /// Detecci�n de colisiones, filtrando los obstaculos que se encuentran dentro del radio de movimiento
        /// </summary>
        private void collideWithWorld(TgcBoundingSphere characterSphere, Vector3 movementVector, List<TgcBoundingBox> obstaculos)
        {
            if (movementVector.LengthSq() < EPSILON)
            {
                return;
            }

            Vector3 lastCenterSafePosition = characterSphere.Center;

            //Dejar solo los obst�culos que est�n dentro del radio de movimiento de la esfera
            Vector3 halfMovementVec = Vector3.Multiply(movementVector, 0.5f);
            TgcBoundingSphere testSphere = new TgcBoundingSphere(
                characterSphere.Center + halfMovementVec,
                halfMovementVec.Length() + characterSphere.Radius
                );
            objetosCandidatos.Clear();
            foreach (TgcBoundingBox obstaculo in obstaculos)
            {
                if (TgcCollisionUtils.testSphereAABB(testSphere, obstaculo))
                {
                    objetosCandidatos.Add(obstaculo);
                }
            }

            //Detectar colisiones y deplazar con sliding
            doCollideWithWorld(characterSphere, movementVector, objetosCandidatos, 0);


            //Manejo de error. No deberiamos colisionar con nadie si todo salio bien
            foreach (TgcBoundingBox obstaculo in objetosCandidatos)
            {
                if (TgcCollisionUtils.testSphereAABB(characterSphere, obstaculo))
                {
                    //Hubo un error, volver a la posici�n original
                    characterSphere.setCenter(lastCenterSafePosition);
                    return;
                }
            }
        }



        /// <summary>
        /// Detecci�n de colisiones recursiva
        /// </summary>
        public void doCollideWithWorld(TgcBoundingSphere characterSphere, Vector3 movementVector, List<TgcBoundingBox> obstaculos, int recursionDepth)
        {
            //Limitar recursividad
            if (recursionDepth > 5)
            {
                return;
            }

            //Ver si la distancia a recorrer es para tener en cuenta
            float distanceToTravelSq = movementVector.LengthSq();
            if (distanceToTravelSq < EPSILON)
            {
                return;
            }

            //Posicion deseada
            Vector3 originalSphereCenter = characterSphere.Center;
            Vector3 nextSphereCenter = originalSphereCenter + movementVector;

            //Buscar el punto de colision mas cercano de todos los objetos candidatos
            float minCollisionDistSq = float.MaxValue;
            Vector3 realMovementVector = movementVector;
            TgcBoundingBox.Face collisionFace = null;
            TgcBoundingBox collisionObstacle = null;
            Vector3 nearestPolygonIntersectionPoint = Vector3.Empty;
            foreach (TgcBoundingBox obstaculoBB in obstaculos)
            {
                //Obtener los pol�gonos que conforman las 6 caras del BoundingBox
                TgcBoundingBox.Face[] bbFaces = obstaculoBB.computeFaces();

                foreach (TgcBoundingBox.Face bbFace in bbFaces)
                {
                    Vector3 pNormal = TgcCollisionUtils.getPlaneNormal(bbFace.Plane);

                    TgcRay movementRay = new TgcRay(originalSphereCenter, movementVector);
                    float brutePlaneDist;
                    Vector3 brutePlaneIntersectionPoint;
                    if (!TgcCollisionUtils.intersectRayPlane(movementRay, bbFace.Plane, out brutePlaneDist, out brutePlaneIntersectionPoint))
                    {
                        continue;
                    }

                    float movementRadiusLengthSq = Vector3.Multiply(movementVector, characterSphere.Radius).LengthSq();
                    if (brutePlaneDist * brutePlaneDist > movementRadiusLengthSq)
                    {
                        continue;
                    }


                    //Obtener punto de colisi�n en el plano, seg�n la normal del plano
                    float pDist;
                    Vector3 planeIntersectionPoint;
                    Vector3 sphereIntersectionPoint;
                    TgcRay planeNormalRay = new TgcRay(originalSphereCenter, -pNormal);
                    bool embebbed = false;
                    bool collisionFound = false;
                    if (TgcCollisionUtils.intersectRayPlane(planeNormalRay, bbFace.Plane, out pDist, out planeIntersectionPoint))
                    {
                        //Ver si el plano est� embebido en la esfera
                        if (pDist <= characterSphere.Radius)
                        {
                            embebbed = true;

                            //TODO: REVISAR ESTO, caso embebido a analizar con m�s detalle
                            sphereIntersectionPoint = originalSphereCenter - pNormal * characterSphere.Radius;
                        }
                        //Esta fuera de la esfera
                        else
                        {
                            //Obtener punto de colisi�n del contorno de la esfera seg�n la normal del plano
                            sphereIntersectionPoint = originalSphereCenter - Vector3.Multiply(pNormal, characterSphere.Radius);

                            //Disparar un rayo desde el contorno de la esfera hacia el plano, con el vector de movimiento
                            TgcRay sphereMovementRay = new TgcRay(sphereIntersectionPoint, movementVector);
                            if (!TgcCollisionUtils.intersectRayPlane(sphereMovementRay, bbFace.Plane, out pDist, out planeIntersectionPoint))
                            {
                                //no hay colisi�n
                                continue;
                            }
                        }

                        //Ver si planeIntersectionPoint pertenece al pol�gono
                        Vector3 newMovementVector;
                        float newMoveDistSq;
                        Vector3 polygonIntersectionPoint;
                        if (pointInBounbingBoxFace(planeIntersectionPoint, bbFace))
                        {
                            if (embebbed)
                            {
                                //TODO: REVISAR ESTO, nunca deber�a pasar
                                //throw new Exception("El pol�gono est� dentro de la esfera");
                            }

                            polygonIntersectionPoint = planeIntersectionPoint;
                            collisionFound = true;
                        }
                        else
                        {
                            //Buscar el punto mas cercano planeIntersectionPoint que tiene el pol�gono real de esta cara
                            polygonIntersectionPoint = TgcCollisionUtils.closestPointRectangle3d(planeIntersectionPoint,
                                bbFace.Extremes[0], bbFace.Extremes[1], bbFace.Extremes[2]);

                            //Revertir el vector de velocidad desde el nuevo polygonIntersectionPoint para ver donde colisiona la esfera, si es que llega
                            Vector3 reversePointSeg = polygonIntersectionPoint - movementVector;
                            if (TgcCollisionUtils.intersectSegmentSphere(polygonIntersectionPoint, reversePointSeg, characterSphere, out pDist, out sphereIntersectionPoint))
                            {
                                collisionFound = true;
                            }
                        }

                        if (collisionFound)
                        {
                            //Nuevo vector de movimiento acotado
                            newMovementVector = polygonIntersectionPoint - sphereIntersectionPoint;
                            newMoveDistSq = newMovementVector.LengthSq();

                            if (newMoveDistSq <= distanceToTravelSq && newMoveDistSq < minCollisionDistSq)
                            {
                                minCollisionDistSq = newMoveDistSq;
                                realMovementVector = newMovementVector;
                                nearestPolygonIntersectionPoint = polygonIntersectionPoint;
                                collisionFace = bbFace;
                                collisionObstacle = obstaculoBB;

                            }
                        }
                    }
                }
            }

            //Si nunca hubo colisi�n, avanzar todo lo requerido
            if (collisionFace == null)
            {
                //Avanzar hasta muy cerca
                float movementLength = movementVector.Length();
                movementVector.Multiply((movementLength - EPSILON) / movementLength);
                characterSphere.moveCenter(movementVector);
                return;
            }

            //Solo movernos si ya no estamos muy cerca
            if (minCollisionDistSq >= EPSILON)
            {
                //Mover el BoundingSphere hasta casi la nueva posici�n real
                float movementLength = realMovementVector.Length();
                realMovementVector.Multiply((movementLength - EPSILON) / movementLength);
                characterSphere.moveCenter(realMovementVector);
            }



            //Calcular plano de Sliding
            Vector3 slidePlaneOrigin = nearestPolygonIntersectionPoint;
            Vector3 slidePlaneNormal = characterSphere.Center - nearestPolygonIntersectionPoint;
            slidePlaneNormal.Normalize();

            Plane slidePlane = Plane.FromPointNormal(slidePlaneOrigin, slidePlaneNormal);

            //Proyectamos el punto original de destino en el plano de sliding
            TgcRay slideRay = new TgcRay(nearestPolygonIntersectionPoint + Vector3.Multiply(movementVector, slideFactor), slidePlaneNormal);
            float slideT;
            Vector3 slideDestinationPoint;

            if (TgcCollisionUtils.intersectRayPlane(slideRay, slidePlane, out slideT, out slideDestinationPoint))
            {
                //Nuevo vector de movimiento
                Vector3 slideMovementVector = slideDestinationPoint - nearestPolygonIntersectionPoint;

                if (slideMovementVector.LengthSq() < EPSILON)
                {
                    return;
                }

                //Recursividad para aplicar sliding
                doCollideWithWorld(characterSphere, slideMovementVector, obstaculos, recursionDepth + 1);
            }
        }

        /// <summary>
        /// Ver si un punto pertenece a una cara de un BoundingBox
        /// </summary>
        /// <returns>True si pertenece</returns>
        private bool pointInBounbingBoxFace(Vector3 p, TgcBoundingBox.Face bbFace)
        {
            Vector3 min = bbFace.Extremes[0];
            Vector3 max = bbFace.Extremes[3];

            return p.X >= min.X && p.Y >= min.Y && p.Z >= min.Z &&
               p.X <= max.X && p.Y <= max.Y && p.Z <= max.Z;
        }




    }
}
