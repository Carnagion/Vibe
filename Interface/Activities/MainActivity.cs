using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.App;

using Java.Lang;

using Vibe.Interface.Fragments;
using Vibe.Interface.Services;
using Vibe.Music;

using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using String = Java.Lang.String;

namespace Vibe.Interface.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.Vibe", MainLauncher = false, NoHistory = false)]
    internal sealed class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            this.SetContentView(Resource.Layout.activity_main);

            ViewPager pager = this.FindViewById<ViewPager>(Resource.Id.activity_main_pager)!;
            pager.Adapter = new MainViewPagerAdapter(this.SupportFragmentManager);
            this.FindViewById<TabLayout>(Resource.Id.activity_main_tabs)!.SetupWithViewPager(pager);
            
            if (savedInstanceState is null)
            {
                Playback.Setup();
            }

            Intent service = new(this, typeof(NowPlayingService));
            service.SetAction(ForegroundService.start);
            this.StartForegroundService(service);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (this.IsFinishing)
            {
                Playback.End();
            }
        }

        private sealed class MainViewPagerAdapter : FragmentStatePagerAdapter
        {
            public MainViewPagerAdapter(FragmentManager fragmentManager) : base(fragmentManager)
            {
            }

            public override int Count
            {
                get
                {
                    return 5;
                }
            }

            public override Fragment GetItem(int position)
            {
                switch (position)
                {
                    case 0:
                        ArtistListFragment artists = new();
                        artists.Items.AddRange(Library.Artists);
                        return artists;
                    case 1:
                        AlbumListFragment albums = new();
                        albums.Items.AddRange(Library.Albums);
                        return albums;
                    case 2:
                        TrackListFragment tracks = new();
                        tracks.Items.AddRange(Library.Tracks);
                        return tracks;
                    case 3 or 4:
                        TrackListFragment temp = new();
                        return temp;
                }
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            public override ICharSequence GetPageTitleFormatted(int position)
            {
                return position switch
                {
                    0 => new String(Application.Context.GetString(Resource.String.tab_artists)),
                    1 => new(Application.Context.GetString(Resource.String.tab_albums)),
                    2 => new(Application.Context.GetString(Resource.String.tab_tracks)),
                    3 => new(Application.Context.GetString(Resource.String.tab_playlists)),
                    4 => new(Application.Context.GetString(Resource.String.tab_compilations)),
                    _ => throw new ArgumentOutOfRangeException(nameof(position)),
                };
            }
        }
    }
}