using System;

using Android.App;
using Android.Arch.Lifecycle;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using Vibe.Interface.Activities;
using Vibe.Music;
using Vibe.Utility.Extensions;

using Fragment = Android.Support.V4.App.Fragment;

namespace Vibe.Interface.Fragments
{
    internal sealed class NowPlayingFragment : Fragment
    {
        private ImageView trackImage = null!;

        private TextView trackTitle = null!;

        private TextView trackInfo = null!;

        private ImageButton pauseButton = null!;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Playback.MediaPlayerStateChanged += this.OnPlaybackMediaPlayerStateChanged;
            
            View view = inflater.Inflate(Resource.Layout.fragment_now, container, false)!;
            view.Click += this.OnViewClick;

            this.trackImage = view.FindViewById<ImageView>(Resource.Id.fragment_now_image)!;
            this.trackTitle = view.FindViewById<TextView>(Resource.Id.fragment_now_title)!;
            this.trackInfo = view.FindViewById<TextView>(Resource.Id.fragment_now_info)!;
            this.pauseButton = view.FindViewById<ImageButton>(Resource.Id.fragment_now_pause)!;
            this.pauseButton.Click += this.OnPauseButtonClick;
            
            return view;
        }

        // Hide or update fragment depending on current playback state
        public override void OnResume()
        {
            base.OnResume();
            
            if (Playback.NowPlaying is null)
            {
                this.Hide();
            }
            else
            {
                if (this.IsHidden)
                {
                    this.Show();
                }
                this.UpdateTrackInfo(Playback.NowPlaying);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            Playback.MediaPlayerStateChanged -= this.OnPlaybackMediaPlayerStateChanged;
        }

        // Update views to match information of the track
        private void UpdateTrackInfo(Track track)
        {
            this.trackImage.SetImageBitmap(track.Album.Artwork);
            this.trackTitle.Text = track.Title;
            this.trackInfo.Text = $"{track.Album.Title} · {track.Artist.Name}";
            this.pauseButton.SetImageResource(Playback.PlayingState is Playback.MediaPlayerState.Started ? Resource.Drawable.pause : Resource.Drawable.play);
        }

        // Switch to now playing activity when clicked
        private void OnViewClick(object source, EventArgs eventArgs)
        {
            this.Activity.StartActivity(new Intent(Application.Context, typeof(NowPlayingActivity)));
        }

        // Play or pause the track when the pause button is clicked
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
        
        // Hide or update fragment depending on current playback state
        private void OnPlaybackMediaPlayerStateChanged(object source, Playback.MediaPlayerStateChangeArgs eventArgs)
        {
            // Prevent updating views if the parent activity is not currently being viewed
            if (!this.Activity.Lifecycle.CurrentState.IsAtLeast(Lifecycle.State.Resumed))
            {
                return;
            }
            
            switch (eventArgs.ChangedTo)
            {
                case Playback.MediaPlayerState.Completed or Playback.MediaPlayerState.Stopped or Playback.MediaPlayerState.End:
                    this.Hide();
                    break;
                default:
                    if (Playback.NowPlaying is null)
                    {
                        break;
                    }
                    if (this.IsHidden)
                    {
                        this.Show();
                    }
                    this.UpdateTrackInfo(Playback.NowPlaying);
                    break;
            }
        }
    }
}