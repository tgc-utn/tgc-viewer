using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using System;
using TGC.Core.Geometries;
using TGC.Viewer;

namespace TGC.Examples.Quake3Loader
{
    /// <summary>
    ///     Herramienta para manipular las colisiones en un escenario BSP.
    ///     Código basado en el siguiente artículo: http://www.devmaster.net/articles/quake3collision/
    ///     y en código de Quake 3 para WebGL: http://blog.tojicode.com/2010/08/rendering-quake-3-maps-with-webgl-demo.html
    ///     Aún posee muchos puntos a mejorar.
    ///     Autor: Martin Giachetti
    /// </summary>
    public class BspCollisionManager
    {
        private readonly BspMap bspMap;
        private Vector3 antCamPos;

        // Almacena si colisiono o no
        private bool bCollided;

        private bool bTryStep;

        private bool firstTime;

        private float time;
        private float traceRadius;

        //Variables de colision
        private float traceRatio;

        private int traceType;
        private Vector3 vCollisionNormal;
        private Vector3 velocidad;
        private Vector3 vExtents;

        //Boundingbox de la camara
        private Vector3 vTraceMaxs;

        private Vector3 vTraceMins;

        /// <summary>
        ///     Crear nuevo manejador de colsiones
        /// </summary>
        public BspCollisionManager(BspMap bspMap)
        {
            this.bspMap = bspMap;
            Camera = new Q3FpsCamera();

            JumpSpeed = 80.0f;
            Gravity = 80.0f;
            MaxStepHeight = 40;

            NoClip = false;
            velocidad = new Vector3();
            antCamPos = Vector3.Empty;
            time = 0;
            firstTime = true;
            PlayerBB = new TgcBoundingBox(new Vector3(-20, -60, -20), new Vector3(20, 20, 20));
        }

        /// <summary>
        ///     Posicion inicial en el mapa
        /// </summary>
        public Vector3 InitialPos { get; set; }

        /// <summary>
        ///     Camara FPS especial para BSP
        /// </summary>
        public Q3FpsCamera Camera { get; }

        /// <summary>
        ///     Indica si la camara esta en el piso o saltando
        /// </summary>
        private bool OnGround { get; set; }

        /// <summary>
        ///     Velocidad de salto
        /// </summary>
        public float JumpSpeed { get; set; }

        /// <summary>
        ///     Gravedad
        /// </summary>
        public float Gravity { get; set; }

        /// <summary>
        ///     Permite atravesar paredes
        /// </summary>
        public bool NoClip { get; set; }

        /// <summary>
        ///     Máxima altura permitida para trepar
        /// </summary>
        public float MaxStepHeight { get; set; }

        /// <summary>
        ///     AABB que representa el volumen del jugador o camara
        /// </summary>
        public TgcBoundingBox PlayerBB { get; set; }

        /// <summary>
        ///     Configurar la cámara en la posicion inicial
        /// </summary>
        public void initCamera()
        {
            Camera.setCamera(InitialPos, InitialPos + new Vector3(1.0f, 0.0f, 0.0f));
        }

        /// <summary>
        ///     Actualizar colisiones y camara
        /// </summary>
        /// <returns>Nueva posicion de la camara</returns>
        public Vector3 update()
        {
            var elapsedTime = GuiController.Instance.ElapsedTime;

            Camera.updateCamera(elapsedTime);

            //Capturar eventos de algunas teclas

            //Jump
            if (GuiController.Instance.D3dInput.keyPressed(Key.Space))
            {
                //Salta si esta en el piso
                if (OnGround)
                {
                    //Vector3 velocity = GuiController.Instance.FpsCamera.Velocity;
                    velocidad.Y = JumpSpeed;
                }
            }

            time += elapsedTime;
            var camPos = Camera.getPosition();
            var camLookAt = Camera.getLookAt();

            /*
            if (noClip)
                camera.JumpSpeed = 100.0f;
            else
                camera.JumpSpeed = 0f;
            */

            //Detecto las colisiones
            if (!firstTime && !NoClip)
            {
                var lookDir = camLookAt - camPos;

                //aplico la velocidad
                var aceleracion = new Vector3(0, -Gravity, 0);

                //aplico la gravedad
                velocidad = velocidad + elapsedTime * aceleracion;

                camPos = camPos + velocidad * elapsedTime;
                camPos.Y -= kEpsilon * 1.5f;

                //aplico las colisiones
                //traceType = TYPE_SPHERE;

                //camPos = TraceSphere(antCamPos, camPos, 25.0f);
                camPos = TraceBox(antCamPos, camPos, PlayerBB.PMin, PlayerBB.PMax);

                //dist
                if (bCollided)
                {
                    //float dist = Vector3.Dot(vCollisionNormal, velocidad);
                    //velocidad = velocidad - vCollisionNormal*dist;
                }

                // actualizo la posicion de la camara
                //Camara.setCamera(camPos, camPos + lookDir);
                Camera.move(camPos - Camera.getPosition());

                if (!OnGround)
                {
                    //Vector3 aceleracion = new Vector3(0, -9.8f, 0);
                    //aplico la gravidad
                    //velocidad = velocidad + elapsedTime*aceleracion;
                }
                else
                {
                    if (velocidad.Y < 0)
                        velocidad.Y = 0;
                }
            }

            antCamPos = camPos;
            firstTime = false;

            return camPos;
        }

