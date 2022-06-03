using System;
using System.Linq;

using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.App;

using Java.Lang;

using Vibe.Interface.Fragments;
using Vibe.Music;

using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using String = Java.Lang.String;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Vibe.Interface.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.Vibe", MainLauncher = false, NoHistory = false)]
    internal sealed class ArtistActivity : AppCompatActivity
    {
        private Artist artist = null!;

        // Make back button go to previous activity when clicked
        public override bool OnSupportNavigateUp()
        {
            this.OnBackPressed();
            return true;
        }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            this.SetContentView(Resource.Layout.activity_artist);
            
            this.SetupArtist();
            this.SetupActionBar();
            this.SetupViewPager();
        }

        // Retrieve the artist that needs to be shown
        private void SetupArtist()
        {
            long artistId = this.Intent!.GetLongExtra("artistId", default);
            this.artist = (from artist in Library.Artists
                           where artist.Id == artistId
                           select artist).Single();
        }

        // Setup the toolbar
        private void SetupActionBar()
        {
            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.activity_artist_toolbar));
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            this.SupportActionBar.SetDisplayShowHomeEnabled(true);

            this.FindViewById<CollapsingToolbarLayout>(Resource.Id.activity_artist_collapsingToolbar)!.Title = this.artist.Name;
        }

        // Setup the view pager to work with the tabs
        private void SetupViewPager()
        {
            ViewPager viewPager = this.FindViewById<ViewPager>(Resource.Id.activity_artist_viewpager)!;
            viewPager.Adapter = new ArtistViewPagerAdapter(this.artist, this.SupportFragmentManager);
            this.FindViewById<TabLayout>(Resource.Id.activity_artist_tabs)!.SetupWithViewPager(viewPager);
        }
        
        // Adapter class for tabs
        private sealed class ArtistViewPagerAdapter : FragmentStatePagerAdapter
        {
            public ArtistViewPagerAdapter(Artist artist, FragmentManager fragmentManager) : base(fragmentManager)
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

            // Return the corresponding fragment for each tab
            public override Fragment GetItem(int position)
            {
                return position switch
                {
                    0 => new AlbumListFragment(this.artist.Albums),
                    1 => new TrackListFragment(this.artist.Tracks),
                    _ => throw new ArgumentOutOfRangeException(nameof(position)),
                };
            }

            // Must be overriden for tab layout titles to display properly. Xamarin moment
            public override ICharSequence GetPageTitleFormatted(int position)
            {
                return position switch
                {
                    0 => new String(Application.Context.GetString(Resource.String.activity_artist_tab_albums)),
                    1 => new(Application.Context.GetString(Resource.String.activity_artist_tab_tracks)),
                    _ => throw new ArgumentOutOfRangeException(nameof(position)),
                };
            }
        }
    }
}