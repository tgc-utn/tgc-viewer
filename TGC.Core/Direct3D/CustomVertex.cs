using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D9;

namespace TGC.Core.Direct3D
{
    //TODO: Need to decide if it's better to use clases with destructors or all structs.
    class CustomVertex
    {
        /**
         * Position + Color vertex
         */
        public struct PositionColored
        {
            public Vector3 Position { get;  set;}
            public Color Color { get; set; }
            /*
            public PositionColored()
            {
            }
            */
            public PositionColored(Vector3 position, Color color)
            {
                Position = position;
                Color = color;
            }
        };

        /**
        * Position + Texcoord vertex
        */
        public struct PositionTextured
        {
            public Vector3 Position { get; set; }
            public Vector2 Texcoord { get; set; }
            /*
            public PositionTextured()
            {
            }
            */
            public PositionTextured(Vector3 position, Vector2 texcoord)
            {
                Position = position;
                Texcoord = texcoord;
            }
        };

        /**
        * Position + Color + Texcoord vertex
        */
        public struct PositionColoredTextured
        {
            public Vector3 Position { get; set; }
            public Color Color { get; set; }
            public Vector2 Texcoord { get; set; }
            /*
            public PositionColoredTextured()
            {
            }
            */
            public PositionColoredTextured(Vector3 position, Color color, Vector2 texcoord)
            {
                Position = position;
                Color = color;
                Texcoord = texcoord;
            }
        };

    }
}