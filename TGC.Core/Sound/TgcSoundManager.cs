using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

namespace TGC.Core.Sound
{
    static class TgcSoundManager
    {
        private static MasteringVoice masteringVoice;
        private static XAudio2 engine;

        internal static void Initialize()
        {
            if(engine == null)
            {
                engine = new XAudio2(XAudio2Version.Version27);
                masteringVoice = new MasteringVoice(engine);
                engine.StartEngine();
            }
            Tgc3DSoundManager.Initialize();
        }
    
        internal static SourceVoice CreateVoice(CustomAudioBuffer buffer)
        {
            return new SourceVoice(engine, buffer.WaveFormat, true);
        }

        internal static void Update()
        {
            Tgc3DSoundManager.Update();
        }

        internal static CustomAudioBuffer CreateSoundBuffer(string soundfile)
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

        internal sealed class CustomAudioBuffer : AudioBuffer
        {
            public WaveFormat WaveFormat { get; set; }
            public uint[] DecodedPacketsInfo { get; set; }
        }
    }
}
