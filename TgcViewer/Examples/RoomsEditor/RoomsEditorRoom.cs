using Microsoft.DirectX;
using System.Collections.Generic;
using System.Drawing;
using TGC.Viewer.Utils.TgcGeometry;

namespace TGC.Examples.RoomsEditor
{
    /// <summary>
    ///     Información lógica y visual de un Room
    /// </summary>
    public class RoomsEditorRoom
    {
        public RoomsEditorRoom(string name, RoomsEditorMapView.RoomPanel roomPanel)
        {
            Name = name;
            RoomPanel = roomPanel;

            RoomPanel.Room = this;

            //crear paredes
            Walls = new RoomsEditorWall[6];
            Walls[0] = new RoomsEditorWall(this, "Roof");
            Walls[1] = new RoomsEditorWall(this, "Floor");
            Walls[2] = new RoomsEditorWall(this, "East");
            Walls[3] = new RoomsEditorWall(this, "West");
            Walls[4] = new RoomsEditorWall(this, "North");
            Walls[5] = new RoomsEditorWall(this, "South");

            //Valores default
            Height = 100;
            FloorLevel = 0;
        }

        /// <summary>
        ///     Panel visual 2D del room
        /// </summary>
        public RoomsEditorMapView.RoomPanel RoomPanel { get; }

        /// <summary>
        ///     Nombre del room
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Altura de todas las paredes del room
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        ///     Altura del piso
        /// </summary>
        public int FloorLevel { get; set; }

        /// <summary>
        ///     Paredes de este cuarto
        /// </summary>
        public RoomsEditorWall[] Walls { get; }

        public void dispose()
        {
            foreach (var wall in Walls)
            {
                wall.dispose();
            }
        }

