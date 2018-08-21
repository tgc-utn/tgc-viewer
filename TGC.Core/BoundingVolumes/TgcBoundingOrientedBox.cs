using SharpDX;
using SharpDX.Direct3D9;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;

namespace TGC.Core.BoundingVolumes
{
    /// <summary>
    ///     Representa un Orientend-BoundingBox (OBB)
    /// </summary>
    public class TgcBoundingOrientedBox : IRenderObject
    {
        private bool dirtyValues;

        protected Effect effect;

        private TGCVector3 extents;

        private TGCVector3[] orientation = new TGCVector3[3];

        protected string technique;

        private CustomVertex.PositionColored[] vertices;

        /// <summary>
        ///     Construir OBB vacio
        /// </summary>
        public TgcBoundingOrientedBox()
        {
            RenderColor = Color.Yellow.ToArgb();
            dirtyValues = true;
            AlphaBlendEnable = false;
        }

        /// <summary>
        ///     Centro
        /// </summary>
        public TGCVector3 Center
        {
            get { return Position; }
            set
            {
                Position = value;
                dirtyValues = true;
            }
        }

        /// <summary>
        ///     Orientacion del OBB, expresada en local axes
        /// </summary>
        public TGCVector3[] Orientation
        {
            get { return orientation; }
            set
            {
                orientation = value;
                dirtyValues = true;
            }
        }

        /// <summary>
        ///     Radios
        /// </summary>
        public TGCVector3 Extents
        {
            get { return extents; }
            set
            {
                extents = value;
                dirtyValues = true;
            }
        }

        /// <summary>
        ///     Color de renderizado del BoundingBox.
        /// </summary>
        public int RenderColor { get; private set; }

        public TGCVector3 Position { get; private set; }

        /// <summary>
        ///     Shader del mesh
        /// </summary>
        public Effect Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        /// <summary>
        ///     Technique que se va a utilizar en el effect.
        ///     Cada vez que se llama a Render() se carga este Technique (pisando lo que el shader ya tenia seteado)
        /// </summary>
        public string Technique
        {
            get { return technique; }
            set { technique = value; }
        }

        /// <summary>
        ///     Habilita el renderizado con AlphaBlending para los modelos
        ///     con textura o colores por vértice de canal Alpha.
        ///     Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable { get; set; }

        /// <summary>
        ///     Renderizar
        /// </summary>
        public void Render()
        {
            TexturesManager.Instance.clear(0);
            TexturesManager.Instance.clear(1);

            //Cargar shader si es la primera vez
            if (effect == null)
            {
                effect = TgcShaders.Instance.VariosShader;
                technique = TgcShaders.T_POSITION_COLORED;
            }

            //Actualizar vertices de BoundingBox solo si hubo una modificación
            if (dirtyValues)
            {
                updateValues();
                dirtyValues = false;
            }

            TgcShaders.Instance.setShaderMatrixIdentity(effect);
            D3DDevice.Instance.Device.VertexDeclaration = TgcShaders.Instance.VdecPositionColored;
            effect.Technique = technique;

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.LineList, 12, vertices);
            effect.EndPass();
            effect.End();
        }

        /// <summary>
        ///     Libera los recursos del objeto
        /// </summary>
        public void Dispose()
        {
            vertices = null;
        }

        /// <summary>
        ///     Configurar el color de renderizado del OBB
        ///     Ejemplo: Color.Yellow.ToArgb();
        /// </summary>
        public void setRenderColor(Color color)
        {
            RenderColor = color.ToArgb();
            dirtyValues = true;
        }

