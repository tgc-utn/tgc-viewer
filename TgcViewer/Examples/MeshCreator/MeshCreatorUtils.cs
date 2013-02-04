using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Examples.MeshCreator.Primitives;

namespace Examples.MeshCreator
{
    /// <summary>
    /// Utilidades generales
    /// </summary>
    public class MeshCreatorUtils
    {

        public static readonly Color SELECTED_OBJECT_COLOR = Color.Yellow;
        public static readonly Color UNSELECTED_OBJECT_COLOR = Color.Gray;

        /// <summary>
        /// Distancia entre un punto y la camara
        /// </summary>
        public static float distanceFromCameraToPoint(MeshCreatorCamera camera, Vector3 p)
        {
            return Vector3.Length(camera.getPosition() - p);
        }

        /// <summary>
        /// Distancia entre un objeto y la camara
        /// </summary>
        public static float distanceFromCameraToObject(MeshCreatorCamera camera, TgcBoundingBox aabb)
        {
            return distanceFromCameraToPoint(camera, aabb.calculateBoxCenter());
        }

        /// <summary>
        /// Velocidad de incremento de altura en Y con el mouse, segun la distancia
        /// del objeto a la camara
        /// </summary>
        public static float getMouseIncrementHeightSpeed(MeshCreatorCamera camera, TgcBoundingBox aabb, float heightY)
        {
            float dist = distanceFromCameraToObject(camera, aabb);
            return heightY * dist / 500;
        }

        /// <summary>
        /// Velocidad de incremento XY con el mouse, segun la distancia
        /// del objeto a la camara
        /// </summary>
        public static Vector2 getMouseIncrementXYSpeed(MeshCreatorCamera camera, TgcBoundingBox aabb, Vector2 mouseMove)
        {
            float dist = distanceFromCameraToObject(camera, aabb);
            mouseMove.Multiply(dist / 500);
            return mouseMove;
        }

        /// <summary>
        /// Velocidad de incremento del mouse cuando traslada un objeto, segun la distancia del objeto a la camara
        /// </summary>
        public static float getMouseTranslateIncrementSpeed(MeshCreatorCamera camera, TgcBoundingBox aabb, float movement)
        {
            float dist = distanceFromCameraToObject(camera, aabb);
            return movement * dist / 50;
        }

        /// <summary>
        /// Velocidad de incremento del mouse cuando escala un objeto, segun la distancia del objeto a la camara
        /// </summary>
        public static float getMouseScaleIncrementSpeed(MeshCreatorCamera camera, TgcBoundingBox aabb, float scaling)
        {
            float dist = distanceFromCameraToObject(camera, aabb);
            return scaling * dist / 1000;
        }

        /// <summary>
        /// Incremento de tamaño de los ejes del Gizmo de traslacion segun la distancia de un punto a la camara
        /// </summary>
        public static float getTranslateGizmoSizeIncrement(MeshCreatorCamera camera, Vector3 p)
        {
            return distanceFromCameraToPoint(camera, p) / 500;
        }

        /// <summary>
        /// Velocidad de de zoom de la rueda del mouse segun la distancia de un punto a la camara
        /// </summary>
        public static float getMouseZoomSpeed(MeshCreatorCamera camera, Vector3 p)
        {
            float dist = distanceFromCameraToPoint(camera, p);
            if (dist < 100)
            {
                return 0.01f;
            }
            return 0.10f;
        }

        /// <summary>
        /// Proyecta un BoundingBox a un rectangulo 2D de screen space
        /// </summary>
        public static Rectangle projectAABB(TgcBoundingBox aabb)
        {
            return aabb.projectToScreen();
        }

        /// <summary>
        /// Proyecta a pantalla el punto minimo y maximo de un BoundingBox y genera un vector 2D normalizado
        /// </summary>
        public static Vector2 projectAABBScreenVec(TgcBoundingBox aabb)
        {
            Device device = GuiController.Instance.D3dDevice;
            Viewport viewport = device.Viewport;
            Matrix world = device.Transform.World;
            Matrix view = device.Transform.View;
            Matrix proj = device.Transform.Projection;

            //Proyectar punto minimo y maximo del AABB
            Vector3 minProj = Vector3.Project(aabb.PMin, viewport, proj, view, world);
            Vector3 maxProj = Vector3.Project(aabb.PMax, viewport, proj, view, world);

            //Armar vector 2D
            Vector2 vec2D = new Vector2(maxProj.X - minProj.X, maxProj.Y - minProj.Y);
            vec2D.Normalize();
            return vec2D;
        }


        /// <summary>
        /// Calcular BoundingBox de todos los objetos seleccionados.
        /// Devuelve null si no hay ningun objeto seleccionado
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
            else if (selectionList.Count > 1)
            {
                //Crear AABB que une a todos los objetos
                List<TgcBoundingBox> auxBoundingBoxList = new List<TgcBoundingBox>();
                foreach (EditorPrimitive p in selectionList)
                {
                    auxBoundingBoxList.Add(p.BoundingBox);
                }
                return TgcBoundingBox.computeFromBoundingBoxes(auxBoundingBoxList);
            }

            return null;
        }

        /// <summary>
        /// Obtener la imagen pedida o devolver null
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
        /// Convierte de string a Dictionary para user properties.
        /// </summary>
        public static Dictionary<string, string> getUserPropertiesDictionary(string userPropString)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            userPropString = userPropString.Trim();
            string[] lines = userPropString.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string[] s = lines[i].Split('=');
                if (s.Length == 2)
                {
                    if (!dict.ContainsKey(s[0]))
                    {
                        string value = s[1].Replace("\r", "");
                        dict.Add(s[0], value);
                    }
                }
            }
            return dict;
        }

        /// <summary>
        /// Convierte de Dictionary a string para user properties.
        /// </summary>
        public static string getUserPropertiesString(Dictionary<string, string> dict)
        {
            if (dict != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<string, string> p in dict)
                {
                    sb.AppendLine(p.Key + "=" + p.Value);
                }
                return sb.ToString();
            }

            return "";
        }

    }
}