        /// <summary>
        ///     Actualiza la información de las paredes
        /// </summary>
        public void buildWalls(List<RoomsEditorRoom> rooms, Vector3 sceneScale)
        {
            var bounds = RoomPanel.Label.Bounds;

            //roof
            var roof = Walls[0];
            var roofWall3d = new TgcPlaneWall(
                new Vector3(bounds.X * sceneScale.X, (FloorLevel + Height) * sceneScale.Y, bounds.Y * sceneScale.Z),
                new Vector3(bounds.Width * sceneScale.X, 0, bounds.Height * sceneScale.Z),
                TgcPlaneWall.Orientations.XZplane,
                roof.Texture, roof.UTile, roof.VTile
                );
            roof.WallSegments.Clear();
            roof.WallSegments.Add(roofWall3d);

            //floor
            var floor = Walls[1];
            var floorWall3d = new TgcPlaneWall(
                new Vector3(bounds.X * sceneScale.X, FloorLevel, bounds.Y * sceneScale.Z),
                new Vector3(bounds.Width * sceneScale.X, 0, bounds.Height * sceneScale.Z),
                TgcPlaneWall.Orientations.XZplane,
                floor.Texture, floor.UTile, floor.VTile
                );
            floor.WallSegments.Clear();
            floor.WallSegments.Add(floorWall3d);

            List<Point> intersectingLines;
            List<RoomsEditorRoom> intersectingRooms;
            Point wallSegment;
            List<Point> finalWallSegments;

            //east wall
            var eastWall = Walls[2];

            //buscar colisiones y particionar
            wallSegment = new Point(bounds.Y, bounds.Y + bounds.Height);
            findIntersectingEastWall(rooms, out intersectingLines, out intersectingRooms);
            finalWallSegments = fragmentWall(eastWall, wallSegment, intersectingLines, intersectingRooms);

            //Crear una una pared 3D por cada segmento final que quedó
            eastWall.WallSegments.Clear();
            foreach (var seg in finalWallSegments)
            {
                var wall3d = new TgcPlaneWall(
                    new Vector3((bounds.X + bounds.Width) * sceneScale.X, FloorLevel * sceneScale.Y, seg.X * sceneScale.Z),
                    new Vector3(0, Height * sceneScale.Y, (seg.Y - seg.X) * sceneScale.Z),
                    TgcPlaneWall.Orientations.YZplane,
                    eastWall.Texture, eastWall.UTile, eastWall.VTile
                    );
                eastWall.WallSegments.Add(wall3d);
            }

            //Crear paredes para las diferencias de alturas con los otros Rooms
            createEastWallSegmentsForHeightDiff(sceneScale, intersectingLines, intersectingRooms, eastWall);

            //west wall
            var westWall = Walls[3];

            //buscar colisiones y particionar
            wallSegment = new Point(bounds.Y, bounds.Y + bounds.Height);
            findIntersectingWestWall(rooms, out intersectingLines, out intersectingRooms);
            finalWallSegments = fragmentWall(eastWall, wallSegment, intersectingLines, intersectingRooms);

            //Crear una una pared 3D por cada segmento final que quedó
            westWall.WallSegments.Clear();
            foreach (var seg in finalWallSegments)
            {
                var wall3d = new TgcPlaneWall(
                    new Vector3(bounds.X * sceneScale.X, FloorLevel * sceneScale.Y, seg.X * sceneScale.Z),
                    new Vector3(0, Height * sceneScale.Y, (seg.Y - seg.X) * sceneScale.Z),
                    TgcPlaneWall.Orientations.YZplane,
                    westWall.Texture, westWall.UTile, westWall.VTile
                    );
                westWall.WallSegments.Add(wall3d);
            }
            //Crear paredes para las diferencias de alturas con los otros Rooms
            createWestWallSegmentsForHeightDiff(sceneScale, intersectingLines, intersectingRooms, westWall);

            //north wall
            var northWall = Walls[4];

            //buscar colisiones y particionar
            wallSegment = new Point(bounds.X, bounds.X + bounds.Width);
            findIntersectingNorthWall(rooms, out intersectingLines, out intersectingRooms);
            finalWallSegments = fragmentWall(eastWall, wallSegment, intersectingLines, intersectingRooms);

            //Crear una una pared 3D por cada segmento final que quedó
            northWall.WallSegments.Clear();
            foreach (var seg in finalWallSegments)
            {
                var wall3d = new TgcPlaneWall(
                    new Vector3(seg.X * sceneScale.X, FloorLevel * sceneScale.Y, bounds.Y * sceneScale.Z),
                    new Vector3((seg.Y - seg.X) * sceneScale.X, Height * sceneScale.Y, 0),
                    TgcPlaneWall.Orientations.XYplane,
                    northWall.Texture, northWall.UTile, northWall.VTile
                    );
                northWall.WallSegments.Add(wall3d);
            }
            //Crear paredes para las diferencias de alturas con los otros Rooms
            createNorthWallSegmentsForHeightDiff(sceneScale, intersectingLines, intersectingRooms, northWall);

            //south wall
            var southWall = Walls[5];

            //buscar colisiones y particionar
            wallSegment = new Point(bounds.X, bounds.X + bounds.Width);
            findIntersectingSouthWall(rooms, out intersectingLines, out intersectingRooms);
            finalWallSegments = fragmentWall(eastWall, wallSegment, intersectingLines, intersectingRooms);

            //Crear una una pared 3D por cada segmento final que quedó
            southWall.WallSegments.Clear();
            foreach (var seg in finalWallSegments)
            {
                var wall3d = new TgcPlaneWall(
                    new Vector3(seg.X * sceneScale.X, FloorLevel * sceneScale.Y, (bounds.Y + bounds.Height) * sceneScale.Z),
                    new Vector3((seg.Y - seg.X) * sceneScale.X, Height * sceneScale.Y, 0),
                    TgcPlaneWall.Orientations.XYplane,
                    southWall.Texture, southWall.UTile, southWall.VTile
                    );
                southWall.WallSegments.Add(wall3d);
            }
            //Crear paredes para las diferencias de alturas con los otros Rooms
            createSouthWallSegmentsForHeightDiff(sceneScale, intersectingLines, intersectingRooms, southWall);
        }