        /// <summary>
        ///     Actualizar los valores de los vertices a renderizar
        /// </summary>
        public void updateValues()
        {
            if (vertices == null)
            {
                vertices = vertices = new CustomVertex.PositionColored[24];
            }

            var corners = computeCorners();

            //Cuadrado de atras
            vertices[0] = new CustomVertex.PositionColored(corners[0], RenderColor);
            vertices[1] = new CustomVertex.PositionColored(corners[4], RenderColor);

            vertices[2] = new CustomVertex.PositionColored(corners[0], RenderColor);
            vertices[3] = new CustomVertex.PositionColored(corners[2], RenderColor);

            vertices[4] = new CustomVertex.PositionColored(corners[2], RenderColor);
            vertices[5] = new CustomVertex.PositionColored(corners[6], RenderColor);

            vertices[6] = new CustomVertex.PositionColored(corners[4], RenderColor);
            vertices[7] = new CustomVertex.PositionColored(corners[6], RenderColor);

            //Cuadrado de adelante
            vertices[8] = new CustomVertex.PositionColored(corners[1], RenderColor);
            vertices[9] = new CustomVertex.PositionColored(corners[5], RenderColor);

            vertices[10] = new CustomVertex.PositionColored(corners[1], RenderColor);
            vertices[11] = new CustomVertex.PositionColored(corners[3], RenderColor);

            vertices[12] = new CustomVertex.PositionColored(corners[3], RenderColor);
            vertices[13] = new CustomVertex.PositionColored(corners[7], RenderColor);

            vertices[14] = new CustomVertex.PositionColored(corners[5], RenderColor);
            vertices[15] = new CustomVertex.PositionColored(corners[7], RenderColor);

            //Union de ambos cuadrados
            vertices[16] = new CustomVertex.PositionColored(corners[0], RenderColor);
            vertices[17] = new CustomVertex.PositionColored(corners[1], RenderColor);

            vertices[18] = new CustomVertex.PositionColored(corners[4], RenderColor);
            vertices[19] = new CustomVertex.PositionColored(corners[5], RenderColor);

            vertices[20] = new CustomVertex.PositionColored(corners[2], RenderColor);
            vertices[21] = new CustomVertex.PositionColored(corners[3], RenderColor);

            vertices[22] = new CustomVertex.PositionColored(corners[6], RenderColor);
            vertices[23] = new CustomVertex.PositionColored(corners[7], RenderColor);
        }

        /// <summary>
        ///     Crea un array con los 8 vertices del OBB
        /// </summary>
        private TGCVector3[] computeCorners()
        {
            var corners = new TGCVector3[8];

            var eX = extents.X * orientation[0];
            var eY = extents.Y * orientation[1];
            var eZ = extents.Z * orientation[2];

            corners[0] = Position - eX - eY - eZ;
            corners[1] = Position - eX - eY + eZ;

            corners[2] = Position - eX + eY - eZ;
            corners[3] = Position - eX + eY + eZ;

            corners[4] = Position + eX - eY - eZ;
            corners[5] = Position + eX - eY + eZ;

            corners[6] = Position + eX + eY - eZ;
            corners[7] = Position + eX + eY + eZ;

            return corners;
        }

        /// <summary>
        ///     Mueve el centro del OBB
        /// </summary>
        /// <param name="movement">Movimiento relativo que se quiere aplicar</param>
        public void move(TGCVector3 movement)
        {
            Position += movement;
            dirtyValues = true;
        }

        /// <summary>
        ///     Rotar OBB en los 3 ejes.
        ///     Es una rotacion relativa, sumando a lo que ya tenia antes de rotacion.
        /// </summary>
        /// <param name="movement">Ángulo de rotación de cada eje en radianes</param>
        public void rotate(TGCVector3 rotation)
        {
            var rotM = TGCMatrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
            var currentRotM = computeRotationMatrix();
            var newRotM = currentRotM * rotM;

            orientation[0] = new TGCVector3(newRotM.M11, newRotM.M12, newRotM.M13);
            orientation[1] = new TGCVector3(newRotM.M21, newRotM.M22, newRotM.M23);
            orientation[2] = new TGCVector3(newRotM.M31, newRotM.M32, newRotM.M33);

            dirtyValues = true;
        }

        /// <summary>
        ///     Cargar la rotacion absoluta del OBB.
        ///     Pierda la rotacion anterior.
        /// </summary>
        /// <param name="rotation">Ángulo de rotación de cada eje en radianes</param>
        public void setRotation(TGCVector3 rotation)
        {
            var rotM = TGCMatrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
            orientation[0] = new TGCVector3(rotM.M11, rotM.M12, rotM.M13);
            orientation[1] = new TGCVector3(rotM.M21, rotM.M22, rotM.M23);
            orientation[2] = new TGCVector3(rotM.M31, rotM.M32, rotM.M33);

            dirtyValues = true;
        }

