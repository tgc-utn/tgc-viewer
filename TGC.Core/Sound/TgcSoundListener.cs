using SharpDX.X3DAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Core.Sound
{
    public class TgcSoundListener
    {
        private Listener listener;
        private ITransformObject receptor;

        public TgcSoundListener(ITransformObject receptor)
        {
            this.receptor = receptor;
            listener = new Listener();
            Update();
        }

        internal Listener Listener { get { return listener; } }
        
        public TGCVector3 Position { get { return receptor.Position; } }

        internal void Update()
        {
            TGCVector3 scaled = receptor.Position;
            listener.Position = scaled.ToRawVector;
        }

        public void SetOrientation(TGCVector3 front, TGCVector3 top)
        {
            listener.OrientFront = front.ToRawVector;
            listener.OrientTop = top.ToRawVector;
            TGCVector3 p = front;
            p.Normalize();
            p.Scale(250f);
            listener.Velocity = p.ToRawVector; 
        }

    }
}