        /// <summary>
        ///     Crear paredes 3D para las diferencias de altura entre la pared East de este Room y el resto
        ///     de los Rooms contra los que colisiona
        /// </summary>
        private void createEastWallSegmentsForHeightDiff(Vector3 sceneScale, List<Point> intersectingLines,
            List<RoomsEditorRoom> intersectingRooms, RoomsEditorWall wall)
        {
            var bounds = RoomPanel.Label.Bounds;
            var wallSide = new Point(RoomPanel.Label.Location.Y, RoomPanel.Label.Location.Y + RoomPanel.Label.Height);
            Point[] supDiffSegments;
            Point[] infDiffSegments;
            findSegmentsForHeightDiff(intersectingRooms, out supDiffSegments, out infDiffSegments);
            for (var i = 0; i < intersectingRooms.Count; i++)
            {
                var fullIntersecLine = intersectingLines[i];
                var intersecLine = intersectLineSegments(fullIntersecLine, wallSide);

                //Fragmento superior
                var supSeg = supDiffSegments[i];
                if (supSeg.X != 0 || supSeg.Y != 0)
                {
                    var wall3d = new TgcPlaneWall(
                        new Vector3((bounds.X + bounds.Width) * sceneScale.X, supSeg.X * sceneScale.Y,
                            intersecLine.X * sceneScale.Z),
                        new Vector3(0, (supSeg.Y - supSeg.X) * sceneScale.Y,
                            (intersecLine.Y - intersecLine.X) * sceneScale.Z),
                        TgcPlaneWall.Orientations.YZplane,
                        wall.Texture, wall.UTile, wall.VTile
                        );
                    wall.WallSegments.Add(wall3d);
                }

                //Fragmento inferior
                var infSeg = infDiffSegments[i];
                if (infSeg.X != 0 || infSeg.Y != 0)
                {
                    var wall3d = new TgcPlaneWall(
                        new Vector3((bounds.X + bounds.Width) * sceneScale.X, infSeg.X * sceneScale.Y,
                            intersecLine.X * sceneScale.Z),
                        new Vector3(0, (infSeg.Y - infSeg.X) * sceneScale.Y,
                            (intersecLine.Y - intersecLine.X) * sceneScale.Z),
                        TgcPlaneWall.Orientations.YZplane,
                        wall.Texture, wall.UTile, wall.VTile
                        );
                    wall.WallSegments.Add(wall3d);
                }
            }
        }

        /// <summary>
        ///     Crear paredes 3D para las diferencias de altura entre la pared West de este Room y el resto
        ///     de los Rooms contra los que colisiona
        /// </summary>
        private void createWestWallSegmentsForHeightDiff(Vector3 sceneScale, List<Point> intersectingLines,
            List<RoomsEditorRoom> intersectingRooms, RoomsEditorWall wall)
        {
            var bounds = RoomPanel.Label.Bounds;
            var wallSide = new Point(RoomPanel.Label.Location.Y, RoomPanel.Label.Location.Y + RoomPanel.Label.Height);
            Point[] supDiffSegments;
            Point[] infDiffSegments;
            findSegmentsForHeightDiff(intersectingRooms, out supDiffSegments, out infDiffSegments);
            for (var i = 0; i < intersectingRooms.Count; i++)
            {
                var fullIntersecLine = intersectingLines[i];
                var intersecLine = intersectLineSegments(fullIntersecLine, wallSide);

                //Fragmento superior
                var supSeg = supDiffSegments[i];
                if (supSeg.X != 0 || supSeg.Y != 0)
                {
                    var wall3d = new TgcPlaneWall(
                        new Vector3(bounds.X * sceneScale.X, supSeg.X * sceneScale.Y, intersecLine.X * sceneScale.Z),
                        new Vector3(0, (supSeg.Y - supSeg.X) * sceneScale.Y,
                            (intersecLine.Y - intersecLine.X) * sceneScale.Z),
                        TgcPlaneWall.Orientations.YZplane,
                        wall.Texture, wall.UTile, wall.VTile
                        );
                    wall.WallSegments.Add(wall3d);
                }

                //Fragmento inferior
                var infSeg = infDiffSegments[i];
                if (infSeg.X != 0 || infSeg.Y != 0)
                {
                    var wall3d = new TgcPlaneWall(
                        new Vector3(bounds.X * sceneScale.X, infSeg.X * sceneScale.Y, intersecLine.X * sceneScale.Z),
                        new Vector3(0, (infSeg.Y - infSeg.X) * sceneScale.Y,
                            (intersecLine.Y - intersecLine.X) * sceneScale.Z),
                        TgcPlaneWall.Orientations.YZplane,
                        wall.Texture, wall.UTile, wall.VTile
                        );
                    wall.WallSegments.Add(wall3d);
                }
            }
        }

