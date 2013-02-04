using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;

namespace Examples.RoomsEditor
{
    /// <summary>
    /// Información lógica y visual de un Room
    /// </summary>
    public class RoomsEditorRoom
    {
        RoomsEditorMapView.RoomPanel roomPanel;
        /// <summary>
        /// Panel visual 2D del room
        /// </summary>
        public RoomsEditorMapView.RoomPanel RoomPanel
        {
            get { return roomPanel; }
        }

        string name;
        /// <summary>
        /// Nombre del room
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        int height;
        /// <summary>
        /// Altura de todas las paredes del room
        /// </summary>
        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        int floorLevel;
        /// <summary>
        /// Altura del piso
        /// </summary>
        public int FloorLevel
        {
            get { return floorLevel; }
            set { floorLevel = value; }
        }

        RoomsEditorWall[] walls;
        /// <summary>
        /// Paredes de este cuarto
        /// </summary>
        public RoomsEditorWall[] Walls
        {
            get { return walls; }
        }

        public RoomsEditorRoom(string name, RoomsEditorMapView.RoomPanel roomPanel)
        {
            this.name = name;
            this.roomPanel = roomPanel;

            this.roomPanel.Room = this;

            //crear paredes
            walls = new RoomsEditorWall[6];
            walls[0] = new RoomsEditorWall(this, "Roof");
            walls[1] = new RoomsEditorWall(this, "Floor");
            walls[2] = new RoomsEditorWall(this, "East");
            walls[3] = new RoomsEditorWall(this, "West");
            walls[4] = new RoomsEditorWall(this, "North");
            walls[5] = new RoomsEditorWall(this, "South");

            //Valores default
            height = 100;
            floorLevel = 0;
            
        }

        public void dispose()
        {
            foreach (RoomsEditorWall wall in walls)
            {
                wall.dispose();
            }
        }

