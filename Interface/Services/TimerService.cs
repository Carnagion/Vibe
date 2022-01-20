using System;
using System.Timers;

using Android.App;
using Android.Content;

namespace Vibe.Interface.Services
{
    internal abstract class TimerService : ForegroundService
    {
        private Timer timer = null!;

        public static event EventHandler<ElapsedEventArgs>? Elapsed;
        
        public override void OnCreate()
        {
            base.OnCreate();

            this.timer = new(100)
            {
                AutoReset = true,
            };
            this.timer.Elapsed += this.OnTimerElapsed;
        }

        public override void OnDestroy()
        {
            this.timer.Elapsed -= this.OnTimerElapsed;
            this.timer.Stop();
            this.timer.Dispose();
            
            base.OnDestroy();
        }

        protected override void HandleCommand(Intent? intent, StartCommandFlags flags, int id)
        {
            this.timer.Start();
        }

        private void OnTimerElapsed(object source, ElapsedEventArgs eventArgs)
        {
            TimerService.Elapsed?.Invoke(source, eventArgs);
        }
    }
}