        /// <summary>
        ///     Posicion actual
        /// </summary>
        public Vector3 getCurrentPosition()
        {
            return Camera.getPosition();
        }

        #region Metodos de colision de Quake 3 para BSP

        /// <summary>
        ///     Recorrer el BSP desde un punto de inicio hasta un punto de fin y detectar colisiones
        /// </summary>
        private Vector3 Trace(Vector3 vStart, Vector3 vEnd)
        {
            // Initially we set our trace ratio to 1.0f, which means that we don't have
            // a collision or intersection point, so we can move freely.
            traceRatio = 1.0f;
            vCollisionNormal = Vector3.Empty;

            // We start out with the first node (0), setting our start and end ratio to 0 and 1.
            // We will recursively go through all of the nodes to see which brushes we should check.
            CheckNode(0, 0.0f, 1.0f, vStart, vEnd);

            // If the traceRatio is STILL 1.0f, then we never collided and just return our end position
            if (traceRatio == 1.0f)
            {
                return vEnd;
            }
            // If we get here then it's assumed that we collided and need to move the position
            // the correct distance from the starting position a position around the intersection
            // point.  This is done by the cool equation below (described in detail at top of page).

            // Set our new position to a position that is right up to the brush we collided with
            var vNewPosition = vStart + (vEnd - vStart) * traceRatio;

            //Aplico el Sliding
            var vMove = vEnd - vNewPosition;

            var distance = Vector3.Dot(vMove, vCollisionNormal);

            var vEndPosition = vEnd - vCollisionNormal * distance;

            //como me movi, Hay que detectar si hubo otra colision
            vNewPosition = Trace(vNewPosition, vEndPosition);

            if (vCollisionNormal.Y > 0.2f || OnGround)
                OnGround = true;
            else
                OnGround = false;

            // Return the new position to be used by our camera (or player))
            return vNewPosition;
        }

        private readonly int TYPE_RAY = 0;
        private readonly int TYPE_SPHERE = 1;
        private readonly int TYPE_BOX = 2;
        private const float kEpsilon = 0.03125f;

        /// <summary>
        ///     Takes a start and end position (ray) to test against the BSP brushes
        /// </summary>
        private Vector3 TraceRay(Vector3 vStart, Vector3 vEnd)
        {
            // We don't use this function, but we set it up to allow us to just check a
            // ray with the BSP tree brushes.  We do so by setting the trace type to TYPE_RAY.
            traceType = TYPE_RAY;

            // Run the normal Trace() function with our start and end
            // position and return a new position
            return Trace(vStart, vEnd);
        }

        /// <summary>
        ///     Tests a sphere around our movement vector against the BSP brushes for collision
        /// </summary>
        private Vector3 TraceSphere(Vector3 vStart, Vector3 vEnd, float radius)
        {
            // In this tutorial we are doing sphere collision, so this is the function
            // that we will be doing to initiate our collision checks.

            // Here we initialize the type of trace (SPHERE) and initialize other data
            traceType = TYPE_SPHERE;
            bCollided = false;
            traceRadius = radius;
            OnGround = false;
            bTryStep = false;

            // Get the new position that we will return to the camera or player
            var vNewPosition = Trace(vStart, vEnd);

            // Se fija si colisiono con algo y si puede intentar subirse
            if (bCollided && bTryStep)
            {
                vNewPosition = TryToStep(vNewPosition, vEnd);
            }

            // Retorna la nueva posicion de la camara del jugador
            return vNewPosition;
        }

