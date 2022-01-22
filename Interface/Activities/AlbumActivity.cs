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

using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using String = Java.Lang.String;

namespace Vibe.Interface.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.Vibe", MainLauncher = false, NoHistory = false)]
    internal sealed class AlbumActivity : AppCompatActivity
    {
        private Album album = null!;
        
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            this.SetContentView(Resource.Layout.activity_album);

            long albumId = this.Intent!.GetLongExtra("albumId", default);
            this.album = (from album in Library.Albums
                          where album.Id == albumId
                          select album).First();

            ViewPager pager = this.FindViewById<ViewPager>(Resource.Id.activity_album_pager)!;
            pager.Adapter = new AlbumViewPagerAdapter(this.SupportFragmentManager, this.album);
            this.FindViewById<TabLayout>(Resource.Id.activity_album_tabs)!.SetupWithViewPager(pager);

            this.FindViewById<TextView>(Resource.Id.activity_album_albumtitle)!.Text = this.album.Title;
        }

        private sealed class AlbumViewPagerAdapter : FragmentStatePagerAdapter
        {
            public AlbumViewPagerAdapter(FragmentManager fragmentManager, Album album) : base(fragmentManager)
            {
                this.album = album;
            }

            private readonly Album album;

            public override int Count
            {
                get
                {
                    return 1;
                }
            }

            public override Fragment GetItem(int position)
            {
                switch (position)
                {
                    case 0:
                        TrackListFragment tracks = new();
                        tracks.Items.AddRange(this.album.Tracks);
                        return tracks;
                }
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            public override ICharSequence GetPageTitleFormatted(int position)
            {
                return position switch
                {
                    0 => new String(Application.Context.GetString(Resource.String.tab_tracks)),
                    _ => throw new ArgumentOutOfRangeException(nameof(position)),
                };
            }
        }
    }
}