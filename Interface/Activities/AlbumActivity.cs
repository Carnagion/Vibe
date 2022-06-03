using System.Linq;

using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Widget;

using Vibe.Interface.Fragments;
using Vibe.Music;

using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Vibe.Interface.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.Vibe", MainLauncher = false, NoHistory = false)]
    internal sealed class AlbumActivity : AppCompatActivity
    {
        private Album album = null!;

        // Make back button go to previous activity when clicked
        public override bool OnSupportNavigateUp()
        {
            this.OnBackPressed();
            return true;
        }
        
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            this.SetContentView(Resource.Layout.activity_album);
            
            this.SetupAlbum();
            this.SetupActionBar();
            this.SetupTrackList();
        }

        // Retrieve the album that needs to be shown
        private void SetupAlbum()
        {
            long albumId = this.Intent!.GetLongExtra("albumId", default);
            long artistId = this.Intent.GetLongExtra("artistId", default); // Both album ID and artist ID are used because album IDs are not unique, which is quite frankly a poor design decision, Android
            this.album = (from album in Library.Albums
                          where (album.Id == albumId) && (album.Artist.Id == artistId)
                          select album).Single();
        }

        // Setup the toolbar
        private void SetupActionBar()
        {
            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.activity_album_toolbar)!);
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            this.SupportActionBar.SetDisplayShowHomeEnabled(true);

            this.FindViewById<CollapsingToolbarLayout>(Resource.Id.activity_album_collapsingToolbar)!.Title = this.album.Title;
            
            this.FindViewById<ImageView>(Resource.Id.activity_album_image)!.SetImageBitmap(this.album.Artwork);
        }

        // Setup the track list view
        private void SetupTrackList()
        {
            TrackListFragment trackList = (TrackListFragment)this.SupportFragmentManager.FindFragmentById(Resource.Id.activity_album_trackList);
            trackList.Items.Clear();
            trackList.Items.AddRange(this.album.Tracks);
            trackList.NotifyDataSetChanged();
        }
    }
}