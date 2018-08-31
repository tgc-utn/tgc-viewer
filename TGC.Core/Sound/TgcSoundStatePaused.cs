using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Core.Sound
{
    internal class TgcSoundStatePaused : TgcSoundState
    {
        public TgcSoundStatePaused(TgcSound sound) : base(sound)
        {
            sound.Voice.Stop();
        }

        public override void Pause() { }
    }
}
