using System.Collections.Generic;
using TGC.Core.Direct3D;
using TGC.Viewer.Utils.TgcGeometry;
using TGC.Viewer.Utils.TgcSceneLoader;

namespace TGC.Examples.RoomsEditor
{
    /// <summary>
    ///     Información lógica y visual de una pared de un Room
    /// </summary>
    public class RoomsEditorWall
    {
        private RoomsEditorRoom room;

        public RoomsEditorWall(RoomsEditorRoom room, string name)
        {
            this.room = room;
            Name = name;
            WallSegments = new List<TgcPlaneWall>();
            IntersectingRooms = new List<RoomsEditorRoom>();

            //cargar valores default de la pared
            Texture = TgcTexture.createTexture(D3DDevice.Instance.Device, room.RoomPanel.mapView.defaultTextureImage);
            AutoAdjustUv = true;
            UTile = 1f;
            VTile = 1f;
        }

        /// <summary>
        ///     Segmentos 3d de pared
        /// </summary>
        public List<TgcPlaneWall> WallSegments { get; }

        /// <summary>
        ///     Textura general de todos los segmentos de la pared
        /// </summary>
        public TgcTexture Texture { get; set; }

        /// <summary>
        ///     Cantidad de tile de la textura en coordenada U
        /// </summary>
        public float UTile { get; set; }

        /// <summary>
        ///     Cantidad de tile de la textura en coordenada V
        /// </summary>
        public float VTile { get; set; }

        /// <summary>
        ///     Auto ajustar coordenadas UV en base a la relación de tamaño de la pared y la textura
        /// </summary>
        public bool AutoAdjustUv { get; set; }

        /// <summary>
        ///     Rooms contra los cuales intersecta esta pared
        /// </summary>
        public List<RoomsEditorRoom> IntersectingRooms { get; }

        /// <summary>
        ///     Nombre que indentifica que pared es: North, East, Floor, etc.
        /// </summary>
        public string Name { get; }

        public void dispose()
        {
            foreach (var wall3d in WallSegments)
            {
                wall3d.dispose();
            }
            Texture.dispose();
        }

        public void render()
        {
            foreach (var wall3d in WallSegments)
            {
                wall3d.render();
            }
        }
    }
}