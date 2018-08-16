using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Core.Sound
{
    public class TgcSoundStatePlaying : TgcSoundState
    {
        public TgcSoundStatePlaying(TgcSound sound) : base(sound)
        {
            sound.Voice.Start();
        }

        public override void Play()
        {
            Switch(new TgcSoundStateStopped(sound));
            Switch(new TgcSoundStatePlaying(sound));
        }

    }
}
