using System;
using System.Timers;

using Android.Widget;

namespace Vibe.Utility
{
    internal sealed class SeekBarTracker : IDisposable
    {
        public SeekBarTracker(SeekBar seekBar, int updateInterval = 100)
        {
            this.seekBar = seekBar;
            seekBar.StartTrackingTouch += this.OnSeekBarStartTrackingTouch;
            seekBar.StopTrackingTouch += this.OnSeekBarStopTrackingTouch;

            this.seekTimer = new(updateInterval)
            {
                AutoReset = true,
            };
            this.seekTimer.Elapsed += this.OnSeekTimerElapsed;
            this.seekTimer.Start();
        }

        private readonly SeekBar seekBar;

        private bool updateSeekBar = true;

        private int seekStart;

        private int seekStop;

        private readonly Timer seekTimer;

        public event EventHandler<SeekEventArgs>? Seek;

        public event EventHandler<UpdateEventArgs>? Update;

        public void Dispose()
        {
            this.seekBar.StartTrackingTouch -= this.OnSeekBarStartTrackingTouch;
            this.seekBar.StopTrackingTouch -= this.OnSeekBarStopTrackingTouch;

            this.seekTimer.Elapsed -= this.OnSeekTimerElapsed;
            this.seekTimer.Stop();
            this.seekTimer.Dispose();
        }

        private void OnSeekBarStartTrackingTouch(object source, SeekBar.StartTrackingTouchEventArgs eventArgs)
        {
            this.seekStart = this.seekBar.Progress;
            this.updateSeekBar = false;
        }

        private void OnSeekBarStopTrackingTouch(object source, SeekBar.StopTrackingTouchEventArgs eventArgs)
        {
            this.seekStop = this.seekBar.Progress;
            this.updateSeekBar = true;
            
            this.Seek?.Invoke(this, new(this.seekStart, this.seekStop));
        }

        private void OnSeekTimerElapsed(object source, ElapsedEventArgs eventArgs)
        {
            this.Update?.Invoke(this, new(!this.updateSeekBar));
        }

        public sealed class SeekEventArgs : EventArgs
        {
            public SeekEventArgs(int start, int stop)
            {
                this.Start = start;
                this.Stop = stop;
            }

            public int Start
            {
                get;
            }

            public int Stop
            {
                get;
            }
        }
        
        public sealed class UpdateEventArgs : EventArgs
        {
            public UpdateEventArgs(bool isTouched)
            {
                this.IsTouched = isTouched;
            }
            
            public bool IsTouched
            {
                get;
            }
        }
    }
}