        /// <summary>
        ///     Crear paredes 3D para las diferencias de altura entre la pared North de este Room y el resto
        ///     de los Rooms contra los que colisiona
        /// </summary>
        private void createNorthWallSegmentsForHeightDiff(Vector3 sceneScale, List<Point> intersectingLines,
            List<RoomsEditorRoom> intersectingRooms, RoomsEditorWall wall)
        {
            var bounds = RoomPanel.Label.Bounds;
            var wallSide = new Point(RoomPanel.Label.Location.X, RoomPanel.Label.Location.X + RoomPanel.Label.Width);
            Point[] supDiffSegments;
            Point[] infDiffSegments;
            findSegmentsForHeightDiff(intersectingRooms, out supDiffSegments, out infDiffSegments);
            for (var i = 0; i < intersectingRooms.Count; i++)
            {
                var fullIntersecLine = intersectingLines[i];
                var intersecLine = intersectLineSegments(fullIntersecLine, wallSide);

                //Fragmento superior
                var supSeg = supDiffSegments[i];
                if (supSeg.X != 0 || supSeg.Y != 0)
                {
                    var wall3d = new TgcPlaneWall(
                        new Vector3(intersecLine.X * sceneScale.X, supSeg.X * sceneScale.Y, bounds.Y * sceneScale.Z),
                        new Vector3((intersecLine.Y - intersecLine.X) * sceneScale.X, (supSeg.Y - supSeg.X) * sceneScale.Y,
                            0),
                        TgcPlaneWall.Orientations.XYplane,
                        wall.Texture, wall.UTile, wall.VTile
                        );
                    wall.WallSegments.Add(wall3d);
                }

                //Fragmento inferior
                var infSeg = infDiffSegments[i];
                if (infSeg.X != 0 || infSeg.Y != 0)
                {
                    var wall3d = new TgcPlaneWall(
                        new Vector3(intersecLine.X * sceneScale.X, infSeg.X * sceneScale.Y, bounds.Y * sceneScale.Z),
                        new Vector3((intersecLine.Y - intersecLine.X) * sceneScale.X, (infSeg.Y - infSeg.X) * sceneScale.Y,
                            0),
                        TgcPlaneWall.Orientations.XYplane,
                        wall.Texture, wall.UTile, wall.VTile
                        );
                    wall.WallSegments.Add(wall3d);
                }
            }
        }

        /// <summary>
        ///     Crear paredes 3D para las diferencias de altura entre la pared South de este Room y el resto
        ///     de los Rooms contra los que colisiona
        /// </summary>
        private void createSouthWallSegmentsForHeightDiff(Vector3 sceneScale, List<Point> intersectingLines,
            List<RoomsEditorRoom> intersectingRooms, RoomsEditorWall wall)
        {
            var bounds = RoomPanel.Label.Bounds;
            var wallSide = new Point(RoomPanel.Label.Location.X, RoomPanel.Label.Location.X + RoomPanel.Label.Width);
            Point[] supDiffSegments;
            Point[] infDiffSegments;
            findSegmentsForHeightDiff(intersectingRooms, out supDiffSegments, out infDiffSegments);
            for (var i = 0; i < intersectingRooms.Count; i++)
            {
                var fullIntersecLine = intersectingLines[i];
                var intersecLine = intersectLineSegments(fullIntersecLine, wallSide);

                //Fragmento superior
                var supSeg = supDiffSegments[i];
                if (supSeg.X != 0 || supSeg.Y != 0)
                {
                    var wall3d = new TgcPlaneWall(
                        new Vector3(intersecLine.X * sceneScale.X, supSeg.X * sceneScale.Y,
                            (bounds.Y + bounds.Height) * sceneScale.Z),
                        new Vector3((intersecLine.Y - intersecLine.X) * sceneScale.X, (supSeg.Y - supSeg.X) * sceneScale.Y,
                            0),
                        TgcPlaneWall.Orientations.XYplane,
                        wall.Texture, wall.UTile, wall.VTile
                        );
                    wall.WallSegments.Add(wall3d);
                }

                //Fragmento inferior
                var infSeg = infDiffSegments[i];
                if (infSeg.X != 0 || infSeg.Y != 0)
                {
                    var wall3d = new TgcPlaneWall(
                        new Vector3(intersecLine.X * sceneScale.X, infSeg.X * sceneScale.Y,
                            (bounds.Y + bounds.Height) * sceneScale.Z),
                        new Vector3((intersecLine.Y - intersecLine.X) * sceneScale.X, (infSeg.Y - infSeg.X) * sceneScale.Y,
                            0),
                        TgcPlaneWall.Orientations.XYplane,
                        wall.Texture, wall.UTile, wall.VTile
                        );
                    wall.WallSegments.Add(wall3d);
                }
            }
        }

