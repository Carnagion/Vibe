using Android.App;
using Android.Content;
using Android.OS;

namespace Vibe.Interface.Services
{
    internal abstract class ForegroundService : Service
    {
        public const string start = "start";
        
        private bool started;

        public abstract int Id
        {
            get;
        }

        public Notification Notification
        {
            get;
            private set;
        } = null!;

        protected NotificationManager NotificationManager
        {
            get;
            private set;
        } = null!;

        public override void OnCreate()
        {
            base.OnCreate();
            this.NotificationManager = (NotificationManager)this.GetSystemService(ForegroundService.NotificationService)!;
        }

        public sealed override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            if (intent?.Action is ForegroundService.start && !this.started)
            {
                this.Notification = this.BuildNotification();
                this.StartForeground(this.Id, this.Notification);
                this.started = true;
            }
            this.HandleCommand(intent, flags, startId);
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            this.NotificationManager.Cancel(this.Id);
            base.OnDestroy();
        }
        
        public override IBinder? OnBind(Intent? intent)
        {
            return null;
        }

        protected abstract Notification BuildNotification();

        protected virtual void HandleCommand(Intent? intent, StartCommandFlags flags, int id)
        {
        }
    }
}