        /// <summary>
        /// Actualiza la información de las paredes
        /// </summary>
        public void buildWalls(List<RoomsEditorRoom> rooms, Vector3 sceneScale)
        {
            Rectangle bounds = roomPanel.Label.Bounds;

            //roof
            RoomsEditorWall roof = walls[0];
            TgcPlaneWall roofWall3d = new TgcPlaneWall(
                new Vector3(bounds.X * sceneScale.X, (floorLevel + height) * sceneScale.Y, bounds.Y * sceneScale.Z),
                new Vector3(bounds.Width * sceneScale.X, 0, bounds.Height * sceneScale.Z),
                TgcPlaneWall.Orientations.XZplane, 
                roof.Texture, roof.UTile, roof.VTile
                );
            roof.WallSegments.Clear();
            roof.WallSegments.Add(roofWall3d);


            //floor
            RoomsEditorWall floor = walls[1];
            TgcPlaneWall floorWall3d = new TgcPlaneWall(
                new Vector3(bounds.X * sceneScale.X, floorLevel, bounds.Y * sceneScale.Z),
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
            RoomsEditorWall eastWall = walls[2];

            //buscar colisiones y particionar
            wallSegment = new Point(bounds.Y, bounds.Y + bounds.Height);
            findIntersectingEastWall(rooms, out intersectingLines, out intersectingRooms);
            finalWallSegments = fragmentWall(eastWall, wallSegment, intersectingLines, intersectingRooms);
            
            //Crear una una pared 3D por cada segmento final que quedó
            eastWall.WallSegments.Clear();
            foreach (Point seg in finalWallSegments)
            {
                TgcPlaneWall wall3d = new TgcPlaneWall(
                    new Vector3((bounds.X + bounds.Width) * sceneScale.X, floorLevel * sceneScale.Y, seg.X * sceneScale.Z),
                    new Vector3(0, height * sceneScale.Y, (seg.Y - seg.X) * sceneScale.Z),
                    TgcPlaneWall.Orientations.YZplane,
                    eastWall.Texture, eastWall.UTile, eastWall.VTile
                    );
                eastWall.WallSegments.Add(wall3d);
            }

            //Crear paredes para las diferencias de alturas con los otros Rooms
            createEastWallSegmentsForHeightDiff(sceneScale, intersectingLines, intersectingRooms, eastWall);



            
            //west wall
            RoomsEditorWall westWall = walls[3];

            //buscar colisiones y particionar
            wallSegment = new Point(bounds.Y, bounds.Y + bounds.Height);
            findIntersectingWestWall(rooms, out intersectingLines, out intersectingRooms);
            finalWallSegments = fragmentWall(eastWall, wallSegment, intersectingLines, intersectingRooms);

            //Crear una una pared 3D por cada segmento final que quedó
            westWall.WallSegments.Clear();
            foreach (Point seg in finalWallSegments)
            {
                TgcPlaneWall wall3d = new TgcPlaneWall(
                    new Vector3(bounds.X * sceneScale.X, floorLevel * sceneScale.Y, seg.X * sceneScale.Z),
                    new Vector3(0, height * sceneScale.Y, (seg.Y - seg.X) * sceneScale.Z),
                    TgcPlaneWall.Orientations.YZplane,
                    westWall.Texture, westWall.UTile, westWall.VTile
                    );
                westWall.WallSegments.Add(wall3d);
            }
            //Crear paredes para las diferencias de alturas con los otros Rooms
            createWestWallSegmentsForHeightDiff(sceneScale, intersectingLines, intersectingRooms, westWall);
            

            
            //north wall
            RoomsEditorWall northWall = walls[4];

            //buscar colisiones y particionar
            wallSegment = new Point(bounds.X, bounds.X + bounds.Width);
            findIntersectingNorthWall(rooms, out intersectingLines, out intersectingRooms);
            finalWallSegments = fragmentWall(eastWall, wallSegment, intersectingLines, intersectingRooms);

            //Crear una una pared 3D por cada segmento final que quedó
            northWall.WallSegments.Clear();
            foreach (Point seg in finalWallSegments)
            {
                TgcPlaneWall wall3d = new TgcPlaneWall(
                        new Vector3(seg.X * sceneScale.X, floorLevel * sceneScale.Y, bounds.Y * sceneScale.Z),
                        new Vector3((seg.Y - seg.X) * sceneScale.X, height * sceneScale.Y, 0),
                        TgcPlaneWall.Orientations.XYplane,
                        northWall.Texture, northWall.UTile, northWall.VTile
                        );
                northWall.WallSegments.Add(wall3d);
            }
            //Crear paredes para las diferencias de alturas con los otros Rooms
            createNorthWallSegmentsForHeightDiff(sceneScale, intersectingLines, intersectingRooms, northWall);

            
            //south wall
            RoomsEditorWall southWall = walls[5];

            //buscar colisiones y particionar
            wallSegment = new Point(bounds.X, bounds.X + bounds.Width);
            findIntersectingSouthWall(rooms, out intersectingLines, out intersectingRooms);
            finalWallSegments = fragmentWall(eastWall, wallSegment, intersectingLines, intersectingRooms);

            //Crear una una pared 3D por cada segmento final que quedó
            southWall.WallSegments.Clear();
            foreach (Point seg in finalWallSegments)
            {
                TgcPlaneWall wall3d = new TgcPlaneWall(
                        new Vector3(seg.X * sceneScale.X, floorLevel * sceneScale.Y, (bounds.Y + bounds.Height) * sceneScale.Z),
                        new Vector3((seg.Y - seg.X) * sceneScale.X, height * sceneScale.Y, 0),
                        TgcPlaneWall.Orientations.XYplane,
                        southWall.Texture, southWall.UTile, southWall.VTile
                        );
                southWall.WallSegments.Add(wall3d);
            }
            //Crear paredes para las diferencias de alturas con los otros Rooms
            createSouthWallSegmentsForHeightDiff(sceneScale, intersectingLines, intersectingRooms, southWall);
            
        }
        
        /// <summary>
        /// Crear paredes 3D para las diferencias de altura entre la pared East de este Room y el resto
        /// de los Rooms contra los que colisiona
        /// </summary>
        private void createEastWallSegmentsForHeightDiff(Vector3 sceneScale, List<Point> intersectingLines, List<RoomsEditorRoom> intersectingRooms, RoomsEditorWall wall)
        {
            Rectangle bounds = roomPanel.Label.Bounds;
            Point wallSide = new Point(roomPanel.Label.Location.Y, roomPanel.Label.Location.Y + roomPanel.Label.Height);
            Point[] supDiffSegments;
            Point[] infDiffSegments;
            findSegmentsForHeightDiff(intersectingRooms, out supDiffSegments, out infDiffSegments);
            for (int i = 0; i < intersectingRooms.Count; i++)
            {
                Point fullIntersecLine = intersectingLines[i];
                Point intersecLine = intersectLineSegments(fullIntersecLine, wallSide);

                //Fragmento superior
                Point supSeg = supDiffSegments[i];
                if (supSeg.X != 0 || supSeg.Y != 0)
                {
                    TgcPlaneWall wall3d = new TgcPlaneWall(
                        new Vector3((bounds.X + bounds.Width) * sceneScale.X, supSeg.X * sceneScale.Y, intersecLine.X * sceneScale.Z),
                        new Vector3(0, (supSeg.Y - supSeg.X) * sceneScale.Y, (intersecLine.Y - intersecLine.X) * sceneScale.Z),
                        TgcPlaneWall.Orientations.YZplane,
                        wall.Texture, wall.UTile, wall.VTile
                        );
                    wall.WallSegments.Add(wall3d);
                }

                //Fragmento inferior
                Point infSeg = infDiffSegments[i];
                if (infSeg.X != 0 || infSeg.Y != 0)
                {
                    TgcPlaneWall wall3d = new TgcPlaneWall(
                        new Vector3((bounds.X + bounds.Width) * sceneScale.X, infSeg.X * sceneScale.Y, intersecLine.X * sceneScale.Z),
                        new Vector3(0, (infSeg.Y - infSeg.X) * sceneScale.Y, (intersecLine.Y - intersecLine.X) * sceneScale.Z),
                        TgcPlaneWall.Orientations.YZplane,
                        wall.Texture, wall.UTile, wall.VTile
                        );
                    wall.WallSegments.Add(wall3d);
                }
            }
        }


        /// <summary>
        /// Crear paredes 3D para las diferencias de altura entre la pared West de este Room y el resto
        /// de los Rooms contra los que colisiona
        /// </summary>
        private void createWestWallSegmentsForHeightDiff(Vector3 sceneScale, List<Point> intersectingLines, List<RoomsEditorRoom> intersectingRooms, RoomsEditorWall wall)
        {
            Rectangle bounds = roomPanel.Label.Bounds;
            Point wallSide = new Point(roomPanel.Label.Location.Y, roomPanel.Label.Location.Y + roomPanel.Label.Height);
            Point[] supDiffSegments;
            Point[] infDiffSegments;
            findSegmentsForHeightDiff(intersectingRooms, out supDiffSegments, out infDiffSegments);
            for (int i = 0; i < intersectingRooms.Count; i++)
            {
                Point fullIntersecLine = intersectingLines[i];
                Point intersecLine = intersectLineSegments(fullIntersecLine, wallSide);

                //Fragmento superior
                Point supSeg = supDiffSegments[i];
                if (supSeg.X != 0 || supSeg.Y != 0)
                {
                    TgcPlaneWall wall3d = new TgcPlaneWall(
                        new Vector3(bounds.X * sceneScale.X, supSeg.X * sceneScale.Y, intersecLine.X * sceneScale.Z),
                        new Vector3(0, (supSeg.Y - supSeg.X) * sceneScale.Y, (intersecLine.Y - intersecLine.X) * sceneScale.Z),
                        TgcPlaneWall.Orientations.YZplane,
                        wall.Texture, wall.UTile, wall.VTile
                        );
                    wall.WallSegments.Add(wall3d);
                }

                //Fragmento inferior
                Point infSeg = infDiffSegments[i];
                if (infSeg.X != 0 || infSeg.Y != 0)
                {
                    TgcPlaneWall wall3d = new TgcPlaneWall(
                        new Vector3(bounds.X * sceneScale.X, infSeg.X * sceneScale.Y, intersecLine.X * sceneScale.Z),
                        new Vector3(0, (infSeg.Y - infSeg.X) * sceneScale.Y, (intersecLine.Y - intersecLine.X) * sceneScale.Z),
                        TgcPlaneWall.Orientations.YZplane,
                        wall.Texture, wall.UTile, wall.VTile
                        );
                    wall.WallSegments.Add(wall3d);
                }
            }
        }

        /// <summary>
        /// Crear paredes 3D para las diferencias de altura entre la pared North de este Room y el resto
        /// de los Rooms contra los que colisiona
        /// </summary>
        private void createNorthWallSegmentsForHeightDiff(Vector3 sceneScale, List<Point> intersectingLines, List<RoomsEditorRoom> intersectingRooms, RoomsEditorWall wall)
        {
            Rectangle bounds = roomPanel.Label.Bounds;
            Point wallSide = new Point(roomPanel.Label.Location.X, roomPanel.Label.Location.X + roomPanel.Label.Width);
            Point[] supDiffSegments;
            Point[] infDiffSegments;
            findSegmentsForHeightDiff(intersectingRooms, out supDiffSegments, out infDiffSegments);
            for (int i = 0; i < intersectingRooms.Count; i++)
            {
                Point fullIntersecLine = intersectingLines[i];
                Point intersecLine = intersectLineSegments(fullIntersecLine, wallSide);

                //Fragmento superior
                Point supSeg = supDiffSegments[i];
                if (supSeg.X != 0 || supSeg.Y != 0)
                {
                    TgcPlaneWall wall3d = new TgcPlaneWall(
                        new Vector3(intersecLine.X * sceneScale.X, supSeg.X * sceneScale.Y, bounds.Y * sceneScale.Z),
                        new Vector3((intersecLine.Y - intersecLine.X) * sceneScale.X, (supSeg.Y - supSeg.X) * sceneScale.Y, 0),
                        TgcPlaneWall.Orientations.XYplane,
                        wall.Texture, wall.UTile, wall.VTile
                     );
                    wall.WallSegments.Add(wall3d);
                }

                //Fragmento inferior
                Point infSeg = infDiffSegments[i];
                if (infSeg.X != 0 || infSeg.Y != 0)
                {
                    TgcPlaneWall wall3d = new TgcPlaneWall(
                        new Vector3(intersecLine.X * sceneScale.X, infSeg.X * sceneScale.Y, bounds.Y * sceneScale.Z),
                        new Vector3((intersecLine.Y - intersecLine.X) * sceneScale.X, (infSeg.Y - infSeg.X) * sceneScale.Y, 0),
                        TgcPlaneWall.Orientations.XYplane,
                        wall.Texture, wall.UTile, wall.VTile
                     );
                    wall.WallSegments.Add(wall3d);
                }
            }
        }

        /// <summary>
        /// Crear paredes 3D para las diferencias de altura entre la pared South de este Room y el resto
        /// de los Rooms contra los que colisiona
        /// </summary>
        private void createSouthWallSegmentsForHeightDiff(Vector3 sceneScale, List<Point> intersectingLines, List<RoomsEditorRoom> intersectingRooms, RoomsEditorWall wall)
        {
            Rectangle bounds = roomPanel.Label.Bounds;
            Point wallSide = new Point(roomPanel.Label.Location.X, roomPanel.Label.Location.X + roomPanel.Label.Width);
            Point[] supDiffSegments;
            Point[] infDiffSegments;
            findSegmentsForHeightDiff(intersectingRooms, out supDiffSegments, out infDiffSegments);
            for (int i = 0; i < intersectingRooms.Count; i++)
            {
                Point fullIntersecLine = intersectingLines[i];
                Point intersecLine = intersectLineSegments(fullIntersecLine, wallSide);

                //Fragmento superior
                Point supSeg = supDiffSegments[i];
                if (supSeg.X != 0 || supSeg.Y != 0)
                {
                    TgcPlaneWall wall3d = new TgcPlaneWall(
                        new Vector3(intersecLine.X * sceneScale.X, supSeg.X * sceneScale.Y, (bounds.Y + bounds.Height) * sceneScale.Z),
                        new Vector3((intersecLine.Y - intersecLine.X) * sceneScale.X, (supSeg.Y - supSeg.X) * sceneScale.Y, 0),
                        TgcPlaneWall.Orientations.XYplane,
                        wall.Texture, wall.UTile, wall.VTile
                     );
                    wall.WallSegments.Add(wall3d);
                }

                //Fragmento inferior
                Point infSeg = infDiffSegments[i];
                if (infSeg.X != 0 || infSeg.Y != 0)
                {
                    TgcPlaneWall wall3d = new TgcPlaneWall(
                        new Vector3(intersecLine.X * sceneScale.X, infSeg.X * sceneScale.Y, (bounds.Y + bounds.Height) * sceneScale.Z),
                        new Vector3((intersecLine.Y - intersecLine.X) * sceneScale.X, (infSeg.Y - infSeg.X) * sceneScale.Y, 0),
                        TgcPlaneWall.Orientations.XYplane,
                        wall.Texture, wall.UTile, wall.VTile
                     );
                    wall.WallSegments.Add(wall3d);
                }
            }
        }
        
        /// <summary>
        /// Busca los segmentos de pared a generar en base a diferencia de alturas de las paredes.
        /// Por cada intersectingRoom carga 2 segmentos en supDiffSegments y infDiffSegments (segmento superior e inferior).
        /// Si alguno no hace falta la posición está en null.
        /// </summary>
        private void findSegmentsForHeightDiff(List<RoomsEditorRoom> intersectingRooms, out Point[] supDiffSegments, out Point[] infDiffSegments)
        {
            supDiffSegments = new Point[intersectingRooms.Count];
            infDiffSegments = new Point[intersectingRooms.Count];
            Point roomHeightLine = new Point(floorLevel, floorLevel + height);

            for (int i = 0; i < intersectingRooms.Count; i++)
            {
                RoomsEditorRoom interRoom = intersectingRooms[i];

                //si hay diferencias de alturas, truncar
                if (floorLevel != interRoom.floorLevel || height != interRoom.height)
                {
                    Point interRoomHeightLine = new Point(interRoom.floorLevel, interRoom.floorLevel + interRoom.height);
                    List<Point> segmentsToExtract = new List<Point>();
                    segmentsToExtract.Add(intersectLineSegments(interRoomHeightLine, roomHeightLine));
                    List<Point> finalSegments = removeSegmentsFromLine(roomHeightLine, segmentsToExtract);

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
                        if (finalSegments[0].X == floorLevel)
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
        /// Fragmenta una pared en varias subparedes, en base a todas las colisiones que hubo con cuartos vecinos
        /// </summary>
        private List<Point> fragmentWall(RoomsEditorWall wall, Point wallSegment, List<Point> intersectingLines, List<RoomsEditorRoom> intersectingRooms)
        {
            List<Point> finalWallSegments;
            wall.IntersectingRooms.Clear();

            //Hubo intersecciones, fragmentar
            if (intersectingRooms.Count > 0)
            {
                //Agregar rooms vecinos
                wall.IntersectingRooms.AddRange(intersectingRooms);

                //Calcular intersecciones con vecinos en esta pared que hay que quitar de la pared original
                List<Point> segmentsToExtract = new List<Point>();
                foreach (Point lineSegment in intersectingLines)
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
        /// Devuelve todos los segmentos de linea correspondiente a la pared West de cada room
        /// que colisionan con la pared East de este room.
        /// Tambien informa los rooms contra los cuales colisiono y la cantidad de colisiones encontradas
        /// </summary>
        private int findIntersectingEastWall(List<RoomsEditorRoom> rooms, out List<Point> intersectingLines, out List<RoomsEditorRoom> intersectingRooms)
        {
            Point eastSide = new Point(roomPanel.Label.Location.Y, roomPanel.Label.Location.Y + roomPanel.Label.Height);
            int eastX = roomPanel.Label.Location.X + roomPanel.Label.Width;
            Point heightLine = new Point(floorLevel, floorLevel + height);

            intersectingLines = new List<Point>();
            intersectingRooms = new List<RoomsEditorRoom>();
            int intesects = 0;
            foreach (RoomsEditorRoom testRoom in rooms)
            {
                if (testRoom != this)
                {
                    Point testWestSide = new Point(testRoom.roomPanel.Label.Location.Y, testRoom.roomPanel.Label.Location.Y + testRoom.roomPanel.Label.Height);
                    int testWestX = testRoom.roomPanel.Label.Location.X;
                    if (eastX == testWestX)
                    {
                        if (testLineSegments(eastSide, testWestSide))
                        {
                            Point testHeightLine = new Point(testRoom.FloorLevel, testRoom.FloorLevel + testRoom.Height);
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
        /// Devuelve todos los segmentos de linea correspondiente a la pared East de cada room
        /// que colisionan con la pared West de este room.
        /// Tambien informa los rooms contra los cuales colisiono y la cantidad de colisiones encontradas
        /// </summary>
        private int findIntersectingWestWall(List<RoomsEditorRoom> rooms, out List<Point> intersectingLines, out List<RoomsEditorRoom> intersectingRooms)
        {
            Point weastSide = new Point(roomPanel.Label.Location.Y, roomPanel.Label.Location.Y + roomPanel.Label.Height);
            int westX = roomPanel.Label.Location.X;
            Point heightLine = new Point(floorLevel, floorLevel + height);

            intersectingLines = new List<Point>();
            intersectingRooms = new List<RoomsEditorRoom>();
            int intesects = 0;
            foreach (RoomsEditorRoom testRoom in rooms)
            {
                if (testRoom != this)
                {
                    Point testEastSide = new Point(testRoom.roomPanel.Label.Location.Y, testRoom.roomPanel.Label.Location.Y + testRoom.roomPanel.Label.Height);
                    int testEastX = testRoom.roomPanel.Label.Location.X + testRoom.roomPanel.Label.Width;
                    if (westX == testEastX)
                    {
                        if (testLineSegments(weastSide, testEastSide))
                        {
                            Point testHeightLine = new Point(testRoom.FloorLevel, testRoom.FloorLevel + testRoom.Height);
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
        /// Devuelve todos los segmentos de linea correspondiente a la pared North de cada room
        /// que colisionan con la pared South de este room.
        /// Tambien informa los rooms contra los cuales colisiono y la cantidad de colisiones encontradas
        /// </summary>
        private int findIntersectingSouthWall(List<RoomsEditorRoom> rooms, out List<Point> intersectingLines, out List<RoomsEditorRoom> intersectingRooms)
        {
            Point southSide = new Point(roomPanel.Label.Location.X, roomPanel.Label.Location.X + roomPanel.Label.Width);
            int southY = roomPanel.Label.Location.Y + roomPanel.Label.Height;
            Point heightLine = new Point(floorLevel, floorLevel + height);

            intersectingLines = new List<Point>();
            intersectingRooms = new List<RoomsEditorRoom>();
            int intesects = 0;
            foreach (RoomsEditorRoom testRoom in rooms)
            {
                if (testRoom != this)
                {
                    Point testNorthSide = new Point(testRoom.roomPanel.Label.Location.X, testRoom.roomPanel.Label.Location.X + testRoom.roomPanel.Label.Width);
                    int testNorthY = testRoom.roomPanel.Label.Location.Y;
                    if (southY == testNorthY)
                    {
                        if (testLineSegments(southSide, testNorthSide))
                        {
                            Point testHeightLine = new Point(testRoom.FloorLevel, testRoom.FloorLevel + testRoom.Height);
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
        /// Devuelve todos los segmentos de linea correspondiente a la pared South de cada room
        /// que colisionan con la pared North de este room.
        /// Tambien informa los rooms contra los cuales colisiono y la cantidad de colisiones encontradas
        /// </summary>
        private int findIntersectingNorthWall(List<RoomsEditorRoom> rooms, out List<Point> intersectingLines, out List<RoomsEditorRoom> intersectingRooms)
        {
            Point northSide = new Point(roomPanel.Label.Location.X, roomPanel.Label.Location.X + roomPanel.Label.Width);
            int northY = roomPanel.Label.Location.Y;
            Point heightLine = new Point(floorLevel, floorLevel + height);

            intersectingLines = new List<Point>();
            intersectingRooms = new List<RoomsEditorRoom>();
            int intesects = 0;
            foreach (RoomsEditorRoom testRoom in rooms)
            {
                if (testRoom != this)
                {
                    Point testSouthSide = new Point(testRoom.roomPanel.Label.Location.X, testRoom.roomPanel.Label.Location.X + testRoom.roomPanel.Label.Width);
                    int testSouthY = testRoom.roomPanel.Label.Location.Y + testRoom.roomPanel.Label.Height;
                    if (northY == testSouthY)
                    {
                        if (testLineSegments(northSide, testSouthSide))
                        {
                            Point testHeightLine = new Point(testRoom.FloorLevel, testRoom.FloorLevel + testRoom.Height);
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
        /// Indica si dos segmentos de recta colisionan entre si
        /// </summary>
        private bool testLineSegments(Point l1, Point l2)
        {
            return (l1.X <= l2.X && l2.X < l1.Y) || (l2.X < l1.X && l1.X < l2.Y);
        }


        /// <summary>
        /// Devuelve el segmento de recta que hay que sacarle a L2, al intersectarlo
        /// con el segmento L1.
        /// Cada coordenada de un Point es el punto inicial y final de la recta.
        /// No importa si es vertical o horizontal
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
        /// Quita al segmento l todos los segmentos de recta y devuelve los segmentos que quedaron
        /// de la version original de l
        /// </summary>
        private List<Point> removeSegmentsFromLine(Point l, List<Point> segments)
        {
            //Ordenar segmentos en forma ascendente
            segments.Sort(new LineSegmentComparer());

            //Ir fragmentando el segmento original segun los pedazos que hay que quitar
            List<Point> finalSegments = new List<Point>();
            int init = l.X;
            foreach (Point seg in segments)
            {
                Point newSeg = new Point(init, seg.X);
                //Solo agregar si tiene longitud
                if (newSeg.Y - newSeg.X > 0)
                {
                    finalSegments.Add(newSeg);
                }
                init = seg.Y;
            }
            //Agregar ultimo tramo, si hay
            Point lastSeg = new Point(init, l.Y);
            if (lastSeg.Y - lastSeg.X > 0)
            {
                finalSegments.Add(lastSeg);
            }

            return finalSegments;
        }

        /// <summary>
        /// Comparador de Segmento de recta
        /// </summary>
        private class LineSegmentComparer : IComparer<Point>
        {
            public int Compare(Point a, Point b)
            {
                if (a.X < b.X) return -1;
                if(a.X > b.X) return 1;
                return 0;
            }
        }


    }
}
