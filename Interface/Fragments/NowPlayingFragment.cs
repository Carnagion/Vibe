using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using Vibe.Interface.Activities;
using Vibe.Music;

namespace Vibe.Interface.Fragments
{
    internal sealed class NowPlayingFragment : HideableFragment
    {
        private View view = null!;
        
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.view = inflater.Inflate(Resource.Layout.fragment_nowplaying, container, false)!;
            this.view.Click += this.OnViewClick;

            if (Playback.NowPlaying is null)
            {
                this.Hide();
            }
            else
            {
                this.UpdateTrackInfo();
            }

            Playback.MediaPlayerStateChanged += this.OnPlaybackMediaPlayerStateChanged;

            return this.view;
        }

        public override void OnDestroy()
        {
            Playback.MediaPlayerStateChanged -= this.OnPlaybackMediaPlayerStateChanged;
            base.OnDestroy();
        }

        private void UpdateTrackInfo()
        {
            this.view.FindViewById<TextView>(Resource.Id.fragment_nowplaying_tracktitle)!.Text = Playback.NowPlaying!.Title;
            this.view.FindViewById<TextView>(Resource.Id.fragment_nowplaying_trackinfo)!.Text = $"{Playback.NowPlaying.Album.Title} · {Playback.NowPlaying.Artist.Name}";
        }

        private void OnViewClick(object source, EventArgs eventArgs)
        {
            this.Activity.StartActivity(new Intent(Application.Context, typeof(NowPlayingActivity)));
        }

        private void OnPlaybackMediaPlayerStateChanged(object source, Playback.MediaPlayerStateChangeArgs eventArgs)
        {
            switch (eventArgs.ChangedTo)
            {
                case Playback.MediaPlayerState.Stopped or Playback.MediaPlayerState.End:
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
                    this.UpdateTrackInfo();
                    break;
            }
        }
    }
}