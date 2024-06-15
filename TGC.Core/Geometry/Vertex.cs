using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;

namespace TGC.Core.Geometry
{
    public class Vertex
    {
        public static readonly VertexElement[] PositionColoredTexturedNormalVertexElements =
        {
            new VertexElement(0, 0, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.Position, 0),
            new VertexElement(0, 12, DeclarationType.Color,
                DeclarationMethod.Default,
                DeclarationUsage.Color, 0),
            new VertexElement(0, 16, DeclarationType.Float2,
                DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 0),
            new VertexElement(0, 24, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.Normal, 0),
            VertexElement.VertexDeclarationEnd
        };

        public static readonly VertexDeclaration PositionColoredTexturedNormalDeclaration =
            new VertexDeclaration(D3DDevice.Instance.Device, PositionColoredTexturedNormalVertexElements);

        public struct PositionColoredTexturedNormal
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
            public int Color { get; set; }
            public float Tu { get; set; }
            public float Tv { get; set; }
            public float NX { get; set; }
            public float NY { get; set; }
            public float NZ { get; set; }

            public PositionColoredTexturedNormal(TGCVector3 pos, int color, float u, float v, TGCVector3 normal)
                : this(pos.X, pos.Y, pos.Z, color, u, v, normal.X, normal.Y, normal.Z)
            {
            }

            public PositionColoredTexturedNormal(float x, float y, float z, int color, float tu1, float tv1, float nx,
                float ny, float nz)
            {
                X = x;
                Y = y;
                Z = z;
                Color = color;
                Tu = tu1;
                Tv = tv1;
                NX = nx;
                NY = ny;
                NZ = nz;
            }

            public TGCVector3 GetPosition()
            {
                return new TGCVector3(X, Y, Z);
            }

            public TGCVector3 GetNormal()
            {
                return new TGCVector3(NX, NY, NZ);
            }

            public static VertexFormats Format =>
                VertexFormats.PositionNormal | VertexFormats.Texture1 | VertexFormats.Diffuse;

            public override string ToString()
            {
                return GetPosition().ToString();
            }
        }
    }
}
