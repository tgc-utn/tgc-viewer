using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TGC.Core.Sound.TgcSoundManager;

namespace TGC.Core.Sound
{
    /// <summary>
    /// Estado del sonido
    /// </summary>
    internal abstract class TgcSoundState
    {
        protected TgcSound sound;

        protected TgcSoundState(TgcSound sound)
        {
            this.sound = sound;
        }

        public virtual void Play()
        {
            this.Switch(new TgcSoundStatePlaying(sound));
        }

        public virtual void Pause()
        {
            this.Switch(new TgcSoundStatePaused(sound));
        }

        public virtual void Stop()
        {
            this.Switch(new TgcSoundStateStopped(sound));
        }

        protected void Switch(TgcSoundState state)
        {
            this.sound.SwitchState(state);
        }

        public virtual void Dispose()
        {            
            this.Switch(new TgcSoundStateDisposed(this.sound));
        }
    }
}
