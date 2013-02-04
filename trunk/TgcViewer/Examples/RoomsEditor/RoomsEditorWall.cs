using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using System.Drawing;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer;

namespace Examples.RoomsEditor
{
    /// <summary>
    /// Información lógica y visual de una pared de un Room
    /// </summary>
    public class RoomsEditorWall
    {
        List<TgcPlaneWall> wallSegments;
        /// <summary>
        /// Segmentos 3d de pared
        /// </summary>
        public List<TgcPlaneWall> WallSegments
        {
            get { return wallSegments; }
        }

        TgcTexture texture;
        /// <summary>
        /// Textura general de todos los segmentos de la pared
        /// </summary>
        public TgcTexture Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        float uTile;
        /// <summary>
        /// Cantidad de tile de la textura en coordenada U
        /// </summary>
        public float UTile
        {
            get { return uTile; }
            set { uTile = value; }
        }

        float vTile;
        /// <summary>
        /// Cantidad de tile de la textura en coordenada V
        /// </summary>
        public float VTile
        {
            get { return vTile; }
            set { vTile = value; }
        }

        bool autoAdjustUv;
        /// <summary>
        /// Auto ajustar coordenadas UV en base a la relación de tamaño de la pared y la textura
        /// </summary>
        public bool AutoAdjustUv
        {
            get { return autoAdjustUv; }
            set { autoAdjustUv = value; }
        } 

        List<RoomsEditorRoom> intersectingRooms;
        /// <summary>
        /// Rooms contra los cuales intersecta esta pared
        /// </summary>
        public List<RoomsEditorRoom> IntersectingRooms
        {
            get { return intersectingRooms; }
        }

        string name;
        /// <summary>
        /// Nombre que indentifica que pared es: North, East, Floor, etc.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        

        RoomsEditorRoom room;




        public RoomsEditorWall(RoomsEditorRoom room, string name)
        {
            this.room = room;
            this.name = name;
            wallSegments = new List<TgcPlaneWall>();
            intersectingRooms = new List<RoomsEditorRoom>();

            //cargar valores default de la pared
            texture = TgcTexture.createTexture(GuiController.Instance.D3dDevice, room.RoomPanel.mapView.defaultTextureImage);
            autoAdjustUv = true;
            uTile = 1f;
            vTile = 1f;
        }

        public void dispose()
        {
            foreach (TgcPlaneWall wall3d in wallSegments)
            {
                wall3d.dispose();
            }
            texture.dispose();
        }

        public void render()
        {
            foreach (TgcPlaneWall wall3d in wallSegments)
            {
                wall3d.render();
            }
        }


    }
}
