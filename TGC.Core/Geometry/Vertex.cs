using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;

namespace TGC.Core.Geometry
{
    public class Vertex
    {
        public static readonly VertexElement[] PositionColoredTexturedNormal_VertexElements =
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

        public static VertexDeclaration PositionColoredTexturedNormal_Declaration =
            new VertexDeclaration(D3DDevice.Instance.Device, PositionColoredTexturedNormal_VertexElements);

        public struct PositionColoredTexturedNormal
        {
            public float X;
            public float Y;
            public float Z;
            public int Color;
            public float Tu;
            public float Tv;
            public float NX;
            public float NY;
            public float NZ;

            public PositionColoredTexturedNormal(TGCVector3 pos, int color, float u, float v, TGCVector3 normal)
                : this(pos.X, pos.Y, pos.Z, color, u, v, normal.X, normal.Y, normal.Z)
            {
            }

            public PositionColoredTexturedNormal(float X, float Y, float Z, int color, float Tu1, float Tv1,
                float NX,
                float NY, float NZ)
            {
                this.X = X;
                this.Y = Y;
                this.Z = Z;
                Color = color;
                Tu = Tu1;
                Tv = Tv1;
                this.NX = NX;
                this.NY = NY;
                this.NZ = NZ;
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