        /// <summary>
        ///     Busca los segmentos de pared a generar en base a diferencia de alturas de las paredes.
        ///     Por cada intersectingRoom carga 2 segmentos en supDiffSegments y infDiffSegments (segmento superior e inferior).
        ///     Si alguno no hace falta la posición está en null.
        /// </summary>
        private void findSegmentsForHeightDiff(List<RoomsEditorRoom> intersectingRooms, out Point[] supDiffSegments,
            out Point[] infDiffSegments)
        {
            supDiffSegments = new Point[intersectingRooms.Count];
            infDiffSegments = new Point[intersectingRooms.Count];
            var roomHeightLine = new Point(FloorLevel, FloorLevel + Height);

            for (var i = 0; i < intersectingRooms.Count; i++)
            {
                var interRoom = intersectingRooms[i];

                //si hay diferencias de alturas, truncar
                if (FloorLevel != interRoom.FloorLevel || Height != interRoom.Height)
                {
                    var interRoomHeightLine = new Point(interRoom.FloorLevel, interRoom.FloorLevel + interRoom.Height);
                    var segmentsToExtract = new List<Point>();
                    segmentsToExtract.Add(intersectLineSegments(interRoomHeightLine, roomHeightLine));
                    var finalSegments = removeSegmentsFromLine(roomHeightLine, segmentsToExtract);

                    if (finalSegments.Count == 0)
                    {
                        infDiffSegments[i] = segmentsToExtract[0];
                    }
                    else if (finalSegments.Count == 2)
                    {
                        infDiffSegments[i] = finalSegments[0];
                        supDiffSegments[i] = finalSegments[1];
                    }
                    else
                    {
                        if (finalSegments[0].X == FloorLevel)
                        {
                            infDiffSegments[i] = finalSegments[0];
                        }
                        else
                        {
                            supDiffSegments[i] = finalSegments[0];
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Fragmenta una pared en varias subparedes, en base a todas las colisiones que hubo con cuartos vecinos
        /// </summary>
        private List<Point> fragmentWall(RoomsEditorWall wall, Point wallSegment, List<Point> intersectingLines,
            List<RoomsEditorRoom> intersectingRooms)
        {
            List<Point> finalWallSegments;
            wall.IntersectingRooms.Clear();

            //Hubo intersecciones, fragmentar
            if (intersectingRooms.Count > 0)
            {
                //Agregar rooms vecinos
                wall.IntersectingRooms.AddRange(intersectingRooms);

                //Calcular intersecciones con vecinos en esta pared que hay que quitar de la pared original
                var segmentsToExtract = new List<Point>();
                foreach (var lineSegment in intersectingLines)
                {
                    segmentsToExtract.Add(intersectLineSegments(lineSegment, wallSegment));
                }
                //Quitarle al segmento original los fragmentos
                finalWallSegments = removeSegmentsFromLine(wallSegment, segmentsToExtract);
            }
            //no fragmentar
            else
            {
                finalWallSegments = new List<Point>();
                finalWallSegments.Add(wallSegment);
            }

            return finalWallSegments;
        }

        /// <summary>
        ///     Devuelve todos los segmentos de linea correspondiente a la pared West de cada room
        ///     que colisionan con la pared East de este room.
        ///     Tambien informa los rooms contra los cuales colisiono y la cantidad de colisiones encontradas
        /// </summary>
        private int findIntersectingEastWall(List<RoomsEditorRoom> rooms, out List<Point> intersectingLines,
            out List<RoomsEditorRoom> intersectingRooms)
        {
            var eastSide = new Point(RoomPanel.Label.Location.Y, RoomPanel.Label.Location.Y + RoomPanel.Label.Height);
            var eastX = RoomPanel.Label.Location.X + RoomPanel.Label.Width;
            var heightLine = new Point(FloorLevel, FloorLevel + Height);

            intersectingLines = new List<Point>();
            intersectingRooms = new List<RoomsEditorRoom>();
            var intesects = 0;
            foreach (var testRoom in rooms)
            {
                if (testRoom != this)
                {
                    var testWestSide = new Point(testRoom.RoomPanel.Label.Location.Y,
                        testRoom.RoomPanel.Label.Location.Y + testRoom.RoomPanel.Label.Height);
                    var testWestX = testRoom.RoomPanel.Label.Location.X;
                    if (eastX == testWestX)
                    {
                        if (testLineSegments(eastSide, testWestSide))
                        {
                            var testHeightLine = new Point(testRoom.FloorLevel, testRoom.FloorLevel + testRoom.Height);
                            if (testLineSegments(heightLine, testHeightLine))
                            {
                                intersectingLines.Add(testWestSide);
                                intersectingRooms.Add(testRoom);
                                intesects++;
                            }
                        }
                    }
                }
            }
            return intesects;
        }

        /// <summary>
        ///     Devuelve todos los segmentos de linea correspondiente a la pared East de cada room
        ///     que colisionan con la pared West de este room.
        ///     Tambien informa los rooms contra los cuales colisiono y la cantidad de colisiones encontradas
        /// </summary>
        private int findIntersectingWestWall(List<RoomsEditorRoom> rooms, out List<Point> intersectingLines,
            out List<RoomsEditorRoom> intersectingRooms)
        {
            var weastSide = new Point(RoomPanel.Label.Location.Y, RoomPanel.Label.Location.Y + RoomPanel.Label.Height);
            var westX = RoomPanel.Label.Location.X;
            var heightLine = new Point(FloorLevel, FloorLevel + Height);

            intersectingLines = new List<Point>();
            intersectingRooms = new List<RoomsEditorRoom>();
            var intesects = 0;
            foreach (var testRoom in rooms)
            {
                if (testRoom != this)
                {
                    var testEastSide = new Point(testRoom.RoomPanel.Label.Location.Y,
                        testRoom.RoomPanel.Label.Location.Y + testRoom.RoomPanel.Label.Height);
                    var testEastX = testRoom.RoomPanel.Label.Location.X + testRoom.RoomPanel.Label.Width;
                    if (westX == testEastX)
                    {
                        if (testLineSegments(weastSide, testEastSide))
                        {
                            var testHeightLine = new Point(testRoom.FloorLevel, testRoom.FloorLevel + testRoom.Height);
                            if (testLineSegments(heightLine, testHeightLine))
                            {
                                intersectingLines.Add(testEastSide);
                                intersectingRooms.Add(testRoom);
                                intesects++;
                            }
                        }
                    }
                }
            }
            return intesects;
        }

        /// <summary>
        ///     Devuelve todos los segmentos de linea correspondiente a la pared North de cada room
        ///     que colisionan con la pared South de este room.
        ///     Tambien informa los rooms contra los cuales colisiono y la cantidad de colisiones encontradas
        /// </summary>
        private int findIntersectingSouthWall(List<RoomsEditorRoom> rooms, out List<Point> intersectingLines,
            out List<RoomsEditorRoom> intersectingRooms)
        {
            var southSide = new Point(RoomPanel.Label.Location.X, RoomPanel.Label.Location.X + RoomPanel.Label.Width);
            var southY = RoomPanel.Label.Location.Y + RoomPanel.Label.Height;
            var heightLine = new Point(FloorLevel, FloorLevel + Height);

            intersectingLines = new List<Point>();
            intersectingRooms = new List<RoomsEditorRoom>();
            var intesects = 0;
            foreach (var testRoom in rooms)
            {
                if (testRoom != this)
                {
                    var testNorthSide = new Point(testRoom.RoomPanel.Label.Location.X,
                        testRoom.RoomPanel.Label.Location.X + testRoom.RoomPanel.Label.Width);
                    var testNorthY = testRoom.RoomPanel.Label.Location.Y;
                    if (southY == testNorthY)
                    {
                        if (testLineSegments(southSide, testNorthSide))
                        {
                            var testHeightLine = new Point(testRoom.FloorLevel, testRoom.FloorLevel + testRoom.Height);
                            if (testLineSegments(heightLine, testHeightLine))
                            {
                                intersectingLines.Add(testNorthSide);
                                intersectingRooms.Add(testRoom);
                                intesects++;
                            }
                        }
                    }
                }
            }
            return intesects;
        }

        /// <summary>
        ///     Devuelve todos los segmentos de linea correspondiente a la pared South de cada room
        ///     que colisionan con la pared North de este room.
        ///     Tambien informa los rooms contra los cuales colisiono y la cantidad de colisiones encontradas
        /// </summary>
        private int findIntersectingNorthWall(List<RoomsEditorRoom> rooms, out List<Point> intersectingLines,
            out List<RoomsEditorRoom> intersectingRooms)
        {
            var northSide = new Point(RoomPanel.Label.Location.X, RoomPanel.Label.Location.X + RoomPanel.Label.Width);
            var northY = RoomPanel.Label.Location.Y;
            var heightLine = new Point(FloorLevel, FloorLevel + Height);

            intersectingLines = new List<Point>();
            intersectingRooms = new List<RoomsEditorRoom>();
            var intesects = 0;
            foreach (var testRoom in rooms)
            {
                if (testRoom != this)
                {
                    var testSouthSide = new Point(testRoom.RoomPanel.Label.Location.X,
                        testRoom.RoomPanel.Label.Location.X + testRoom.RoomPanel.Label.Width);
                    var testSouthY = testRoom.RoomPanel.Label.Location.Y + testRoom.RoomPanel.Label.Height;
                    if (northY == testSouthY)
                    {
                        if (testLineSegments(northSide, testSouthSide))
                        {
                            var testHeightLine = new Point(testRoom.FloorLevel, testRoom.FloorLevel + testRoom.Height);
                            if (testLineSegments(heightLine, testHeightLine))
                            {
                                intersectingLines.Add(testSouthSide);
                                intersectingRooms.Add(testRoom);
                                intesects++;
                            }
                        }
                    }
                }
            }
            return intesects;
        }

        /// <summary>
        ///     Indica si dos segmentos de recta colisionan entre si
        /// </summary>
        private bool testLineSegments(Point l1, Point l2)
        {
            return (l1.X <= l2.X && l2.X < l1.Y) || (l2.X < l1.X && l1.X < l2.Y);
        }

        /// <summary>
        ///     Devuelve el segmento de recta que hay que sacarle a L2, al intersectarlo
        ///     con el segmento L1.
        ///     Cada coordenada de un Point es el punto inicial y final de la recta.
        ///     No importa si es vertical o horizontal
        /// </summary>
        private Point intersectLineSegments(Point l1, Point l2)
        {
            Point p;
            if (l1.X > l2.X)
            {
                if (l1.Y > l2.Y)
                {
                    p = new Point(l1.X, l2.Y);
                }
                else
                {
                    p = new Point(l1.X, l1.Y);
                }
            }
            else
            {
                if (l1.Y > l2.Y)
                {
                    p = new Point(l2.X, l2.Y);
                }
                else
                {
                    p = new Point(l2.X, l1.Y);
                }
            }

            return p;
        }

        /// <summary>
        ///     Quita al segmento l todos los segmentos de recta y devuelve los segmentos que quedaron
        ///     de la version original de l
        /// </summary>
        private List<Point> removeSegmentsFromLine(Point l, List<Point> segments)
        {
            //Ordenar segmentos en forma ascendente
            segments.Sort(new LineSegmentComparer());

            //Ir fragmentando el segmento original segun los pedazos que hay que quitar
            var finalSegments = new List<Point>();
            var init = l.X;
            foreach (var seg in segments)
            {
                var newSeg = new Point(init, seg.X);
                //Solo agregar si tiene longitud
                if (newSeg.Y - newSeg.X > 0)
                {
                    finalSegments.Add(newSeg);
                }
                init = seg.Y;
            }
            //Agregar ultimo tramo, si hay
            var lastSeg = new Point(init, l.Y);
            if (lastSeg.Y - lastSeg.X > 0)
            {
                finalSegments.Add(lastSeg);
            }

            return finalSegments;
        }

        /// <summary>
        ///     Comparador de Segmento de recta
        /// </summary>
        private class LineSegmentComparer : IComparer<Point>
        {
            public int Compare(Point a, Point b)
            {
                if (a.X < b.X) return -1;
                if (a.X > b.X) return 1;
                return 0;
            }
        }
    }
}