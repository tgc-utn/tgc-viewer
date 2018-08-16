using SharpDX;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Core.Sound
{
    public class TgcSoundEmitter
    {
        private ITransformObject origin;
        private Emitter emitter;
        private TgcSound sound;

        public TgcSoundEmitter(string path, ITransformObject origin)
        {
            sound = new TgcSound(path);
            sound.LoopCount = TgcSound.LOOP_INFINITE;
            this.origin = origin;
            emitter = new Emitter();
            emitter.ChannelCount = 1;
            emitter.CurveDistanceScaler = float.MinValue;
            this.UpdatePosition();
            this.sound.Play();
        }

        private void UpdatePosition()
        {
            TGCVector3 scaled = this.origin.Position;
            emitter.Position = scaled.ToRawVector;
            
        }

        float el = 0;
        public void Update(float elapsedTime, TgcSoundListener listener)
        {
            //listener.Update();
            //UpdatePosition();

            var emi = new Emitter
            {
                ChannelCount = 1,
                CurveDistanceScaler = float.MinValue,
                OrientFront = new SharpDX.Mathematics.Interop.RawVector3(1, 0, 0),
                OrientTop = new SharpDX.Mathematics.Interop.RawVector3(0, 1, 0),
                Position = new SharpDX.Mathematics.Interop.RawVector3(-10, 0, 0),
                Velocity = new SharpDX.Mathematics.Interop.RawVector3(0, 0, 0)
            };

            var list = new Listener
            {
                OrientFront = new SharpDX.Mathematics.Interop.RawVector3(0, 0, 1),
                OrientTop = new SharpDX.Mathematics.Interop.RawVector3(0, 1, 0),
                Position = new SharpDX.Mathematics.Interop.RawVector3(0, 0, 0),
                Velocity = new SharpDX.Mathematics.Interop.RawVector3(0, 0, 0)
            };


            Console.WriteLine("Play a sound rotating around the listener");
            // Rotates the emitter
            /*el += elapsedTime * 100;
            var rotateEmitter = TGCMatrix.RotationY(el / 5.0f);
            var newPosition = TGCVector3.Transform(new TGCVector3(0, 0, 1000), rotateEmitter);
            var newPositionVector3 = new TGCVector3(newPosition.X, newPosition.Y, newPosition.Z);
            emi.Velocity = ((newPositionVector3 - new TGCVector3(emi.Position.X, emi.Position.Y, emi.Position.Z)) * 20).ToRawVector;
            emi.Position = newPositionVector3.ToRawVector;*/

            // Calculate X3DAudio settings
            var dspSettings = TgcSoundManager.Audio3D().Calculate(list, emi, CalculateFlags.Matrix | CalculateFlags.Doppler, 1, 2);
            // Modify XAudio2 source voice settings
            this.sound.Voice.SetOutputMatrix(1, 2, dspSettings.MatrixCoefficients);
            this.sound.Voice.SetFrequencyRatio(dspSettings.DopplerFactor);

            /*
            //DspSettings settings = new DspSettings(1, 2);
            var settings = TgcSoundManager.Audio3D().Calculate(listener.Listener, emitter, CalculateFlags.Matrix | CalculateFlags.Doppler, 1, 2);
            //var settings = TgcSoundManager.Audio3D().Calculate(listener.Listener, emitter, CalculateFlags.Matrix | CalculateFlags.Doppler | CalculateFlags.LpfDirect | CalculateFlags.Reverb, 1, 2);

            sound.Process3D(settings);
            */
        }

        public void Dispose()
        {
            sound.Dispose();
        }

    }
}
