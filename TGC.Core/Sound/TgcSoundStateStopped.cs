using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Core.Sound
{
    internal class TgcSoundStateStopped : TgcSoundState
    {
        public TgcSoundStateStopped(TgcSound sound) : base(sound)
        {
            sound.Voice.Stop();
            sound.Voice.FlushSourceBuffers();
            sound.Voice.SubmitSourceBuffer(sound.Buffer, null);
        }

        public override void Stop() { }
    }
}