        /// <summary>
        ///     Tests a BoundingBox around our movement vector against the BSP brushes for collision
        /// </summary>
        private Vector3 TraceBox(Vector3 vStart, Vector3 vEnd, Vector3 vMin, Vector3 vMax)
        {
            traceType = TYPE_BOX; // Set the trace type to a BOX
            vTraceMaxs = vMax; // Set the max value of our AABB
            vTraceMins = vMin; // Set the min value of our AABB
            bCollided = false; // Reset the collised flag
            OnGround = false;
            bTryStep = false;

            // Grab the extend of our box (the largest size for each x, y, z axis)
            vExtents = new Vector3(-vTraceMins.X > vTraceMaxs.X ? -vTraceMins.X : vTraceMaxs.X,
                -vTraceMins.Y > vTraceMaxs.Y ? -vTraceMins.Y : vTraceMaxs.Y,
                -vTraceMins.Z > vTraceMaxs.Z ? -vTraceMins.Z : vTraceMaxs.Z);

            // Check if our movement collided with anything, then get back our new position
            var vNewPosition = Trace(vStart, vEnd);

            // Se fija si colisiono con algo y si puede intentar subirse
            if (bCollided && bTryStep)
            {
                vNewPosition = TryToStep(vNewPosition, vEnd);
            }

            // Return our new position
            return vNewPosition;
        }

