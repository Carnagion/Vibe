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
        
        private const string pauseAction = "pause";

        private const string skipNextAction = "next";

        private const string skipPreviousAction = "previous";

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
            
            this.SetupNotificationChannel();
            
            Playback.MediaPlayerStateChanged += this.OnPlaybackMediaPlayerStateChanged;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Playback.MediaPlayerStateChanged -= this.OnPlaybackMediaPlayerStateChanged;
        }

        // Handle notification actions
        protected override void HandleCommand(Intent? intent, StartCommandFlags flags, int id)
        {
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

        // Return a notification based on the playback state
        protected override Notification BuildNotification()
        {
            return Playback.NowPlaying is null
                ? new NotificationCompat.Builder(this, this.nowPlayingChannel.Id)
                    .SetSmallIcon(Resource.Drawable.music_off)
                    .SetContentTitle(this.Resources?.GetString(Resource.String.notification_not_title))
                    .SetContentText(this.Resources?.GetString(Resource.String.notification_not_description))
                    .SetOngoing(true)
                    .Build()
                : new NotificationCompat.Builder(this, this.nowPlayingChannel.Id)
                    .SetSmallIcon(Resource.Drawable.music_note)
                    .SetContentTitle(Playback.NowPlaying.Title)
                    .SetContentText($"{Playback.NowPlaying.Album.Title} · {Playback.NowPlaying.Artist.Name}")
                    .AddAction(this.BuildPreviousAction())
                    .AddAction(this.BuildPauseAction())
                    .AddAction(this.BuildNextAction())
                    .SetOngoing(true)
                    .Build();
        }

        // Setup the notification channel used to push notifications
        private void SetupNotificationChannel()
        {
            this.nowPlayingChannel = new(NowPlayingService.notificationChannelId.ToString(), this.Resources?.GetString(Resource.String.channel_now_title), NotificationImportance.Low)
            {
                Description = this.Resources?.GetString(Resource.String.channel_now_description),
            };
            this.NotificationManager.CreateNotificationChannel(this.nowPlayingChannel);
        }

        // Return an action for pausing/playing the track
        private NotificationCompat.Action BuildPauseAction()
        {
            Intent pause = new(this, typeof(NowPlayingService));
            pause.SetAction(NowPlayingService.pauseAction);

            PendingIntent? pending = PendingIntent.GetService(this, 0, pause, 0);

            int icon = Playback.PlayingState is Playback.MediaPlayerState.Started ? Resource.Drawable.pause : Resource.Drawable.play;
            string? title = Playback.PlayingState is Playback.MediaPlayerState.Started ? this.Resources?.GetString(Resource.String.notification_now_pause) : this.Resources?.GetString(Resource.String.notification_now_play);
            return new NotificationCompat.Action.Builder(icon, title, pending).Build();
        }

        // Return an action for skipping to the next track
        private NotificationCompat.Action BuildNextAction()
        {
            Intent next = new(this, typeof(NowPlayingService));
            next.SetAction(NowPlayingService.skipNextAction);

            PendingIntent? pending = PendingIntent.GetService(this, 0, next, 0);
            
            return new NotificationCompat.Action.Builder(Resource.Drawable.skip_next, this.Resources?.GetString(Resource.String.notification_now_next), pending).Build();
        }

        // Return an action for skipping to the previous track
        private NotificationCompat.Action BuildPreviousAction()
        {
            Intent previous = new(this, typeof(NowPlayingService));
            previous.SetAction(NowPlayingService.skipPreviousAction);

            PendingIntent? pending = PendingIntent.GetService(this, 0, previous, 0);
            
            return new NotificationCompat.Action.Builder(Resource.Drawable.skip_previous, this.Resources?.GetString(Resource.String.notification_now_previous), pending).Build();
        }

        // Change or remove the notification based on the playback state
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