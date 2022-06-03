using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Support.V7.Widget;

using Java.Lang;

using Vibe.Interface.Fragments;
using Vibe.Interface.Services;
using Vibe.Music;
using Vibe.Utility.Extensions;

using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using String = Java.Lang.String;

namespace Vibe.Interface.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.Vibe", MainLauncher = false, NoHistory = false)]
    internal sealed class MainActivity : AppCompatActivity
    {
        private NowPlayingFragment nowPlayingFragment = null!;
        
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            // If savedInstanceState is null, it indicates that the activity has been created for the first time, and not due to changing orientations or other reasons
            if (savedInstanceState is null)
            {
                Playback.Setup();
            }
            
            this.SetContentView(Resource.Layout.activity_main);
            
            this.SetupActionBar();
            this.SetupViewPager();
            this.SetupNowPlayingService();

            this.nowPlayingFragment = (NowPlayingFragment)this.SupportFragmentManager.FindFragmentById(Resource.Id.activity_main_fragment_now);
        }

        protected override void OnResume()
        {
            base.OnResume();
            
            // Show now playing upon resume if necessary
            if (this.nowPlayingFragment.IsHidden && Playback.NowPlaying is not null && Playback.PlayingState is not (Playback.MediaPlayerState.Completed or Playback.MediaPlayerState.Stopped or Playback.MediaPlayerState.End))
            {
                this.nowPlayingFragment.Show(true);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // If not finishing, it indicates that the activity will be recreated again due to changing orientations or other reasons
            if (!this.IsFinishing)
            {
                return;
            }
            
            Playback.End();
        }

        // Setup the toolbar
        private void SetupActionBar()
        {
            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.activity_main_toolbar));
            /*this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            this.SupportActionBar.SetDisplayShowTitleEnabled(false);
            this.SupportActionBar.SetHomeButtonEnabled(true);
            this.SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.menu);*/
        }

        // Setup the view pager to work with the tabs
        private void SetupViewPager()
        {
            ViewPager viewPager = this.FindViewById<ViewPager>(Resource.Id.activity_main_viewpager)!;
            viewPager.Adapter = new MainViewPagerAdapter(this.SupportFragmentManager);
            this.FindViewById<TabLayout>(Resource.Id.activity_main_tabs)!.SetupWithViewPager(viewPager);
        }

        // Setup the now playing notification service
        private void SetupNowPlayingService()
        {
            Intent service = new(this, typeof(NowPlayingService));
            service.SetAction(ForegroundService.start);
            this.StartForegroundService(service);
        }
        
        // Adapter class for tabs
        private sealed class MainViewPagerAdapter : FragmentStatePagerAdapter
        {
            public MainViewPagerAdapter(FragmentManager fragmentManager) : base(fragmentManager)
            {
            }

            public override int Count
            {
                get
                {
                    return 3;
                }
            }

            // Return the corresponding fragment for each tab
            public override Fragment GetItem(int position)
            {
                return position switch
                {
                    0 => new ArtistListFragment(Library.Artists),
                    1 => new AlbumListFragment(Library.Albums),
                    2 => new TrackListFragment(Library.Tracks),
                    _ => throw new ArgumentOutOfRangeException(nameof(position)),
                };
            }

            // Must be overriden for tab layout titles to display properly. Xamarin moment
            public override ICharSequence GetPageTitleFormatted(int position)
            {
                return position switch
                {
                    0 => new String(Application.Context.GetString(Resource.String.activity_main_tab_artists)),
                    1 => new(Application.Context.GetString(Resource.String.activity_main_tab_albums)),
                    2 => new(Application.Context.GetString(Resource.String.activity_main_tab_tracks)),
                    _ => throw new ArgumentOutOfRangeException(nameof(position)),
                };
            }
        }
    }
}