        /// <summary>
        ///     Calcula la matriz de rotacion 4x4 del Obb en base a su orientacion
        /// </summary>
        /// <returns>Matriz de rotacion de 4x4</returns>
        public TGCMatrix computeRotationMatrix()
        {
            var rot = TGCMatrix.Identity;

            rot.M11 = orientation[0].X;
            rot.M12 = orientation[0].Y;
            rot.M13 = orientation[0].Z;

            rot.M21 = orientation[1].X;
            rot.M22 = orientation[1].Y;
            rot.M23 = orientation[1].Z;

            rot.M31 = orientation[2].X;
            rot.M32 = orientation[2].Y;
            rot.M33 = orientation[2].Z;

            return rot;
        }

        /// <summary>
        ///     Calcular OBB a partir de un conjunto de puntos.
        ///     Busca por fuerza bruta el mejor OBB en la mejor orientación que se ajusta a esos puntos.
        ///     Es un calculo costoso.
        /// </summary>
        /// <param name="points">puntos</param>
        /// <returns>OBB calculado</returns>
        public static TgcBoundingOrientedBox computeFromPoints(TGCVector3[] points)
        {
            return computeFromPointsRecursive(points, TGCVector3.Empty, new TGCVector3(360, 360, 360), 10f).toClass();
        }

        /// <summary>
        ///     Calcular OBB a partir de un conjunto de puntos.
        ///     Prueba todas las orientaciones entre initValues y endValues, saltando de angulo en cada intervalo segun step
        ///     Continua recursivamente hasta llegar a un step menor a 0.01f
        /// </summary>
        /// <returns></returns>
        private static OBBStruct computeFromPointsRecursive(TGCVector3[] points, TGCVector3 initValues, TGCVector3 endValues,
            float step)
        {
            var minObb = new OBBStruct();
            var minVolume = float.MaxValue;

            var minInitValues = TGCVector3.Empty;
            var minEndValues = TGCVector3.Empty;
            var transformedPoints = new TGCVector3[points.Length];

            float x, y, z;

            x = initValues.X;
            while (x <= endValues.X)
            {
                y = initValues.Y;
                var rotX = FastMath.ToRad(x);
                while (y <= endValues.Y)
                {
                    z = initValues.Z;
                    var rotY = FastMath.ToRad(y);
                    while (z <= endValues.Z)
                    {
                        //Matriz de rotacion
                        var rotZ = FastMath.ToRad(z);
                        var rotM = TGCMatrix.RotationYawPitchRoll(rotY, rotX, rotZ);
                        TGCVector3[] orientation =
                        {
                            new TGCVector3(rotM.M11, rotM.M12, rotM.M13),
                            new TGCVector3(rotM.M21, rotM.M22, rotM.M23),
                            new TGCVector3(rotM.M31, rotM.M32, rotM.M33)
                        };

                        //Transformar todos los puntos a OBB-space
                        for (var i = 0; i < transformedPoints.Length; i++)
                        {
                            transformedPoints[i].X = TGCVector3.Dot(points[i], orientation[0]);
                            transformedPoints[i].Y = TGCVector3.Dot(points[i], orientation[1]);
                            transformedPoints[i].Z = TGCVector3.Dot(points[i], orientation[2]);
                        }

                        //Obtener el AABB de todos los puntos transformados
                        var aabb = TgcBoundingAxisAlignBox.computeFromPoints(transformedPoints);

                        //Calcular volumen del AABB
                        var extents = aabb.calculateAxisRadius();
                        extents = TGCVector3.Abs(extents);
                        var volume = extents.X * 2 * extents.Y * 2 * extents.Z * 2;

                        //Buscar menor volumen
                        if (volume < minVolume)
                        {
                            minVolume = volume;
                            minInitValues = new TGCVector3(x, y, z);
                            minEndValues = new TGCVector3(x + step, y + step, z + step);

                            //Volver centro del AABB a World-space
                            var center = aabb.calculateBoxCenter();
                            center = center.X * orientation[0] + center.Y * orientation[1] + center.Z * orientation[2];

                            //Crear OBB
                            minObb.center = center;
                            minObb.extents = extents;
                            minObb.orientation = orientation;
                        }

                        z += step;
                    }
                    y += step;
                }
                x += step;
            }

            //Recursividad en mejor intervalo encontrado
            if (step > 0.01f)
            {
                minObb = computeFromPointsRecursive(points, minInitValues, minEndValues, step / 10f);
            }

            return minObb;
        }

