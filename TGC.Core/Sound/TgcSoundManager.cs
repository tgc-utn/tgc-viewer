using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SharpDX.X3DAudio;

namespace TGC.Core.Sound
{
    public static class TgcSoundManager
    {
        private static MasteringVoice masteringVoice;
        private static XAudio2 engine;
        private static X3DAudio audio3D;

        public static void Initialize()
        {
            engine = new XAudio2(XAudio2Version.Version27);
            masteringVoice = new MasteringVoice(engine);
            //masteringVoice.SetVolume(1);

            audio3D = new X3DAudio(Speakers.All);
            engine.StartEngine();
        }

        public static X3DAudio Audio3D()
        {
            return audio3D;
        }

        public static SourceVoice CreateVoice(CustomAudioBuffer buffer)
        {
            return new SourceVoice(engine, buffer.WaveFormat, true);
        }

        public static CustomAudioBuffer CreateSoundBuffer(string soundfile)
        {
            SoundStream stream = new SoundStream(File.OpenRead(soundfile));

            var buffer = new CustomAudioBuffer()
            {
                Stream = stream.ToDataStream(),
                AudioBytes = (int)stream.Length,
                Flags = BufferFlags.EndOfStream,
                WaveFormat = stream.Format,
                DecodedPacketsInfo = stream.DecodedPacketsInfo,
            };

            if (stream.Format is WaveFormatAdpcm)
            {
                var format = (WaveFormatAdpcm)stream.Format;
                int duration = ((int)stream.Length / stream.Format.BlockAlign) * format.SamplesPerBlock;
                int partial = (int)stream.Length % stream.Format.BlockAlign;
                if (partial != 0)
                {
                    if (partial >= (7 * format.Channels))
                        duration += (partial * 2 / format.Channels - 12);
                }
                buffer.PlayLength = duration;
                buffer.LoopLength = buffer.PlayLength;
            }
                
            return buffer;
        }

        public sealed class CustomAudioBuffer : AudioBuffer
        {
            public WaveFormat WaveFormat { get; set; }
            public uint[] DecodedPacketsInfo { get; set; }
        }
    }
}