        /// <summary>
        ///     Traverses the BSP to find the brushes closest to our position
        /// </summary>
        private void CheckNode(int nodeIndex, float startRatio, float endRatio, Vector3 vStart, Vector3 vEnd)
        {
            // Remember, the nodeIndices are stored as negative numbers when we get to a leaf, so we
            // check if the current node is a leaf, which holds brushes.  If the nodeIndex is negative,
            // the next index is a leaf (note the: nodeIndex + 1)
            if (nodeIndex < 0)
            {
                // If this node in the BSP is a leaf, we need to negate and add 1 to offset
                // the real node index into the m_pLeafs[] array.  You could also do [~nodeIndex].
                var pLeaf = bspMap.Data.leafs[~nodeIndex];

                // We have a leaf, so let's go through all of the brushes for that leaf
                for (var i = 0; i < pLeaf.numLeafBrushes; i++)
                {
                    // Get the current brush that we going to check
                    var pBrush = bspMap.Data.brushes[bspMap.Data.leafbrushes[pLeaf.firstLeafBrush + i]];

                    // This is kind of an important line.  First, we check if there is actually
                    // and brush sides (which store indices to the normal and plane data for the brush).
                    // If not, then go to the next one.  Otherwise, we also check to see if the brush
                    // is something that we want to collide with.  For instance, there are brushes for
                    // water, lava, bushes, misc. sprites, etc...  We don't want to collide with water
                    // and other things like bushes, so we check the texture type to see if it's a solid.
                    // If the textureType can be binary-anded (&) and still be 1, then it's solid,
                    // otherwise it's something that can be walked through.  That's how Quake chose to
                    // do it.

                    // Check if we have brush sides and the current brush is solid and collidable
                    if ((pBrush.numSides > 0) && (bspMap.Data.shaders[pBrush.shaderNum].contentFlags & 1) > 0)
                    {
                        // Now we delve into the dark depths of the real calculations for collision.
                        // We can now check the movement vector against our brush planes.
                        CheckBrush(pBrush, vStart, vEnd);
                    }
                }

                // Since we found the brushes, we can go back up and stop recursing at this level
                return;
            }

            // If we haven't found a leaf in the node, then we need to keep doing some dirty work
            // until we find the leafs which store the brush information for collision detection.

            // Grad the next node to work with and grab this node's plane data
            var pNode = bspMap.Data.nodes[nodeIndex];
            var pPlane = bspMap.Data.planes[pNode.planeNum];

            // Now we do some quick tests to see which side we fall on of the node in the BSP

            // Here we use the plane equation to find out where our initial start position is
            // according the the node that we are checking.  We then grab the same info for the end pos.
            var startDistance = Vector3.Dot(vStart, pPlane.normal) - pPlane.dist;
            var endDistance = Vector3.Dot(vEnd, pPlane.normal) - pPlane.dist;
            var offset = 0.0f;

            // If we are doing any type of collision detection besides a ray, we need to change
            // the offset for which we are testing collision against the brushes.  If we are testing
            // a sphere against the brushes, we need to add the sphere's offset when we do the plane
            // equation for testing our movement vector (start and end position).  * More Info * For
            // more info on sphere collision, check out our tutorials on this subject.

            // If we are doing sphere collision, include an offset for our collision tests below
            if (traceType == TYPE_SPHERE)
                offset = traceRadius;
            else if (traceType == TYPE_BOX)
            {
                // This equation does a dot product to see how far our
                // AABB is away from the current plane we are checking.
                // Since this is a distance, we need to make it an absolute
                // value, which calls for the fabs() function (abs() for floats).

                // Get the distance our AABB is from the current splitter plane
                offset = Math.Abs(vExtents.X * pPlane.normal.X) +
                         Math.Abs(vExtents.Y * pPlane.normal.Y) +
                         Math.Abs(vExtents.Z * pPlane.normal.Z);
            }

            // Below we just do a basic traversal down the BSP tree.  If the points are in
            // front of the current splitter plane, then only check the nodes in front of
            // that splitter plane.  Otherwise, if both are behind, check the nodes that are
            // behind the current splitter plane.  The next case is that the movement vector
            // is on both sides of the splitter plane, which makes it a bit more tricky because we now
            // need to check both the front and the back and split up the movement vector for both sides.

            // Here we check to see if the start and end point are both in front of the current node.
            // If so, we want to check all of the nodes in front of this current splitter plane.
            if (startDistance >= offset && endDistance >= offset)
            {
                // Traverse the BSP tree on all the nodes in front of this current splitter plane
                CheckNode(pNode.children[0], startDistance, endDistance, vStart, vEnd);
            }
            // If both points are behind the current splitter plane, traverse down the back nodes
            else if (startDistance < -offset && endDistance < -offset)
            {
                // Traverse the BSP tree on all the nodes in back of this current splitter plane
                CheckNode(pNode.children[1], startDistance, endDistance, vStart, vEnd);
            }
            else
            {
                // If we get here, then our ray needs to be split in half to check the nodes
                // on both sides of the current splitter plane.  Thus we create 2 ratios.
                float Ratio1 = 1.0f, Ratio2 = 0.0f, middleRatio = 0.0f;
                Vector3 vMiddle; // This stores the middle point for our split ray

                // Start of the side as the front side to check
                var side = pNode.children[0];

                // Here we check to see if the start point is in back of the plane (negative)
                if (startDistance < endDistance)
                {
                    // Since the start position is in back, let's check the back nodes
                    side = pNode.children[1];

                    // Here we create 2 ratios that hold a distance from the start to the
                    // extent closest to the start (take into account a sphere and epsilon).
                    // We use epsilon like Quake does to compensate for float errors.  The second
                    // ratio holds a distance from the other size of the extents on the other side
                    // of the plane.  This essential splits the ray for both sides of the splitter plane.
                    var inverseDistance = 1.0f / (startDistance - endDistance);
                    Ratio1 = (startDistance - offset - kEpsilon) * inverseDistance;
                    Ratio2 = (startDistance + offset + kEpsilon) * inverseDistance;
                }
                // Check if the starting point is greater than the end point (positive)
                else if (startDistance > endDistance)
                {
                    // This means that we are going to recurse down the front nodes first.
                    // We do the same thing as above and get 2 ratios for split ray.
                    // Ratio 1 and 2 are switched in contrast to the last if statement.
                    // This is because the start is starting in the front of the splitter plane.
                    var inverseDistance = 1.0f / (startDistance - endDistance);
                    Ratio1 = (startDistance + offset + kEpsilon) * inverseDistance;
                    Ratio2 = (startDistance - offset - kEpsilon) * inverseDistance;
                }

                // Make sure that we have valid numbers and not some weird float problems.
                // This ensures that we have a value from 0 to 1 as a good ratio should be :)
                if (Ratio1 < 0.0f) Ratio1 = 0.0f;
                else if (Ratio1 > 1.0f) Ratio1 = 1.0f;

                if (Ratio2 < 0.0f) Ratio2 = 0.0f;
                else if (Ratio2 > 1.0f) Ratio2 = 1.0f;

                // Just like we do in the Trace() function, we find the desired middle
                // point on the ray, but instead of a point we get a middleRatio percentage.
                // This isn't the true middle point since we are using offset's and the epsilon value.
                // We also grab the middle point to go with the ratio.
                middleRatio = startRatio + (endRatio - startRatio) * Ratio1;
                vMiddle = vStart + (vEnd - vStart) * Ratio1;

                // Now we recurse on the current side with only the first half of the ray
                CheckNode(side, startRatio, middleRatio, vStart, vMiddle);

                // Now we need to make a middle point and ratio for the other side of the node
                middleRatio = startRatio + (endRatio - startRatio) * Ratio2;
                vMiddle = vStart + (vEnd - vStart) * Ratio2;

                // Depending on which side should go last, traverse the bsp with the
                // other side of the split ray (movement vector).
                if (side == pNode.children[1])
                    CheckNode(pNode.children[0], middleRatio, endRatio, vMiddle, vEnd);
                else
                    CheckNode(pNode.children[1], middleRatio, endRatio, vMiddle, vEnd);
            }
        }

