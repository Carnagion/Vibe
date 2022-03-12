using System;
using System.Linq;

using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Widget;

using Java.Lang;

using Vibe.Interface.Fragments;
using Vibe.Music;
using Vibe.Utility.Extensions;

using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using String = Java.Lang.String;

namespace Vibe.Interface.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.Vibe", MainLauncher = false, NoHistory = false)]
    internal sealed class ArtistActivity : AppCompatActivity
    {
        private Artist artist = null!;

        private NowPlayingFragment nowPlayingFragment = null!;

        public override bool OnSupportNavigateUp()
        {
            this.OnBackPressed();
            return true;
        }
        
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            this.SetContentView(Resource.Layout.activity_artist);

            long artistId = this.Intent!.GetLongExtra("artistId", default);
            this.artist = (from artist in Library.Artists
                           where artist.Id == artistId
                           select artist).First();

            ViewPager pager = this.FindViewById<ViewPager>(Resource.Id.activity_artist_pager)!;
            pager.Adapter = new ArtistViewPagerAdapter(this.SupportFragmentManager, this.artist);
            this.FindViewById<TabLayout>(Resource.Id.activity_artist_tabs)!.SetupWithViewPager(pager);

            this.FindViewById<TextView>(Resource.Id.activity_artist_artistname)!.Text = this.artist.Name;

            this.nowPlayingFragment = (NowPlayingFragment)this.SupportFragmentManager.FindFragmentById(Resource.Id.activity_artist_fragment_nowplaying);
            
            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.activity_artist_toolbar)!);
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            this.SupportActionBar.SetDisplayShowHomeEnabled(true);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (this.nowPlayingFragment.IsHidden && Playback.NowPlaying is not null && Playback.PlayingState is not (Playback.MediaPlayerState.Completed or Playback.MediaPlayerState.Stopped or Playback.MediaPlayerState.End))
            {
                this.nowPlayingFragment.Show(true);
            }
        }

        private sealed class ArtistViewPagerAdapter : FragmentStatePagerAdapter
        {
            public ArtistViewPagerAdapter(FragmentManager fragmentManager, Artist artist) : base(fragmentManager)
            {
                this.artist = artist;
            }

            private readonly Artist artist;

            public override int Count
            {
                get
                {
                    return 2;
                }
            }

            public override Fragment GetItem(int position)
            {
                return position switch
                {
                    0 => new AlbumListFragment(this.artist.Albums),
                    1 => new TrackListFragment(this.artist.Tracks),
                    _ => throw new ArgumentOutOfRangeException(nameof(position)),
                };
            }

            public override ICharSequence GetPageTitleFormatted(int position)
            {
                return position switch
                {
                    0 => new String(Application.Context.GetString(Resource.String.tab_albums)),
                    1 => new(Application.Context.GetString(Resource.String.tab_tracks)),
                    _ => throw new ArgumentOutOfRangeException(nameof(position)),
                };
            }
        }
    }
}