using System;

using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

using Vibe.Music;
using Vibe.Utility;

using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Vibe.Interface.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.Vibe", MainLauncher = false, NoHistory = false)]
    internal sealed class NowPlayingActivity : AppCompatActivity
    {
        private ImageView trackImage = null!;

        private TextView trackTitle = null!;

        private TextView trackArtist = null!;

        private TextView trackAlbum = null!;

        private ImageButton pauseButton = null!;

        private ImageButton nextButton = null!;

        private ImageButton previousButton = null!;

        private TextView currentDuration = null!;

        private TextView maxDuration = null!;

        private SeekBar seekBar = null!;

        private SeekBarTracker seekBarTracker = null!;
        
        // Make back button go to previous activity when clicked
        public override bool OnSupportNavigateUp()
        {
            this.OnBackPressed();
            return true;
        }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            this.SetContentView(Resource.Layout.activity_now);
            
            this.SetupActionBar();
            this.SetupViews();
            
            Playback.MediaPlayerStateChanged += this.OnPlaybackMediaPlayerStateChanged;
            this.OnPlaybackMediaPlayerStateChanged(null, new(Playback.PlayingState, Playback.PlayingState));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            Playback.MediaPlayerStateChanged -= this.OnPlaybackMediaPlayerStateChanged;
            
            this.seekBarTracker.Seek -= this.OnSeekBarTrackerSeek;
            this.seekBarTracker.Update -= this.OnSeekBarTrackerUpdate;
            this.seekBarTracker.Dispose();
            
            this.pauseButton.Click -= this.OnPauseButtonClick;
            this.nextButton.Click -= this.OnNextButtonClick;
            this.previousButton.Click -= this.OnPreviousButtonClick;
        }

        // Setup the toolbar
        private void SetupActionBar()
        {
            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.activity_now_toolbar));
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            this.SupportActionBar.SetDisplayShowTitleEnabled(false);
            this.SupportActionBar.SetHomeButtonEnabled(true);
        }

        // Setup all images, buttons, seek bars, etc. within the layout
        private void SetupViews()
        {
            this.trackImage = this.FindViewById<ImageView>(Resource.Id.activity_now_image)!;
            this.trackTitle = this.FindViewById<TextView>(Resource.Id.activity_now_title)!;
            this.trackArtist = this.FindViewById<TextView>(Resource.Id.activity_now_artist)!;
            this.trackAlbum = this.FindViewById<TextView>(Resource.Id.activity_now_album)!;

            this.pauseButton = this.FindViewById<ImageButton>(Resource.Id.activity_now_pause)!;
            this.pauseButton.Click += this.OnPauseButtonClick;

            this.nextButton = this.FindViewById<ImageButton>(Resource.Id.activity_now_next)!;
            this.nextButton.Click += this.OnNextButtonClick;

            this.previousButton = this.FindViewById<ImageButton>(Resource.Id.activity_now_previous)!;
            this.previousButton.Click += this.OnPreviousButtonClick;

            this.seekBar = this.FindViewById<SeekBar>(Resource.Id.activity_now_seek)!;
            this.seekBarTracker = new(this.seekBar);
            this.seekBarTracker.Seek += this.OnSeekBarTrackerSeek;
            this.seekBarTracker.Update += this.OnSeekBarTrackerUpdate;
            
            this.currentDuration = this.FindViewById<TextView>(Resource.Id.activity_now_duration_current)!;
            this.maxDuration = this.FindViewById<TextView>(Resource.Id.activity_now_duration_max)!;
        }

        // Change images and text values depending on the current playback state
        private void OnPlaybackMediaPlayerStateChanged(object? source, Playback.MediaPlayerStateChangeArgs eventArgs)
        {
            if (Playback.NowPlaying is null)
            {
                return;
            }
            
            this.trackImage.SetImageBitmap(Playback.NowPlaying.Album.Artwork);
            this.trackTitle.Text = Playback.NowPlaying.Title;
            this.trackArtist.Text = $"by {Playback.NowPlaying.Artist.Name}";
            this.trackAlbum.Text = $"on {Playback.NowPlaying.Album.Title}";
            
            switch (eventArgs.ChangedTo)
            {
                case Playback.MediaPlayerState.Started:
                    this.pauseButton.SetImageResource(Resource.Drawable.pause_circle);
                    break;
                case Playback.MediaPlayerState.Paused:
                    this.pauseButton.SetImageResource(Resource.Drawable.play_circle);
                    break;
            }
            
            this.seekBar.Max = (int)Playback.NowPlaying.Duration * 1000;

            TimeSpan max = TimeSpan.FromSeconds(Playback.NowPlaying.Duration);
            this.maxDuration.Text = $"{max.Minutes}:{max.Seconds:D2}";
            this.currentDuration.Text ??= "0:00";
        }

        // Toggle the play/pause button image when clicked
        private void OnPauseButtonClick(object sender, EventArgs eventArgs)
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

        // Skip to the next track when skip next is clicked
        private void OnNextButtonClick(object sender, EventArgs eventArgs)
        {
            Playback.SkipNext();
        }

        // Skip to the previous track when skip previous is clicked
        private void OnPreviousButtonClick(object sender, EventArgs eventArgs)
        {
            Playback.SkipPrevious();
        }

        // Seek playback to the specified position when the seekbar is moved
        private void OnSeekBarTrackerSeek(object source, SeekBarTracker.SeekEventArgs eventArgs)
        {
            Playback.CurrentPosition = (uint)eventArgs.Stop;
        }

        // Update seekbar progress and text periodically
        private void OnSeekBarTrackerUpdate(object source, SeekBarTracker.UpdateEventArgs eventArgs)
        {
            int position = (int)Playback.CurrentPosition;
            this.RunOnUiThread(() =>
            {
                if (!eventArgs.IsTouched)
                {
                    this.seekBar.Progress = position;
                }
                TimeSpan current = TimeSpan.FromMilliseconds(position);
                this.currentDuration.Text = $"{current.Minutes}:{current.Seconds:D2}";
            });
        }
    }
}