using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TgcViewer;
using TgcViewer.Utils;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Shaders;

namespace TgcViewer.Utils.TgcGeometry
{
    /// <summary>
    /// Representa un Orientend-BoundingBox (OBB)
    /// </summary>
    public class TgcObb : IRenderObject
    {

        Vector3 center;
        /// <summary>
        /// Centro
        /// </summary>
        public Vector3 Center
        {
            get { return center; }
            set { 
                center = value;
                dirtyValues = true;
            }
        }

        Vector3[] orientation = new Vector3[3];
        /// <summary>
        /// Orientacion del OBB, expresada en local axes
        /// </summary>
        public Vector3[] Orientation
        {
            get { return orientation; }
            set { 
                orientation = value;
                dirtyValues = true;
            }
        }

        Vector3 extents;
        /// <summary>
        /// Radios
        /// </summary>
        public Vector3 Extents
        {
            get { return extents; }
            set { 
                extents = value;
                dirtyValues = true;
            }
        }


        int renderColor;
        /// <summary>
        /// Color de renderizado del BoundingBox.
        /// </summary>
        public int RenderColor
        {
            get { return renderColor; }
        }

        public Vector3 Position
        {
            get { return center; }
        }

        private bool alphaBlendEnable;
        /// <summary>
        /// Habilita el renderizado con AlphaBlending para los modelos
        /// con textura o colores por vértice de canal Alpha.
        /// Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable
        {
            get { return alphaBlendEnable; }
            set { alphaBlendEnable = value; }
        }

        protected Effect effect;
        /// <summary>
        /// Shader del mesh
        /// </summary>
        public Effect Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        protected string technique;
        /// <summary>
        /// Technique que se va a utilizar en el effect.
        /// Cada vez que se llama a render() se carga este Technique (pisando lo que el shader ya tenia seteado)
        /// </summary>
        public string Technique
        {
            get { return technique; }
            set { technique = value; }
        }



        CustomVertex.PositionColored[] vertices;
        bool dirtyValues;

        /// <summary>
        /// Construir OBB vacio
        /// </summary>
        public TgcObb()
        {
            renderColor = Color.Yellow.ToArgb();
            dirtyValues = true;
            alphaBlendEnable = false;
        }





        /// <summary>
        /// Configurar el color de renderizado del OBB
        /// Ejemplo: Color.Yellow.ToArgb();
        /// </summary>
        public void setRenderColor(Color color)
        {
            this.renderColor = color.ToArgb();
            dirtyValues = true;
        }

        /// <summary>
        /// Actualizar los valores de los vertices a renderizar
        /// </summary>
        public void updateValues()
        {
            if (vertices == null)
            {
                vertices = vertices = new CustomVertex.PositionColored[24];
            }

            Vector3[] corners = computeCorners(); 


            //Cuadrado de atras
            vertices[0] = new CustomVertex.PositionColored(corners[0], renderColor);
            vertices[1] = new CustomVertex.PositionColored(corners[4], renderColor);

            vertices[2] = new CustomVertex.PositionColored(corners[0], renderColor);
            vertices[3] = new CustomVertex.PositionColored(corners[2], renderColor);

            vertices[4] = new CustomVertex.PositionColored(corners[2], renderColor);
            vertices[5] = new CustomVertex.PositionColored(corners[6], renderColor);

            vertices[6] = new CustomVertex.PositionColored(corners[4], renderColor);
            vertices[7] = new CustomVertex.PositionColored(corners[6], renderColor);

            //Cuadrado de adelante
            vertices[8] = new CustomVertex.PositionColored(corners[1], renderColor);
            vertices[9] = new CustomVertex.PositionColored(corners[5], renderColor);

            vertices[10] = new CustomVertex.PositionColored(corners[1], renderColor);
            vertices[11] = new CustomVertex.PositionColored(corners[3], renderColor);

            vertices[12] = new CustomVertex.PositionColored(corners[3], renderColor);
            vertices[13] = new CustomVertex.PositionColored(corners[7], renderColor);

            vertices[14] = new CustomVertex.PositionColored(corners[5], renderColor);
            vertices[15] = new CustomVertex.PositionColored(corners[7], renderColor);

            //Union de ambos cuadrados
            vertices[16] = new CustomVertex.PositionColored(corners[0], renderColor);
            vertices[17] = new CustomVertex.PositionColored(corners[1], renderColor);

            vertices[18] = new CustomVertex.PositionColored(corners[4], renderColor);
            vertices[19] = new CustomVertex.PositionColored(corners[5], renderColor);

            vertices[20] = new CustomVertex.PositionColored(corners[2], renderColor);
            vertices[21] = new CustomVertex.PositionColored(corners[3], renderColor);

            vertices[22] = new CustomVertex.PositionColored(corners[6], renderColor);
            vertices[23] = new CustomVertex.PositionColored(corners[7], renderColor);
        }

