using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Core.Sound
{
    public class TgcSoundStateDisposed : TgcSoundState
    {
        public TgcSoundStateDisposed(TgcSound sound) : base(sound)
        {
            sound.Voice.DestroyVoice();
            sound.Voice.Dispose();
            sound.Buffer.Stream.Dispose();
        }

        public override void Play() { }

        public override void Pause() { }

        public override void Stop() { }

        public override void Dispose() { }
    }
}