        /// <summary>
        ///     Generar OBB a partir de AABB
        /// </summary>
        /// <param name="aabb">BoundingBox</param>
        /// <returns>OBB generado</returns>
        public static TgcBoundingOrientedBox computeFromAABB(TgcBoundingAxisAlignBox aabb)
        {
            return computeFromAABB(aabb.toStruct()).toClass();
        }

        /// <summary>
        ///     Generar OBB a partir de AABB
        /// </summary>
        /// <param name="aabb">BoundingBox</param>
        /// <returns>OBB generado</returns>
        public static OBBStruct computeFromAABB(TgcBoundingAxisAlignBox.AABBStruct aabb)
        {
            var obb = new OBBStruct();
            obb.extents = (aabb.max - aabb.min) * 0.5f;
            obb.center = aabb.min + obb.extents;

            obb.orientation = new[] { new TGCVector3(1, 0, 0), TGCVector3.Up, new TGCVector3(0, 0, 1) };
            return obb;
        }

        /// <summary>
        ///     Convertir un punto de World-Space espacio de coordenadas del OBB (OBB-Space)
        /// </summary>
        /// <param name="p">Punto en World-space</param>
        /// <returns>Punto convertido a OBB-space</returns>
        public TGCVector3 toObbSpace(TGCVector3 p)
        {
            var t = p - Position;
            return new TGCVector3(TGCVector3.Dot(t, orientation[0]), TGCVector3.Dot(t, orientation[1]),
                TGCVector3.Dot(t, orientation[2]));
        }

        /// <summary>
        ///     Convertir un punto de OBB-space a World-space
        /// </summary>
        /// <param name="p">Punto en OBB-space</param>
        /// <returns>Punto convertido a World-space</returns>
        public TGCVector3 toWorldSpace(TGCVector3 p)
        {
            return Position + p.X * orientation[0] + p.Y * orientation[1] + p.Z * orientation[2];
        }

        /// <summary>
        ///     Convertir a struct
        /// </summary>
        public OBBStruct toStruct()
        {
            var obbStruct = new OBBStruct();
            obbStruct.center = Position;
            obbStruct.orientation = orientation;
            obbStruct.extents = extents;
            return obbStruct;
        }

        /// <summary>
        ///     OBB en un struct liviano
        /// </summary>
        public struct OBBStruct
        {
            public TGCVector3 center;
            public TGCVector3[] orientation;
            public TGCVector3 extents;

            /// <summary>
            ///     Convertir a clase
            /// </summary>
            public TgcBoundingOrientedBox toClass()
            {
                var obb = new TgcBoundingOrientedBox();
                obb.Position = center;
                obb.orientation = orientation;
                obb.extents = extents;
                return obb;
            }

            /// <summary>
            ///     Convertir un punto de World-Space espacio de coordenadas del OBB (OBB-Space)
            /// </summary>
            /// <param name="p">Punto en World-space</param>
            /// <returns>Punto convertido a OBB-space</returns>
            public TGCVector3 toObbSpace(TGCVector3 p)
            {
                var t = p - center;
                return new TGCVector3(TGCVector3.Dot(t, orientation[0]), TGCVector3.Dot(t, orientation[1]),
                    TGCVector3.Dot(t, orientation[2]));
            }

            /// <summary>
            ///     Convertir un punto de OBB-space a World-space
            /// </summary>
            /// <param name="p">Punto en OBB-space</param>
            /// <returns>Punto convertido a World-space</returns>
            public TGCVector3 toWorldSpace(TGCVector3 p)
            {
                return center + p.X * orientation[0] + p.Y * orientation[1] + p.Z * orientation[2];
            }
        }
    }
}