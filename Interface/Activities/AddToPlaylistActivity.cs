using System.Linq;

using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

using Vibe.Music;

using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Vibe.Interface.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.Vibe", MainLauncher = false, NoHistory = false)]
    internal sealed class AddToPlaylistActivity : AppCompatActivity
    {
        private ListView listView = null!;

        private Playlist[] playlists = null!;

        private Track? trackToAdd;

        private Album? albumToAdd;

        private Artist? artistToAdd;

        public void RefreshPlaylists()
        {
            this.playlists = (from playlist in Library.Playlists
                              orderby playlist.Title
                              select playlist).ToArray();
            this.listView.Adapter = new AddToPlaylistAdapter(this);
        }
        
        public override bool OnSupportNavigateUp()
        {
            this.OnBackPressed();
            return true;
        }
        
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            this.SetContentView(Resource.Layout.activity_addtoplaylist);

            long trackId = this.Intent!.GetLongExtra("trackId", default);
            this.trackToAdd = Library.Tracks.FirstOrDefault(track => track.Id == trackId);

            long albumId = this.Intent.GetLongExtra("albumId", default);
            this.albumToAdd = Library.Albums.FirstOrDefault(album => album.Id == albumId);

            long artistId = this.Intent.GetLongExtra("artistId", default);
            this.artistToAdd = Library.Artists.FirstOrDefault(artist => artist.Id == artistId);
            
            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.activity_addtoplaylist_toolbar)!);
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            this.SupportActionBar.SetDisplayShowHomeEnabled(true);

            this.listView = this.FindViewById<ListView>(Resource.Id.activity_addtoplaylist_list)!;
            this.listView.NestedScrollingEnabled = true;
            this.listView.ItemClick += this.OnListViewItemClick;
            
            this.RefreshPlaylists();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.listView.ItemClick -= this.OnListViewItemClick;
        }

        private void OnListViewItemClick(object source, AdapterView.ItemClickEventArgs eventArgs)
        {
            Playlist playlist = this.playlists[eventArgs.Position];
            if (this.trackToAdd is not null && !playlist.Contains(this.trackToAdd))
            {
                playlist.Add(this.trackToAdd);
            }
            else if (this.albumToAdd is not null && !playlist.ContainsAll(this.albumToAdd.Tracks))
            {
                playlist.AddRange(this.albumToAdd.Tracks.Except(playlist));
            }
            else if (this.artistToAdd is not null && !playlist.ContainsAll(this.artistToAdd.Tracks))
            {
                playlist.AddRange(this.artistToAdd.Tracks.Except(playlist));
            }
            else
            {
                Snackbar.Make(this.FindViewById<CoordinatorLayout>(Resource.Id.activity_addtoplaylist_layout), $"The selected track(s) already exist in {playlist.Title}.", Snackbar.LengthShort)
                    .Show();
            }
            this.RefreshPlaylists();
        }

        private sealed class AddToPlaylistAdapter : BaseAdapter<Playlist>
        {
            public AddToPlaylistAdapter(AddToPlaylistActivity addToPlaylistActivity)
            {
                this.activity = addToPlaylistActivity;
            }

            private readonly AddToPlaylistActivity activity;
            
            public override int Count
            {
                get
                {
                    return this.activity.playlists.Length;
                }
            }

            public override Playlist this[int position]
            {
                get
                {
                    return this.activity.playlists[position];
                }
            }

            public override View? GetView(int position, View? convertView, ViewGroup? parent)
            {
                Playlist playlist = this.activity.playlists[position];
                
                View? view = convertView ?? this.activity.LayoutInflater.Inflate(Resource.Layout.list_playlist_item, parent, false);
                
                ImageView image = view!.FindViewById<ImageView>(Resource.Id.list_playlist_item_image)!;
                Bitmap? artwork = playlist.FirstOrDefault()?.Album.Artwork;
                if (artwork is null)
                {
                    image.SetImageResource(Resource.Drawable.album);
                }
                else
                {
                    image.SetImageBitmap(artwork);
                }
            
                view.FindViewById<TextView>(Resource.Id.list_playlist_item_title)!.Text = playlist.Title;
                view.FindViewById<TextView>(Resource.Id.list_playlist_item_info)!.Text = playlist.Count is 1 ? "1 track" : $"{playlist.Count} tracks";

                view.FindViewById<ImageButton>(Resource.Id.list_playlist_item_more)!.Visibility = ViewStates.Gone;
                
                return view;
            }

            public override long GetItemId(int position)
            {
                return position;
            }
        }
    }
}