using TGC.Core.Mathematica;
using TGC.Core.Utils;

namespace TGC.Core.Geometry
{
    /// <summary>
    ///     Representa un Ray 3D con un origen y una direccion, de la forma: r = p + td
    /// </summary>
    public class TgcRay
    {
        private TGCVector3 direction;

        public TgcRay()
        {
        }

        /// <summary>
        ///     Se normaliza la direccion
        /// </summary>
        public TgcRay(TGCVector3 origin, TGCVector3 direction)
        {
            Origin = origin;
            this.direction = direction;
            this.direction.Normalize();
        }

        /// <summary>
        ///     Punto de origen del Ray
        /// </summary>
        public TGCVector3 Origin { get; set; }

        /// <summary>
        ///     Direccion del Ray
        /// </summary>
        public TGCVector3 Direction
        {
            get { return direction; }
            set
            {
                direction = value;
                direction.Normalize();
            }
        }

        public override string ToString()
        {
            return "Origin[" + TgcParserUtils.printFloat(Origin.X) + ", " + TgcParserUtils.printFloat(Origin.Y) + ", " +
                   TgcParserUtils.printFloat(Origin.Z) + "]" +
                   " Direction[" + TgcParserUtils.printFloat(direction.X) + ", " +
                   TgcParserUtils.printFloat(direction.Y) + ", " + TgcParserUtils.printFloat(direction.Z) + "]";
        }

        /// <summary>
        ///     Convertir a Struct
        /// </summary>
        public RayStruct toStruct()
        {
            var rayStruct = new RayStruct();
            rayStruct.origin = Origin;
            rayStruct.direction = direction;
            return rayStruct;
        }

        /// <summary>
        ///     Ray en un struct liviano
        /// </summary>
        public struct RayStruct
        {
            public TGCVector3 origin;
            public TGCVector3 direction;

            /// <summary>
            ///     Convertir a clase
            /// </summary>
            public TgcRay toClass()
            {
                return new TgcRay(origin, direction);
            }
        }
    }
}