        /// <summary>
        /// Crea un array con los 8 vertices del OBB
        /// </summary>
        private Vector3[] computeCorners()
        {
            Vector3[] corners = new Vector3[8];

            Vector3 eX = extents.X * orientation[0];
            Vector3 eY = extents.Y * orientation[1];
            Vector3 eZ = extents.Z * orientation[2];

            corners[0] = center - eX - eY - eZ;
            corners[1] = center - eX - eY + eZ;

            corners[2] = center - eX + eY - eZ;
            corners[3] = center - eX + eY + eZ;

            corners[4] = center + eX - eY - eZ;
            corners[5] = center + eX - eY + eZ;

            corners[6] = center + eX + eY - eZ;
            corners[7] = center + eX + eY + eZ;

            return corners;
        }

        /// <summary>
        /// Renderizar
        /// </summary>
        public void render()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            texturesManager.clear(0);
            texturesManager.clear(1);

            //Cargar shader si es la primera vez
            if (this.effect == null)
            {
                this.effect = GuiController.Instance.Shaders.VariosShader;
                this.technique = TgcShaders.T_POSITION_COLORED;
            }

            //Actualizar vertices de BoundingBox solo si hubo una modificación
            if (dirtyValues)
            {
                updateValues();
                dirtyValues = false;
            }

            GuiController.Instance.Shaders.setShaderMatrixIdentity(this.effect);
            d3dDevice.VertexDeclaration = GuiController.Instance.Shaders.VdecPositionColored;
            effect.Technique = this.technique;

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            d3dDevice.DrawUserPrimitives(PrimitiveType.LineList, 12, vertices);
            effect.EndPass();
            effect.End();
        }

        /// <summary>
        /// Libera los recursos del objeto
        /// </summary>
        public void dispose()
        {
            vertices = null;
        }

        /// <summary>
        /// Mueve el centro del OBB
        /// </summary>
        /// <param name="movement">Movimiento relativo que se quiere aplicar</param>
        public void move(Vector3 movement)
        {
            center += movement;
            dirtyValues = true;
        }

        /// <summary>
        /// Rotar OBB en los 3 ejes.
        /// Es una rotacion relativa, sumando a lo que ya tenia antes de rotacion.
        /// </summary>
        /// <param name="movement">Ángulo de rotación de cada eje en radianes</param>
        public void rotate(Vector3 rotation)
        {
            Matrix rotM = Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
            Matrix currentRotM = computeRotationMatrix();
            Matrix newRotM = currentRotM * rotM;

            orientation[0] = new Vector3(newRotM.M11, newRotM.M12, newRotM.M13);
            orientation[1] = new Vector3(newRotM.M21, newRotM.M22, newRotM.M23);
            orientation[2] = new Vector3(newRotM.M31, newRotM.M32, newRotM.M33);

            dirtyValues = true;
        }

        /// <summary>
        /// Cargar la rotacion absoluta del OBB.
        /// Pierda la rotacion anterior.
        /// </summary>
        /// <param name="rotation">Ángulo de rotación de cada eje en radianes</param>
        public void setRotation(Vector3 rotation)
        {
            Matrix rotM = Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
            orientation[0] = new Vector3(rotM.M11, rotM.M12, rotM.M13);
            orientation[1] = new Vector3(rotM.M21, rotM.M22, rotM.M23);
            orientation[2] = new Vector3(rotM.M31, rotM.M32, rotM.M33);

            dirtyValues = true;
        }

