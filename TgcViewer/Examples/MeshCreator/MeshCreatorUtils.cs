using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Examples.MeshCreator.Primitives;
using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TGC.Core.Utils;

namespace Examples.MeshCreator
{
    /// <summary>
    ///     Utilidades generales
    /// </summary>
    public class MeshCreatorUtils
    {
        public static readonly Color SELECTED_OBJECT_COLOR = Color.Yellow;
        public static readonly Color UNSELECTED_OBJECT_COLOR = Color.Gray;

        /// <summary>
        ///     Distancia entre un punto y la camara
        /// </summary>
        public static float distanceFromCameraToPoint(MeshCreatorCamera camera, Vector3 p)
        {
            return Vector3.Length(camera.getPosition() - p);
        }

        /// <summary>
        ///     Distancia entre un objeto y la camara
        /// </summary>
        public static float distanceFromCameraToObject(MeshCreatorCamera camera, TgcBoundingBox aabb)
        {
            return distanceFromCameraToPoint(camera, aabb.calculateBoxCenter());
        }

        /// <summary>
        ///     Velocidad de incremento de altura en Y con el mouse, segun la distancia
        ///     del objeto a la camara
        /// </summary>
        public static float getMouseIncrementHeightSpeed(MeshCreatorCamera camera, TgcBoundingBox aabb, float heightY)
        {
            var dist = distanceFromCameraToObject(camera, aabb);
            return heightY*dist/500;
        }

        /// <summary>
        ///     Velocidad de incremento XY con el mouse, segun la distancia
        ///     del objeto a la camara
        /// </summary>
        public static Vector2 getMouseIncrementXYSpeed(MeshCreatorCamera camera, TgcBoundingBox aabb, Vector2 mouseMove)
        {
            var dist = distanceFromCameraToObject(camera, aabb);
            mouseMove.Multiply(dist/500);
            return mouseMove;
        }

        /// <summary>
        ///     Velocidad de incremento del mouse cuando traslada un objeto, segun la distancia del objeto a la camara
        /// </summary>
        public static float getMouseTranslateIncrementSpeed(MeshCreatorCamera camera, TgcBoundingBox aabb,
            float movement)
        {
            var dist = distanceFromCameraToObject(camera, aabb);
            return movement*dist/50;
        }

        /// <summary>
        ///     Velocidad de incremento del mouse cuando escala un objeto, segun la distancia del objeto a la camara
        /// </summary>
        public static float getMouseScaleIncrementSpeed(MeshCreatorCamera camera, TgcBoundingBox aabb, float scaling)
        {
            var dist = distanceFromCameraToObject(camera, aabb);
            return scaling*dist/1000;
        }

        /// <summary>
        ///     Incremento de tamaño de los ejes del Gizmo de traslacion segun la distancia de un punto a la camara
        /// </summary>
        public static float getTranslateGizmoSizeIncrement(MeshCreatorCamera camera, Vector3 p)
        {
            return distanceFromCameraToPoint(camera, p)/500;
        }

        /// <summary>
        ///     Velocidad de de zoom de la rueda del mouse segun la distancia de un punto a la camara
        /// </summary>
        public static float getMouseZoomSpeed(MeshCreatorCamera camera, Vector3 p)
        {
            var dist = distanceFromCameraToPoint(camera, p);
            if (dist < 100)
            {
                return 0.01f;
            }
            return 0.10f;
        }

        /*
        /// <summary>
        /// Proyecta un BoundingBox a un rectangulo 2D de screen space
        /// </summary>
        public static Rectangle projectAABB(TgcBoundingBox aabb)
        {
            return aabb.projectToScreen();
        }
        */

        /// <summary>
        ///     Proyectar AABB a 2D
        /// </summary>
        /// <param name="box3d">BoundingBox 3D</param>
        /// <param name="box2D">Rectangulo 2D proyectado</param>
        /// <returns>False si es un caso degenerado de proyeccion y no debe considerarse</returns>
        public static bool projectBoundingBox(TgcBoundingBox box3d, out Rectangle box2D)
        {
            //Datos de viewport
            var d3dDevice = GuiController.Instance.D3dDevice;
            var viewport = d3dDevice.Viewport;
            var view = d3dDevice.Transform.View;
            var proj = d3dDevice.Transform.Projection;
            var width = viewport.Width;
            var height = viewport.Height;

            box2D = new Rectangle();

            //Proyectar los 8 puntos, sin dividir aun por W
            var corners = box3d.computeCorners();
            var m = view*proj;
            var projVertices = new Vector3[corners.Length];
            for (var i = 0; i < corners.Length; i++)
            {
                var pOut = Vector3.Transform(corners[i], m);
                if (pOut.W < 0) return false;
                projVertices[i] = toScreenSpace(pOut, width, height);
            }

            //Buscar los puntos extremos
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            var minDepth = float.MaxValue;
            foreach (var v in projVertices)
            {
                if (v.X < min.X)
                {
                    min.X = v.X;
                }
                if (v.Y < min.Y)
                {
                    min.Y = v.Y;
                }
                if (v.X > max.X)
                {
                    max.X = v.X;
                }
                if (v.Y > max.Y)
                {
                    max.Y = v.Y;
                }

                if (v.Z < minDepth)
                {
                    minDepth = v.Z;
                }
            }

            //Clamp
            if (min.X < 0f) min.X = 0f;
            if (min.Y < 0f) min.Y = 0f;
            if (max.X >= width) max.X = width - 1;
            if (max.Y >= height) max.Y = height - 1;

            //Control de tamaño minimo
            if (max.X - min.X < 1f) return false;
            if (max.Y - min.Y < 1f) return false;

            //Cargar valores de box2D
            box2D.Location = new Point((int) min.X, (int) min.Y);
            box2D.Size = new Size((int) (max.X - min.X), (int) (max.Y - min.Y));
            return true;
        }