        /// <summary>
        ///     Checks our movement vector against all the planes of the brush
        /// </summary>
        private void CheckBrush(QBrush pBrush, Vector3 vStart, Vector3 vEnd)
        {
            var startRatio = -1.0f; // Like in BrushCollision.htm, start a ratio at -1
            var endRatio = 1.0f; // Set the end ratio to 1
            var startsOut = false; // This tells us if we starting outside the brush

            // This function actually does the collision detection between our movement
            // vector and the brushes in the world data.  We will go through all of the
            // brush sides and check our start and end ratio against the planes to see if
            // they pass each other.  We start the startRatio at -1 and the endRatio at
            // 1, but as we set the ratios to their intersection points (ratios), then
            // they slowly move toward each other.  If they pass each other, then there
            // is definitely not a collision.

            // Go through all of the brush sides and check collision against each plane
            for (var i = 0; i < pBrush.numSides; i++)
            {
                // Here we grab the current brush side and plane in this brush
                var pBrushSide = bspMap.Data.brushSides[pBrush.firstSide + i];
                var pPlane = bspMap.Data.planes[pBrushSide.planeNum];

                // Let's store a variable for the offset (like for sphere collision)
                var offset = 0.0f;

                // If we are testing sphere collision we need to add the sphere radius
                if (traceType == TYPE_SPHERE)
                    offset = traceRadius;

                // Test the start and end points against the current plane of the brush side.
                // Notice that we add an offset to the distance from the origin, which makes
                // our sphere collision work.
                var startDistance = Vector3.Dot(vStart, pPlane.normal) - (pPlane.dist + offset);
                var endDistance = Vector3.Dot(vEnd, pPlane.normal) - (pPlane.dist + offset);

                // This is the last beefy part of code in this tutorial.  In this
                // section we need to do a few special checks to see which extents
                // we should use.  We do this by checking the x,y,z of the normal.
                // If the vNormal.x is less than zero, we want to use the Max.x
                // value, otherwise use the Min.x value.  We do these checks because
                // we want the corner of the bounding box that is closest to the plane to
                // test for collision.  Write it down on paper and see how this works.
                // We want to grab the closest corner (x, y, or z value that is...) so we
                // dont go through the wall.  This works because the bounding box is axis aligned.

                // Store the offset that we will check against the plane

                // If we are using AABB collision
                if (traceType == TYPE_BOX)
                {
                    var vOffset = new Vector3();
                    // Grab the closest corner (x, y, or z value) that is closest to the plane
                    vOffset.X = pPlane.normal.X < 0 ? vTraceMaxs.X : vTraceMins.X;
                    vOffset.Y = pPlane.normal.Y < 0 ? vTraceMaxs.Y : vTraceMins.Y;
                    vOffset.Z = pPlane.normal.Z < 0 ? vTraceMaxs.Z : vTraceMins.Z;

                    // Use the plane equation to grab the distance our start position is from the plane.
                    // We need to add the offset to this to see if the box collides with the plane,
                    // even if the position doesn't.
                    startDistance = Vector3.Dot(vStart + vOffset, pPlane.normal) - pPlane.dist;

                    // Get the distance our end position is from this current brush plane
                    endDistance = Vector3.Dot(vEnd + vOffset, pPlane.normal) - pPlane.dist;
                }

                // Make sure we start outside of the brush's volume
                if (startDistance > 0) startsOut = true;

                // Stop checking since both the start and end position are in front of the plane
                if (startDistance > 0 && endDistance > 0)
                    return;

                // Continue on to the next brush side if both points are behind or on the plane
                if (startDistance <= 0 && endDistance <= 0)
                    continue;

                // If the distance of the start point is greater than the end point, we have a collision!
                if (startDistance > endDistance)
                {
                    // This gets a ratio from our starting point to the approximate collision spot
                    var Ratio1 = (startDistance - kEpsilon) / (startDistance - endDistance);

                    // If this is the first time coming here, then this will always be true,
                    // since startRatio starts at -1.0f.  We want to find the closest collision,
                    // so we still continue to check all of the brushes before quitting.
                    if (Ratio1 > startRatio)
                    {
                        // Set the startRatio (currently the closest collision distance from start)
                        startRatio = Ratio1;
                        bCollided = true; // Let us know we collided!
                        vCollisionNormal = pPlane.normal;
                        if (vCollisionNormal.Y >= 0.2f)
                            OnGround = true;

                        // This checks first tests if we actually moved along the x or z-axis,
                        // meaning that we went in a direction somewhere.  The next check makes
                        // sure that we don't always check to step every time we collide.  If
                        // the normal of the plane has a Y value of 1, that means it's just the
                        // flat ground and we don't need to check if we can step over it, it's flat!
                        if ((vStart.X != vEnd.X || vStart.Z != vEnd.Z) && pPlane.normal.Y != 1)
                        {
                            // We can try and step over the wall we collided with
                            bTryStep = true;
                        }
                    }
                }
                else
                {
                    // Get the ratio of the current brush side for the endRatio
                    var Ratio = (startDistance + kEpsilon) / (startDistance - endDistance);

                    // If the ratio is less than the current endRatio, assign a new endRatio.
                    // This will usually always be true when starting out.
                    if (Ratio < endRatio)
                        endRatio = Ratio;
                }
            }

            // If we didn't start outside of the brush we don't want to count this collision - return;
            if (startsOut == false)
            {
                return;
            }

            // If our startRatio is less than the endRatio there was a collision!!!
            if (startRatio < endRatio)
            {
                // Make sure the startRatio moved from the start and check if the collision
                // ratio we just got is less than the current ratio stored in m_traceRatio.
                // We want the closest collision to our original starting position.
                if (startRatio > -1 && startRatio < traceRatio)
                {
                    // If the startRatio is less than 0, just set it to 0
                    if (startRatio < 0)
                        startRatio = 0;

                    // Store the new ratio in our member variable for later
                    traceRatio = startRatio;
                }
            }
        }

