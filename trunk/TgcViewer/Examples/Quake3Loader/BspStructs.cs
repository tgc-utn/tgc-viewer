﻿using System;
using System.Text;
using Microsoft.DirectX;
using System.Drawing;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.Quake3Loader
{
    /*
    * Estructuras para parsear el archivo binario BSP
    */


    internal enum LumpEnum
    {
        Entities = 0,
        Shaders,
        Planes,
        Nodes,
        Leafs,
        Leafsurfaces,
        Leafbrushes,
        Models,
        Brushes,
        Brushsides,
        Drawverts,
        Drawindexes,
        Fogs,
        Surfaces,
        Lightmaps,
        Lightgrid,
        Visibility,
        Header
    }

    class Lump
    {
        public int fileofs;
        public int filelen;
        public Lump() { }
    }

    public class QModel
    {
        public const int SIZE = 40;
        public Vector3 min, max;
        public int firstSurface, numSurfaces;
        public int firstBrush, numBrushes;
        public QModel() { }

        public void LoadFromByteArray(byte[] buffer, int offset)
        {
            min = new Vector3(BitConverter.ToSingle(buffer, offset + 0),
               BitConverter.ToSingle(buffer, offset + 4),
               BitConverter.ToSingle(buffer, offset + 8));

            max = new Vector3(BitConverter.ToSingle(buffer, offset + 12),
               BitConverter.ToSingle(buffer, offset + 16),
               BitConverter.ToSingle(buffer, offset + 20));

            firstSurface = BitConverter.ToInt32(buffer, offset + 24);
            numSurfaces = BitConverter.ToInt32(buffer, offset + 28);
            firstBrush = BitConverter.ToInt32(buffer, offset + 32);
            numBrushes = BitConverter.ToInt32(buffer, offset + 36);

        }
    }

    public class QShader
    {
        public const int SIZE = 72;
        public const int MAX_QPATH = 64;
        public string shader;
        public int surfaceFlags;
        public int contentFlags;
        public QShader() { }

        public void LoadFromByteArray(byte[] buffer, int offset)
        {
            ASCIIEncoding enc = new ASCIIEncoding();
            shader = enc.GetString(buffer, offset, MAX_QPATH);

            surfaceFlags = BitConverter.ToInt32(buffer, offset + MAX_QPATH);
            contentFlags = BitConverter.ToInt32(buffer, offset + MAX_QPATH + 4);
        }
    }

    // planes x^1 is allways the opposite of plane x

    public class QPlane
    {
        public const int SIZE = 16;
        public Vector3 normal;
        public float dist;
        public QPlane() { }

        public void LoadFromByteArray(byte[] buffer, int offset)
        {
            // en el archivo BSP Usan la Z como si fuera el Eje vertical,
            //aca hay que invertir el Z por el Y
            normal = new Vector3(BitConverter.ToSingle(buffer, offset + 0),
               BitConverter.ToSingle(buffer, offset + 8),
               BitConverter.ToSingle(buffer, offset + 4));

            dist = BitConverter.ToSingle(buffer, offset + 12);
        }
    }

    public class QNode
    {
        public const int SIZE = 36;
        public int planeNum;
        public int[] children = new int[2];	// negative numbers are -(leafs+1), not nodes
        public int[] mins = new int[3];		// for frustom culling
        public int[] maxs = new int[3];
        public QNode() { }

        public void LoadFromByteArray(byte[] buffer, int offset)
        {
            planeNum = BitConverter.ToInt32(buffer, offset);

            children[0] = BitConverter.ToInt32(buffer, offset + 4);
            children[1] = BitConverter.ToInt32(buffer, offset + 8);

            mins[0] = BitConverter.ToInt32(buffer, offset + 12);
            mins[1] = BitConverter.ToInt32(buffer, offset + 16);
            mins[2] = BitConverter.ToInt32(buffer, offset + 20);

            maxs[0] = BitConverter.ToInt32(buffer, offset + 24);
            maxs[1] = BitConverter.ToInt32(buffer, offset + 28);
            maxs[2] = BitConverter.ToInt32(buffer, offset + 32);
        }
    }

    public class QLeaf
    {
        public const int SIZE = 48;
        public int cluster;			// -1 = opaque cluster (do I still store these?)
        public int area;

        public int[] mins = new int[3];			// Minimos y maximos de bounding box de la hoja
        public int[] maxs = new int[3];
        public TgcBoundingBox boundingBox;

        public int firstLeafSurface;
        public int numLeafSurfaces;

        public int firstLeafBrush;
        public int numLeafBrushes;
        public QLeaf() { }

        public void LoadFromByteArray(byte[] buffer, int offset)
        {
            cluster = BitConverter.ToInt32(buffer, offset);
            area = BitConverter.ToInt32(buffer, offset + 4);

            // en el archivo BSP Usan la Z como si fuera el Eje vertical,
            //aca hay que invertir el Z por el Y
            mins[0] = BitConverter.ToInt32(buffer, offset + 8);
            mins[1] = BitConverter.ToInt32(buffer, offset + 16);
            mins[2] = BitConverter.ToInt32(buffer, offset + 12);

            maxs[0] = BitConverter.ToInt32(buffer, offset + 20);
            maxs[1] = BitConverter.ToInt32(buffer, offset + 28);
            maxs[2] = BitConverter.ToInt32(buffer, offset + 24);

            Vector3 pMin = new Vector3(Math.Min(mins[0], maxs[0]), Math.Min(mins[1], maxs[1]), Math.Min(mins[2], maxs[2]));
            Vector3 pMax = new Vector3(Math.Max(mins[0], maxs[0]), Math.Max(mins[1], maxs[1]), Math.Max(mins[2], maxs[2]));
            boundingBox = new TgcBoundingBox(pMin, pMax);

            firstLeafSurface = BitConverter.ToInt32(buffer, offset + 32);
            numLeafSurfaces = BitConverter.ToInt32(buffer, offset + 36);

            firstLeafBrush = BitConverter.ToInt32(buffer, offset + 40);
            numLeafBrushes = BitConverter.ToInt32(buffer, offset + 44);
        }
    }

    public class QBrushSide
    {
        public const int SIZE = 8;
        public int planeNum;			// positive plane side faces out of the leaf
        public int shaderNum;
        public QBrushSide() { }

        public void LoadFromByteArray(byte[] buffer, int offset)
        {
            planeNum = BitConverter.ToInt32(buffer, offset);
            shaderNum = BitConverter.ToInt32(buffer, offset + 4);
        }
    }

    public class QBrush
    {
        public const int SIZE = 12;
        public int firstSide;
        public int numSides;
        public int shaderNum;		// the shader that determines the contents flags
        public QBrush() { }

        public void LoadFromByteArray(byte[] buffer, int offset)
        {
            firstSide = BitConverter.ToInt32(buffer, offset);
            numSides = BitConverter.ToInt32(buffer, offset + 4);
            shaderNum = BitConverter.ToInt32(buffer, offset + 8);
        }
    }

    public class QFog
    {
        public const int SIZE = 72;
        public const int MAX_QPATH = 64;
        public string shader;
        public int brushNum;
        public int visibleSide;	// the brush side that ray tests need to clip against (-1 == none)

        public QFog() { }

        public void LoadFromByteArray(byte[] buffer, int offset)
        {
            ASCIIEncoding enc = new ASCIIEncoding();
            shader = enc.GetString(buffer, offset, MAX_QPATH);

            brushNum = BitConverter.ToInt32(buffer, offset + MAX_QPATH);
            visibleSide = BitConverter.ToInt32(buffer, offset + MAX_QPATH + 4);
        }
    }

    public class QVisData
    {
        public int nVec;
        public int sizeVec;
        public byte[] data;
        public QVisData() { }
    }

    public class QDrawVert
    {
        public const int SIZE = 44;
        public Vector3 xyz;
        public Vector2 st;
        public Vector2 lightmap;
        public Vector3 normal;
        public int color;
        public QDrawVert() { }

        public void LoadFromByteArray(byte[] buffer, int offset)
        {
            // en el archivo BSP Usan la Z como si fuera el Eje vertical,
            //aca hay que invertir el Z por el Y
            xyz = new Vector3(BitConverter.ToSingle(buffer, offset + 0),
               BitConverter.ToSingle(buffer, offset + 8),
               BitConverter.ToSingle(buffer, offset + 4));

            st = new Vector2(BitConverter.ToSingle(buffer, offset + 12),
                BitConverter.ToSingle(buffer, offset + 16));

            lightmap = new Vector2(BitConverter.ToSingle(buffer, offset + 20),
                BitConverter.ToSingle(buffer, offset + 24));

            normal = new Vector3(BitConverter.ToSingle(buffer, offset + 28),
                BitConverter.ToSingle(buffer, offset + 36),
                BitConverter.ToSingle(buffer, offset + 32));


            color = Color.FromArgb(buffer[offset + 40], buffer[offset + 37], buffer[offset + 38], buffer[offset + 39]).ToArgb();

        }

    }

    public enum QMapSurfaceType
    {
        Bad,
        Planar,
        Patch,
        TriangleSoup,
        Flare
    }

    public class QSurface
    {
        public const int SIZE = 104;
        public int shaderNum;
        public int fogNum;
        public QMapSurfaceType surfaceType;

        public int firstVert;
        public int numVerts;

        public int firstIndex;
        public int numIndexes;

        public int lightmapNum;
        public int lightmapX, lightmapY;
        public int lightmapWidth, lightmapHeight;

        public Vector3 lightmapOrigin;
        public Vector3[] lightmapVecs = new Vector3[3];	// for patches, [0] and [1] are lodbounds

        public int patchWidth;
        public int patchHeight;

        public QSurface() { }

        public void LoadFromByteArray(byte[] buffer, int offset)
        {
            shaderNum = BitConverter.ToInt32(buffer, offset);
            fogNum = BitConverter.ToInt32(buffer, offset + 4);
            surfaceType = (QMapSurfaceType)BitConverter.ToInt32(buffer, offset + 8);

            firstVert = BitConverter.ToInt32(buffer, offset + 12);
            numVerts = BitConverter.ToInt32(buffer, offset + 16);

            firstIndex = BitConverter.ToInt32(buffer, offset + 20);
            numIndexes = BitConverter.ToInt32(buffer, offset + 24);

            lightmapNum = BitConverter.ToInt32(buffer, offset + 28);
            lightmapX = BitConverter.ToInt32(buffer, offset + 32);
            lightmapY = BitConverter.ToInt32(buffer, offset + 36);
            lightmapWidth = BitConverter.ToInt32(buffer, offset + 40);
            lightmapHeight = BitConverter.ToInt32(buffer, offset + 44);

            lightmapOrigin = new Vector3(BitConverter.ToSingle(buffer, offset + 48),
                BitConverter.ToSingle(buffer, offset + 52),
                BitConverter.ToSingle(buffer, offset + 56));

            lightmapVecs[0] = new Vector3(BitConverter.ToSingle(buffer, offset + 60),
                BitConverter.ToSingle(buffer, offset + 64),
                BitConverter.ToSingle(buffer, offset + 68));

            lightmapVecs[1] = new Vector3(BitConverter.ToSingle(buffer, offset + 72),
                BitConverter.ToSingle(buffer, offset + 76),
                BitConverter.ToSingle(buffer, offset + 80));

            lightmapVecs[2] = new Vector3(BitConverter.ToSingle(buffer, offset + 84),
                BitConverter.ToSingle(buffer, offset + 88),
                BitConverter.ToSingle(buffer, offset + 92));

            patchWidth = BitConverter.ToInt32(buffer, offset + 96);
            patchHeight = BitConverter.ToInt32(buffer, offset + 100);
        }
    }


    internal class Header
    {
        public const int CANT_LUMPS = 17;
        public int ident;
        public int version;
        public Lump[] lumps = new Lump[CANT_LUMPS];

        public Header() { }
    }

}
