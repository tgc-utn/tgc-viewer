using System;
using System.Runtime.InteropServices;

namespace TgcViewer.Utils
{
    /// <summary>
    /// High resolution game timer.
    /// </summary>
    public class HighResolutionTimer
    {
        /// <summary>
        /// The current system ticks (count).
        /// </summary>
        /// <param name="lpPerformanceCount">Current performance count of the system.</param>
        /// <returns>False on failure.</returns>
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        /// <summary>
        /// Ticks per second (frequency) that the high performance counter performs.
        /// </summary>
        /// <param name="lpFrequency">Frequency the higher performance counter performs.</param>
        /// <returns>False if the high performance counter is not supported.</returns>
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        // Static Members
        private static readonly HighResolutionTimer _timer = new HighResolutionTimer();

        // Members
        private long _startTime;
        private float _frameTime;

        private int _fps;
        private int _frameCount;
        private float _frameSecond;

        // Constructor
        /// <summary>
        /// Cannot instantiate the timer directly.
        /// </summary>
        private HighResolutionTimer() { }

        // Timer Methods
        /// <summary>
        /// Resets the timer for a new game.
        /// </summary>
        public void Reset()
        {
            // Time needs to be initialized to current system count.
            _startTime = HighResolutionTimer.Ticks;
        }

        /// <summary>
        /// Calculates frame time since last call to Set.
        /// </summary>
        public void Set()
        {
            // Calc frame time.
            long endTime = HighResolutionTimer.Ticks;
            _frameTime = (float)(endTime - _startTime) / (float)HighResolutionTimer.Frequency;
            _startTime = endTime;

            // Calc fps.
            _frameCount++;
            _frameSecond += _frameTime;
            if (_frameSecond >= 1.0F)
            {
                _frameSecond = 0;
                _fps = _frameCount;
                _frameCount = 0;
            }
        }

        // Static Properties
        /// <summary>
        /// Gets the timer instance.
        /// </summary>
        static public HighResolutionTimer Instance
        {
            get { return _timer; }
        }

        /// <summary>
        /// Gets the frequency that all timers performs at.
        /// </summary>
        static public long Frequency
        {
            get
            {
                long freq = 0;
                HighResolutionTimer.QueryPerformanceFrequency(out freq);

                return freq;
            }
        }

        /// <summary>
        /// Gets the current system ticks.
        /// </summary>
        static public long Ticks
        {
            get
            {
                long ticks = 0;
                HighResolutionTimer.QueryPerformanceCounter(out ticks);

                return ticks;
            }
        }

        // Properties

        /// <summary>
        /// Gets the time recorded between frames.
        /// </summary>
        public float FrameTime
        {
            get
            { return _frameTime; }
        }

        /// <summary>
        /// Gets the frames per second based on the time between frames.
        /// </summary>
        public float FramesPerSecond
        {
            get { return _fps; }
        }

    }
}