        /// <summary>
        ///     Pasa un punto a screen-space
        /// </summary>
        public static Vector3 toScreenSpace(Vector4 p, int width, int height)
        {
            //divido por w, (lo paso al proj. space)
            p.X = p.X/p.W;
            p.Y = p.Y/p.W;
            p.Z = p.Z/p.W;

            //lo paso a screen space
            p.X = (int) (0.5f + (p.X + 1)*0.5f*width);
            p.Y = (int) (0.5f + (1 - p.Y)*0.5f*height);

            return new Vector3(p.X, p.Y, p.Z);
        }

        /// <summary>
        ///     Proyecta a pantalla el punto minimo y maximo de un BoundingBox y genera un vector 2D normalizado
        /// </summary>
        public static Vector2 projectAABBScreenVec(TgcBoundingBox aabb)
        {
            var device = GuiController.Instance.D3dDevice;
            var viewport = device.Viewport;
            var world = device.Transform.World;
            var view = device.Transform.View;
            var proj = device.Transform.Projection;

            //Proyectar punto minimo y maximo del AABB
            var minProj = Vector3.Project(aabb.PMin, viewport, proj, view, world);
            var maxProj = Vector3.Project(aabb.PMax, viewport, proj, view, world);

            //Armar vector 2D
            var vec2D = new Vector2(maxProj.X - minProj.X, maxProj.Y - minProj.Y);
            vec2D.Normalize();
            return vec2D;
        }

        /// <summary>
        ///     Proyectar punto 3D a 2D
        /// </summary>
        /// <param name="box3d">Punto 3D</param>
        /// <param name="box2D">Rectangulo 2D proyectado</param>
        /// <returns>False si es un caso degenerado de proyeccion y no debe considerarse</returns>
        public static bool projectPoint(Vector3 p, out Rectangle box2D)
        {
            //Datos de viewport
            var d3dDevice = GuiController.Instance.D3dDevice;
            var viewport = d3dDevice.Viewport;
            var view = d3dDevice.Transform.View;
            var proj = d3dDevice.Transform.Projection;
            var width = viewport.Width;
            var height = viewport.Height;
            var m = view*proj;

            //Proyectar
            box2D = new Rectangle();
            var pOut = Vector3.Transform(p, m);
            if (pOut.W < 0) return false;
            var projVertex = toScreenSpace(pOut, width, height);
            var min = new Vector2(projVertex.X, projVertex.Y);
            var max = min + new Vector2(1, 1);

            //Clamp
            if (min.X < 0f) min.X = 0f;
            if (min.Y < 0f) min.Y = 0f;
            if (max.X >= width) max.X = width - 1;
            if (max.Y >= height) max.Y = height - 1;

            //Control de tamaño minimo
            if (max.X - min.X < 1f) return false;
            if (max.Y - min.Y < 1f) return false;

            //Cargar valores de box2D
            box2D.Location = new Point((int) min.X, (int) min.Y);
            box2D.Size = new Size((int) (max.X - min.X), (int) (max.Y - min.Y));
            return true;
        }

        /// <summary>
        ///     Proyectar un segmento 3D (a, b) a un rectangulo 2D en pantalla
        /// </summary>
        /// <param name="a">Inicio del segmento</param>
        /// <param name="b">Fin del segmento</param>
        /// <param name="box2D">Rectangulo 2D proyectado</param>
        /// <returns>False si es un caso degenerado de proyeccion y no debe considerarse</returns>
        public static bool projectSegmentToScreenRect(Vector3 a, Vector3 b, out Rectangle box2D)
        {
            //Datos de viewport
            var d3dDevice = GuiController.Instance.D3dDevice;
            var viewport = d3dDevice.Viewport;
            var view = d3dDevice.Transform.View;
            var proj = d3dDevice.Transform.Projection;
            var width = viewport.Width;
            var height = viewport.Height;
            var m = view*proj;

            //Proyectar
            box2D = new Rectangle();
            var aOut = Vector3.Transform(a, m);
            if (aOut.W < 0) return false;
            var bOut = Vector3.Transform(b, m);
            if (bOut.W < 0) return false;
            var aProjVertex = toScreenSpace(aOut, width, height);
            var bProjVertex = toScreenSpace(bOut, width, height);
            var min = new Vector2(FastMath.Min(aProjVertex.X, bProjVertex.X), FastMath.Min(aProjVertex.Y, bProjVertex.Y));
            var max = new Vector2(FastMath.Max(aProjVertex.X, bProjVertex.X), FastMath.Max(aProjVertex.Y, bProjVertex.Y));

            //Clamp
            if (min.X < 0f) min.X = 0f;
            if (min.Y < 0f) min.Y = 0f;
            if (max.X >= width) max.X = width - 1;
            if (max.Y >= height) max.Y = height - 1;

            //Control de tamaño minimo
            if (max.X - min.X < 1f) return false;
            if (max.Y - min.Y < 1f) return false;

            //Cargar valores de box2D
            box2D.Location = new Point((int) min.X, (int) min.Y);
            box2D.Size = new Size((int) (max.X - min.X), (int) (max.Y - min.Y));
            return true;
        }