        /// <summary>
        /// Calcula la matriz de rotacion 4x4 del Obb en base a su orientacion
        /// </summary>
        /// <returns>Matriz de rotacion de 4x4</returns>
        public Matrix computeRotationMatrix()
        {
            Matrix rot = Matrix.Identity;

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
        /// Calcular OBB a partir de un conjunto de puntos.
        /// Busca por fuerza bruta el mejor OBB en la mejor orientación que se ajusta a esos puntos.
        /// Es un calculo costoso.
        /// </summary>
        /// <param name="points">puntos</param>
        /// <returns>OBB calculado</returns>
        public static TgcObb computeFromPoints(Vector3[] points)
        {
            return TgcObb.computeFromPointsRecursive(points, new Vector3(0, 0, 0), new Vector3(360, 360, 360), 10f).toClass();
        }


        /// <summary>
        /// Calcular OBB a partir de un conjunto de puntos.
        /// Prueba todas las orientaciones entre initValues y endValues, saltando de angulo en cada intervalo segun step
        /// Continua recursivamente hasta llegar a un step menor a 0.01f
        /// </summary>
        /// <returns></returns>
        private static OBBStruct computeFromPointsRecursive(Vector3[] points, Vector3 initValues, Vector3 endValues, float step)
        {
            OBBStruct minObb = new OBBStruct();
            float minVolume = float.MaxValue;
            Vector3 minInitValues = Vector3.Empty;
            Vector3 minEndValues = Vector3.Empty;
            Vector3[] transformedPoints = new Vector3[points.Length];
            float x, y, z;
            

            x = initValues.X;
            while(x <= endValues.X)
            {
                y = initValues.Y;
                float rotX = FastMath.ToRad(x);
                while (y <= endValues.Y)
                {
                    z = initValues.Z;
                    float rotY = FastMath.ToRad(y);
                    while (z <= endValues.Z)
                    {
                        //Matriz de rotacion
                        float rotZ = FastMath.ToRad(z);
                        Matrix rotM = Matrix.RotationYawPitchRoll(rotY, rotX, rotZ);
                        Vector3[] orientation = new Vector3[]{
                                new Vector3(rotM.M11, rotM.M12, rotM.M13),
                                new Vector3(rotM.M21, rotM.M22, rotM.M23),
                                new Vector3(rotM.M31, rotM.M32, rotM.M33)
                            };

                        //Transformar todos los puntos a OBB-space
                        for (int i = 0; i < transformedPoints.Length; i++)
                        {
                            transformedPoints[i].X = Vector3.Dot(points[i], orientation[0]);
                            transformedPoints[i].Y = Vector3.Dot(points[i], orientation[1]);
                            transformedPoints[i].Z = Vector3.Dot(points[i], orientation[2]);
                        }

                        //Obtener el AABB de todos los puntos transformados
                        TgcBoundingBox aabb = TgcBoundingBox.computeFromPoints(transformedPoints);

                        //Calcular volumen del AABB
                        Vector3 extents = aabb.calculateAxisRadius();
                        extents = TgcVectorUtils.abs(extents);
                        float volume = extents.X * 2 * extents.Y * 2 * extents.Z * 2;

                        //Buscar menor volumen
                        if (volume < minVolume)
                        {
                            minVolume = volume;
                            minInitValues = new Vector3(x, y, z);
                            minEndValues = new Vector3(x + step, y + step, z + step);

                            //Volver centro del AABB a World-space
                            Vector3 center = aabb.calculateBoxCenter();
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
        /// Generar OBB a partir de AABB
        /// </summary>
        /// <param name="aabb">BoundingBox</param>
        /// <returns>OBB generado</returns>
        public static TgcObb computeFromAABB(TgcBoundingBox aabb)
        {
            return TgcObb.computeFromAABB(aabb.toStruct()).toClass();
        }

        /// <summary>
        /// Generar OBB a partir de AABB
        /// </summary>
        /// <param name="aabb">BoundingBox</param>
        /// <returns>OBB generado</returns>
        public static TgcObb.OBBStruct computeFromAABB(TgcBoundingBox.AABBStruct aabb)
        {
            OBBStruct obb = new OBBStruct();
            obb.extents = (aabb.max - aabb.min) * 0.5f;
            obb.center = aabb.min + obb.extents;
            
            obb.orientation = new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1) };
            return obb;
        }


        /// <summary>
        /// Convertir un punto de World-Space espacio de coordenadas del OBB (OBB-Space)
        /// </summary>
        /// <param name="p">Punto en World-space</param>
        /// <returns>Punto convertido a OBB-space</returns>
        public Vector3 toObbSpace(Vector3 p)
        {
            Vector3 t = p - center;
            return new Vector3(Vector3.Dot(t, orientation[0]), Vector3.Dot(t, orientation[1]), Vector3.Dot(t, orientation[2]));
        }

        /// <summary>
        /// Convertir un punto de OBB-space a World-space
        /// </summary>
        /// <param name="p">Punto en OBB-space</param>
        /// <returns>Punto convertido a World-space</returns>
        public Vector3 toWorldSpace(Vector3 p)
        {
            return center + p.X * orientation[0] + p.Y * orientation[1] + p.Z * orientation[2];
        }



        /// <summary>
        /// Convertir a struct
        /// </summary>
        public OBBStruct toStruct()
        {
            OBBStruct obbStruct = new OBBStruct();
            obbStruct.center = center;
            obbStruct.orientation = orientation;
            obbStruct.extents = extents;
            return obbStruct;
        }

        /// <summary>
        /// OBB en un struct liviano
        /// </summary>
        public struct OBBStruct
        {
            public Vector3 center;
            public Vector3[] orientation;
            public Vector3 extents;

            /// <summary>
            /// Convertir a clase
            /// </summary>
            public TgcObb toClass()
            {
                TgcObb obb = new TgcObb();
                obb.center = center;
                obb.orientation = orientation;
                obb.extents = extents;
                return obb;
            }

            /// <summary>
            /// Convertir un punto de World-Space espacio de coordenadas del OBB (OBB-Space)
            /// </summary>
            /// <param name="p">Punto en World-space</param>
            /// <returns>Punto convertido a OBB-space</returns>
            public Vector3 toObbSpace(Vector3 p)
            {
                Vector3 t = p - center;
                return new Vector3(Vector3.Dot(t, orientation[0]), Vector3.Dot(t, orientation[1]), Vector3.Dot(t, orientation[2]));
            }

            /// <summary>
            /// Convertir un punto de OBB-space a World-space
            /// </summary>
            /// <param name="p">Punto en OBB-space</param>
            /// <returns>Punto convertido a World-space</returns>
            public Vector3 toWorldSpace(Vector3 p)
            {
                return center + p.X * orientation[0] + p.Y * orientation[1] + p.Z * orientation[2];
            }
        }


    }
}
