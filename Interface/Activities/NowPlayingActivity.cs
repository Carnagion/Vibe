using System;
using System.Timers;

using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

using Vibe.Music;

namespace Vibe.Interface.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.Vibe", MainLauncher = false, NoHistory = false)]
    internal sealed class NowPlayingActivity : AppCompatActivity
    {
        private TextView trackTitle = null!;

        private TextView trackInfo = null!;

        private ImageView trackImage = null!;

        private ImageButton pauseButton = null!;

        private ImageButton skipNext = null!;

        private ImageButton skipPrevious = null!;

        private ImageButton backButton = null!;

        private SeekBar seekBar = null!;

        private Timer seekTimer = null!;

        private bool moveSeekBar = true;

        private TextView currentDuration = null!;

        private TextView maxDuration = null!;
        
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            this.SetContentView(Resource.Layout.activity_nowplaying);
            this.Window?.SetFlags(WindowManagerFlags.BlurBehind, WindowManagerFlags.BlurBehind);
            
            this.trackTitle = this.FindViewById<TextView>(Resource.Id.activity_nowplaying_tracktitle)!;
            this.trackInfo = this.FindViewById<TextView>(Resource.Id.activity_nowplaying_trackinfo)!;
            this.trackImage = this.FindViewById<ImageView>(Resource.Id.activity_nowplaying_trackimage)!;
            
            this.pauseButton = this.FindViewById<ImageButton>(Resource.Id.activity_nowplaying_pause)!;
            this.pauseButton.Click += this.OnPauseButtonClick;
            
            this.skipNext = this.FindViewById<ImageButton>(Resource.Id.activity_nowplaying_skipnext)!;
            this.skipNext.Click += this.OnSkipNextClick;
            
            this.skipPrevious = this.FindViewById<ImageButton>(Resource.Id.activity_nowplaying_skipprevious)!;
            this.skipPrevious.Click += this.OnSkipPreviousClick;
            
            this.seekBar = this.FindViewById<SeekBar>(Resource.Id.activity_nowplaying_seekbar)!;
            this.seekBar.StartTrackingTouch += this.OnSeekBarStartTrackingTouch;
            this.seekBar.StopTrackingTouch += this.OnSeekBarStopTrackingTouch;
            
            this.currentDuration = this.FindViewById<TextView>(Resource.Id.activity_nowplaying_duration_current)!;
            this.maxDuration = this.FindViewById<TextView>(Resource.Id.activity_nowplaying_duration_max)!;
            
            this.seekTimer = new(100)
            {
                AutoReset = true,
            };
            this.seekTimer.Elapsed += this.OnSeekTimerElapsed;
            this.seekTimer.Start();
            
            Playback.MediaPlayerStateChanged += this.OnPlaybackMediaPlayerStateChanged;
            this.OnPlaybackMediaPlayerStateChanged(null!, new(Playback.PlayingState, Playback.PlayingState));
            
            this.backButton = this.FindViewById<ImageButton>(Resource.Id.activity_nowplaying_back)!;
            this.backButton.Click += this.OnBackButtonClicked;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            this.backButton.Click -= this.OnBackButtonClicked;
            
            Playback.MediaPlayerStateChanged -= this.OnPlaybackMediaPlayerStateChanged;
            
            this.seekTimer.Elapsed -= this.OnSeekTimerElapsed;
            this.seekTimer.Dispose();
            
            this.seekBar.StartTrackingTouch -= this.OnSeekBarStartTrackingTouch;
            this.seekBar.StopTrackingTouch -= this.OnSeekBarStopTrackingTouch;
            
            this.pauseButton.Click -= this.OnPauseButtonClick;
            this.skipNext.Click -= this.OnSkipNextClick;
            this.skipPrevious.Click -= this.OnSkipPreviousClick;
        }

        private void OnPlaybackMediaPlayerStateChanged(object source, Playback.MediaPlayerStateChangeArgs eventArgs)
        {
            if (Playback.NowPlaying is null)
            {
                return;
            }
            
            this.trackTitle.Text = Playback.NowPlaying.Title;
            this.trackInfo.Text = $"{Playback.NowPlaying.Album.Title} · {Playback.NowPlaying.Artist.Name}";
            this.trackImage.SetImageBitmap(Playback.NowPlaying.Album.Artwork);
            
            switch (eventArgs.ChangedTo)
            {
                case Playback.MediaPlayerState.Started:
                    this.pauseButton.SetImageResource(Resource.Drawable.icon_nowplaying_pause);
                    break;
                case Playback.MediaPlayerState.Paused:
                    this.pauseButton.SetImageResource(Resource.Drawable.icon_nowplaying_play);
                    break;
            }
            
            this.seekBar.Max = (int)Playback.NowPlaying.Duration * 1000;

            TimeSpan max = TimeSpan.FromSeconds(Playback.NowPlaying.Duration);
            this.maxDuration.Text = $"{max.Minutes}:{max.Seconds:D2}";
            this.currentDuration.Text ??= "0:00";
        }

        private void OnPauseButtonClick(object source, EventArgs eventArgs)
        {
            switch (Playback.PlayingState)
            {
                case Playback.MediaPlayerState.Started:
                    Playback.Pause();
                    break;
                case Playback.MediaPlayerState.Paused:
                    Playback.Start();
                    break;
            }
        }

        private void OnSkipNextClick(object source, EventArgs eventArgs)
        {
            Playback.SkipNext();
        }

        private void OnSkipPreviousClick(object source, EventArgs eventArgs)
        {
            Playback.SkipPrevious();
        }

        private void OnSeekTimerElapsed(object source, ElapsedEventArgs eventArgs)
        {
            int position = (int)Playback.CurrentPosition;
            this.RunOnUiThread(() =>
            {
                if (this.moveSeekBar)
                {
                    this.seekBar.Progress = position;
                }
            
                TimeSpan current = TimeSpan.FromMilliseconds(position);
                this.currentDuration.Text = $"{current.Minutes}:{current.Seconds:D2}";
            });
        }

        private void OnSeekBarStartTrackingTouch(object source, SeekBar.StartTrackingTouchEventArgs eventArgs)
        {
            this.moveSeekBar = false;
        }

        private void OnSeekBarStopTrackingTouch(object source, SeekBar.StopTrackingTouchEventArgs eventArgs)
        {
            Playback.CurrentPosition = (uint)this.seekBar.Progress;
            this.moveSeekBar = true;
        }

        private void OnBackButtonClicked(object source, EventArgs eventArgs)
        {
            this.OnBackPressed();
        }
    }
}