        /// <summary>
        ///     Proyectar un poligono 3D a 2D en la pantalla
        /// </summary>
        /// <param name="vertices">Vertices del poligono</param>
        /// <param name="box2D">Rectangulo 2D proyectado</param>
        /// <returns>False si es un caso degenerado de proyeccion y no debe considerarse</returns>
        public static bool projectPolygon(Vector3[] vertices, out Rectangle box2D)
        {
            //Datos de viewport
            var d3dDevice = GuiController.Instance.D3dDevice;
            var viewport = d3dDevice.Viewport;
            var view = d3dDevice.Transform.View;
            var proj = d3dDevice.Transform.Projection;
            var width = viewport.Width;
            var height = viewport.Height;

            box2D = new Rectangle();

            //Proyectar todos los puntos, sin dividir aun por W
            var m = view*proj;
            var projVertices = new Vector3[vertices.Length];
            for (var i = 0; i < vertices.Length; i++)
            {
                var pOut = Vector3.Transform(vertices[i], m);
                if (pOut.W < 0) return false;
                projVertices[i] = toScreenSpace(pOut, width, height);
            }

            //Buscar los puntos extremos
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            var minDepth = float.MaxValue;
            foreach (var v in projVertices)
            {
                if (v.X < min.X)
                {
                    min.X = v.X;
                }
                if (v.Y < min.Y)
                {
                    min.Y = v.Y;
                }
                if (v.X > max.X)
                {
                    max.X = v.X;
                }
                if (v.Y > max.Y)
                {
                    max.Y = v.Y;
                }

                if (v.Z < minDepth)
                {
                    minDepth = v.Z;
                }
            }

            //Clamp
            if (min.X < 0f) min.X = 0f;
            if (min.Y < 0f) min.Y = 0f;
            if (max.X >= width) max.X = width - 1;
            if (max.Y >= height) max.Y = height - 1;

            //Control de tamaño minimo
            if (max.X - min.X < 1f) return false;
            if (max.Y - min.Y < 1f) return false;

            //Cargar valores de box2D
            box2D.Location = new Point((int) min.X, (int) min.Y);
            box2D.Size = new Size((int) (max.X - min.X), (int) (max.Y - min.Y));
            return true;
        }

        /// <summary>
        ///     Calcular BoundingBox de todos los objetos seleccionados.
        ///     Devuelve null si no hay ningun objeto seleccionado
        /// </summary>
        public static TgcBoundingBox getSelectionBoundingBox(List<EditorPrimitive> selectionList)
        {
            //Hay un solo objeto seleccionado
            if (selectionList.Count == 1)
            {
                //Devolver su AABB
                return selectionList[0].BoundingBox;
            }

                //Hay varios objetos seleccionados
            if (selectionList.Count > 1)
            {
                //Crear AABB que une a todos los objetos
                var auxBoundingBoxList = new List<TgcBoundingBox>();
                foreach (var p in selectionList)
                {
                    auxBoundingBoxList.Add(p.BoundingBox);
                }
                return TgcBoundingBox.computeFromBoundingBoxes(auxBoundingBoxList);
            }

            return null;
        }

        /// <summary>
        ///     Obtener la imagen pedida o devolver null
        /// </summary>
        public static Image getImage(string path)
        {
            try
            {
                return Image.FromFile(path);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        ///     Convierte de string a Dictionary para user properties.
        /// </summary>
        public static Dictionary<string, string> getUserPropertiesDictionary(string userPropString)
        {
            var dict = new Dictionary<string, string>();
            userPropString = userPropString.Trim();
            var lines = userPropString.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                var s = lines[i].Split('=');
                if (s.Length == 2)
                {
                    if (!dict.ContainsKey(s[0]))
                    {
                        var value = s[1].Replace("\r", "");
                        dict.Add(s[0], value);
                    }
                }
            }
            return dict;
        }

        /// <summary>
        ///     Convierte de Dictionary a string para user properties.
        /// </summary>
        public static string getUserPropertiesString(Dictionary<string, string> dict)
        {
            if (dict != null)
            {
                var sb = new StringBuilder();
                foreach (var p in dict)
                {
                    sb.AppendLine(p.Key + "=" + p.Value);
                }
                return sb.ToString();
            }

            return "";
        }
    }
}