        /// <summary>
        ///     Checks a bunch of different heights to see if we can step up
        /// </summary>
        /// <param name="vStart"></param>
        /// <param name="vEnd"></param>
        /// <returns></returns>
        private Vector3 TryToStep(Vector3 vStart, Vector3 vEnd)
        {
            // In this function we loop until we either found a reasonable height
            // that we can step over, or find out that we can't step over anything.
            // We check 10 times, each time increasing the step size to check for
            // a collision.  If we don't collide, then we climb over the step.

            // Go through and check different heights to step up
            for (var height = 1.0f; height <= MaxStepHeight; height++)
            {
                // Reset our variables for each loop interation
                bCollided = false;
                bTryStep = false;

                // Here we add the current height to our y position of a new start and end.
                // If these 2 new start and end positions are okay, we can step up.
                var vStepStart = new Vector3(vStart.X, vStart.Y + height, vStart.Z);
                var vStepEnd = new Vector3(vEnd.X, vStart.Y + height, vEnd.Z);

                // Test to see if the new position we are trying to step collides or not
                var vStepPosition = Trace(vStepStart, vStepEnd);

                // If we didn't collide, we can step!
                if (!bCollided)
                {
                    // Return the current position since we stepped up somewhere
                    return vStepPosition;
                }
            }

            // If we can't step, then we just return the original position of the collision
            return vStart;
        }

        #endregion Metodos de colision de Quake 3 para BSP
    }
}