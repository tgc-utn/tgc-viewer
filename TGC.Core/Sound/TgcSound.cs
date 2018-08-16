using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;
using TGC.Core.Mathematica;
using static TGC.Core.Sound.TgcSoundManager;

namespace TGC.Core.Sound
{
    public class TgcSound
    {
        public const int LOOP_INFINITE = 255;

        private SourceVoice voice;
        private CustomAudioBuffer buffer;
        private TgcSoundState state;

        public TgcSound(string path)
        {
            buffer = TgcSoundManager.CreateSoundBuffer(path);
            this.voice = TgcSoundManager.CreateVoice(buffer);
            this.voice.SetVolume(1);
            this.LoopCount = LOOP_INFINITE;
            this.state = new TgcSoundStateStopped(this);
        }

        public void Process3D(DspSettings settings)
        {
            this.voice.SetOutputMatrix(1, 2, settings.MatrixCoefficients);
            this.voice.Stop();
            this.voice.Start();
            //FilterParameters filters = new FilterParameters();
            //filters.Type = FilterType.LowPassFilter;
            //filters.Frequency = 2.0f * FastMath.Sin(FastMath.PI / 6.0f * settings.LpfDirectCoefficient);
            //filters.OneOverQ = 1;
            //this.voice.SetFilterParameters(filters);
        }

        public void SwitchState(TgcSoundState newState)
        {
            this.state = newState;
        }

        public SourceVoice Voice { get { return this.voice; } }

        public CustomAudioBuffer Buffer { get { return this.buffer; } }

        public int LoopCount
        {
            get
            {
                return this.buffer.LoopCount;
            }
            set
            {
                this.buffer.LoopCount = value;
                this.state = new TgcSoundStateStopped(this);
            }
        }

        public void Play()
        {
            this.state.Play();
        }

        public void Pause()
        {
            this.state.Pause();
        }

        public void Stop()
        {
            this.state.Stop();
        }

        public void Dispose()
        {
            this.state.Dispose();
        }

    }
}
