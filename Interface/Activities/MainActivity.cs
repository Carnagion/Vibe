using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;

using Java.Lang;

using Vibe.Interface.Fragments;
using Vibe.Interface.Services;
using Vibe.Music;
using Vibe.Utility.Extensions;

using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using String = Java.Lang.String;

namespace Vibe.Interface.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.Vibe", MainLauncher = false, NoHistory = false)]
    internal sealed class MainActivity : AppCompatActivity
    {
        private DrawerLayout drawerLayout = null!;

        private NavigationView navigationView = null!;

        private NowPlayingFragment nowPlayingFragment = null!;
        
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId is not 16908332)
            {
                return base.OnOptionsItemSelected(item);
            }
            if (this.drawerLayout.IsDrawerOpen(this.navigationView))
            {
                this.drawerLayout.CloseDrawer(this.navigationView);
            }
            else
            {
                this.drawerLayout.OpenDrawer(this.navigationView);
            }
            return true;
        }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            this.SetContentView(Resource.Layout.activity_main);

            ViewPager pager = this.FindViewById<ViewPager>(Resource.Id.activity_main_pager)!;
            pager.Adapter = new MainViewPagerAdapter(this.SupportFragmentManager);
            this.FindViewById<TabLayout>(Resource.Id.activity_main_tabs)!.SetupWithViewPager(pager);
            
            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.activity_main_toolbar)!);
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            this.SupportActionBar.SetDisplayShowTitleEnabled(false);
            this.SupportActionBar.SetHomeButtonEnabled(true);
            this.SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.icon_main_menu);
            this.drawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.activity_main_drawer)!;
            this.navigationView = this.FindViewById<NavigationView>(Resource.Id.activity_main_navigator)!;
            this.drawerLayout.CloseDrawer(this.navigationView);
            this.navigationView.NavigationItemSelected += this.OnNavigationViewNavigationItemSelected;
            
            if (savedInstanceState is null)
            {
                Playback.Setup();
            }

            Intent service = new(this, typeof(NowPlayingService));
            service.SetAction(ForegroundService.start);
            this.StartForegroundService(service);

            this.nowPlayingFragment = (NowPlayingFragment)this.SupportFragmentManager.FindFragmentById(Resource.Id.activity_main_fragment_nowplaying);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (this.nowPlayingFragment.IsHidden && Playback.NowPlaying is not null && Playback.PlayingState is not (Playback.MediaPlayerState.Completed or Playback.MediaPlayerState.Stopped or Playback.MediaPlayerState.End))
            {
                this.nowPlayingFragment.Show(true);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            this.navigationView.NavigationItemSelected -= this.OnNavigationViewNavigationItemSelected;
            
            if (!this.IsFinishing)
            {
                return;
            }
            Playback.End();
        }

        private void OnNavigationViewNavigationItemSelected(object source, NavigationView.NavigationItemSelectedEventArgs eventArgs)
        {
            switch (eventArgs.MenuItem.ItemId)
            {
                case Resource.Id.menu_main_navigator_settings:
                    break;
                case Resource.Id.menu_main_navigator_about:
                    break;
            }
            this.drawerLayout.CloseDrawer(this.navigationView);
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