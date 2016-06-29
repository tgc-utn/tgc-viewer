using System.Runtime.InteropServices;

namespace TGC.Core
{
    /// <summary>
    ///     High resolution game timer.
    /// </summary>
    public class HighResolutionTimer
    {
        // Static Members
        private int _fps;

        private int _frameCount;
        private float _frameSecond;

        // Members
        private long _startTime;

        /// <summary>
        ///     Gets the frequency that all timers performs at.
        /// </summary>
        public static long Frequency
        {
            get
            {
                long freq = 0;
                QueryPerformanceFrequency(out freq);

                return freq;
            }
        }

        /// <summary>
        ///     Gets the current system ticks.
        /// </summary>
        public static long Ticks
        {
            get
            {
                long ticks = 0;
                QueryPerformanceCounter(out ticks);

                return ticks;
            }
        }

        // Properties

        /// <summary>
        ///     Gets the time recorded between frames.
        /// </summary>
        public float FrameTime { get; private set; }

        /// <summary>
        ///     Gets the frames per second based on the time between frames.
        /// </summary>
        public float FramesPerSecond
        {
            get { return _fps; }
        }

        /// <summary>
        ///     The current system ticks (count).
        /// </summary>
        /// <param name="lpPerformanceCount">Current performance count of the system.</param>
        /// <returns>False on failure.</returns>
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        /// <summary>
        ///     Ticks per second (frequency) that the high performance counter performs.
        /// </summary>
        /// <param name="lpFrequency">Frequency the higher performance counter performs.</param>
        /// <returns>False if the high performance counter is not supported.</returns>
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        // Timer Methods
        /// <summary>
        ///     Resets the timer for a new game.
        /// </summary>
        public void Reset()
        {
            // Time needs to be initialized to current system count.
            _startTime = Ticks;
        }

        /// <summary>
        ///     Calculates frame time since last call to Set.
        /// </summary>
        public void Set()
        {
            // Calc frame time.
            var endTime = Ticks;
            FrameTime = (endTime - _startTime) / (float)Frequency;
            _startTime = endTime;

            // Calc fps.
            _frameCount++;
            _frameSecond += FrameTime;
            if (_frameSecond >= 1.0F)
            {
                _frameSecond = 0;
                _fps = _frameCount;
                _frameCount = 0;
            }
        }
    }
}