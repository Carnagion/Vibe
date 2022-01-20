using Android.App;
using Android.Content;
using Android.Support.V4.App;

using Vibe.Music;

namespace Vibe.Interface.Services
{
    [Service]
    internal sealed class NowPlayingService : ForegroundService
    {
        private const int notificationChannelId = 9;
        
        private const string pauseAction = "pauseAction";

        private const string skipNextAction = "skipNextAction";

        private const string skipPreviousAction = "skipPreviousAction";

        private NotificationChannel nowPlayingChannel = null!;

        public override int Id
        {
            get
            {
                return 9;
            }
        }

        public override void OnCreate()
        {
            base.OnCreate();
            
            this.nowPlayingChannel = new(NowPlayingService.notificationChannelId.ToString(), this.Resources?.GetString(Resource.String.notification_channel_nowplaying_title), NotificationImportance.Low)
            {
                Description = this.Resources?.GetString(Resource.String.notification_channel_nowplaying_description),
            };
            this.NotificationManager.CreateNotificationChannel(this.nowPlayingChannel);
            
            Playback.MediaPlayerStateChanged += this.OnPlaybackMediaPlayerStateChanged;
        }

        public override void OnDestroy()
        {
            Playback.MediaPlayerStateChanged -= this.OnPlaybackMediaPlayerStateChanged;
            base.OnDestroy();
        }

        protected override void HandleCommand(Intent? intent, StartCommandFlags flags, int id)
        {
            base.HandleCommand(intent, flags, id);
            
            switch (intent?.Action)
            {
                case NowPlayingService.pauseAction:
                    switch (Playback.PlayingState)
                    {
                        case Playback.MediaPlayerState.Started:
                            Playback.Pause();
                            break;
                        case Playback.MediaPlayerState.Paused:
                            Playback.Start();
                            break;
                    }
                    break;
                case NowPlayingService.skipNextAction:
                    Playback.SkipNext();
                    break;
                case NowPlayingService.skipPreviousAction:
                    Playback.SkipPrevious();
                    break;
            }
        }

        protected override Notification BuildNotification()
        {
            return Playback.NowPlaying is null
                ? new NotificationCompat.Builder(this, this.nowPlayingChannel.Id)
                    .SetSmallIcon(Resource.Drawable.icon_notification_notplaying)
                    .SetContentTitle(this.Resources?.GetString(Resource.String.notification_channel_nowplaying_title))
                    .SetContentText(this.Resources?.GetString(Resource.String.notification_nowplaying_description))
                    .SetOngoing(true)
                    .Build()
                : new NotificationCompat.Builder(this, this.nowPlayingChannel.Id)
                    .SetSmallIcon(Resource.Drawable.icon_notification_nowplaying)
                    .SetContentTitle(Playback.NowPlaying.Title)
                    .SetContentText($"{Playback.NowPlaying.Album.Title} · {Playback.NowPlaying.Artist.Name}")
                    .AddAction(this.BuildPreviousAction())
                    .AddAction(this.BuildPauseAction())
                    .AddAction(this.BuildNextAction())
                    .SetOngoing(true)
                    .Build();
        }

        private NotificationCompat.Action BuildPauseAction()
        {
            Intent pause = new(this, typeof(NowPlayingService));
            pause.SetAction(NowPlayingService.pauseAction);

            PendingIntent? pending = PendingIntent.GetService(this, 0, pause, 0);

            int icon = Playback.PlayingState is Playback.MediaPlayerState.Started ? Resource.Drawable.icon_notification_pause : Resource.Drawable.icon_notification_play;
            string? title = Playback.PlayingState is Playback.MediaPlayerState.Started ? this.Resources?.GetString(Resource.String.notification_nowplaying_pause) : this.Resources?.GetString(Resource.String.notification_nowplaying_play);
            return new NotificationCompat.Action.Builder(icon, title, pending).Build();
        }

        private NotificationCompat.Action BuildNextAction()
        {
            Intent next = new(this, typeof(NowPlayingService));
            next.SetAction(NowPlayingService.skipNextAction);

            PendingIntent? pending = PendingIntent.GetService(this, 0, next, 0);
            
            return new NotificationCompat.Action.Builder(Resource.Drawable.icon_nowplaying_skipnext, this.Resources?.GetString(Resource.String.notification_nowplaying_next), pending).Build();
        }

        private NotificationCompat.Action BuildPreviousAction()
        {
            Intent previous = new(this, typeof(NowPlayingService));
            previous.SetAction(NowPlayingService.skipPreviousAction);

            PendingIntent? pending = PendingIntent.GetService(this, 0, previous, 0);
            
            return new NotificationCompat.Action.Builder(Resource.Drawable.icon_nowplaying_skipnext, this.Resources?.GetString(Resource.String.notification_nowplaying_previous), pending).Build();
        }

        private void OnPlaybackMediaPlayerStateChanged(object source, Playback.MediaPlayerStateChangeArgs eventArgs)
        {
            if (eventArgs.ChangedTo is Playback.MediaPlayerState.End)
            {
                this.StopForeground(true);
                this.StopSelf();
            }
            else
            {
                this.NotificationManager.Notify(NowPlayingService.notificationChannelId, this.BuildNotification());
            }
        }
    }
}