using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using Vibe.Interface.Activities;
using Vibe.Music;

namespace Vibe.Interface.Fragments
{
    internal sealed class ArtistListFragment : SelectablePopupListFragment<Artist>
    {
        public ArtistListFragment()
        {
        }
        
        public ArtistListFragment(IEnumerable<Artist> artists) : base(artists)
        {
        }
        
        protected override int FragmentViewId
        {
            get
            {
                return Resource.Layout.fragment_artists;
            }
        }

        protected override int ListViewId
        {
            get
            {
                return Resource.Id.artists;
            }
        }

        protected override int ListItemViewId
        {
            get
            {
                return Resource.Layout.item_artist;
            }
        }

        protected override int MoreButtonId
        {
            get
            {
                return Resource.Id.item_artist_more;
            }
        }

        protected override int MoreMenuId
        {
            get
            {
                return Resource.Menu.more_artist;
            }
        }

        protected override int MoreMenuStyle
        {
            get
            {
                return Resource.Style.Menu_Vibe_Popup;
            }
        }

        protected override void OnListItemViewGet(Artist artist, int position, View? view)
        {
            base.OnListItemViewGet(artist, position, view);
            
            ImageView image = view!.FindViewById<ImageView>(Resource.Id.item_artist_image)!;
            image.SetImageBitmap(artist.Albums.First().Artwork);
            image.ClipToOutline = true;
            
            view.FindViewById<TextView>(Resource.Id.item_artist_name)!.Text = artist.Name;
            
            int albumCount = artist.Albums.Count();
            int trackCount = artist.Tracks.Count();
            view.FindViewById<TextView>(Resource.Id.item_artist_info)!.Text = $"{albumCount} {(albumCount is 1 ? "album" : "albums")} ({trackCount} {(trackCount is 1 ? "track" : "tracks")})";
        }

        protected override void OnListViewItemClick(Artist artist, int position, View? view)
        {
            Bundle extras = new();
            extras.PutLong("artistId", artist.Id);
            
            Intent intent = new(Application.Context, typeof(ArtistActivity));
            intent.PutExtras(extras);
            
            this.Activity.StartActivity(intent);
        }

        protected override void OnMorePopupItemClick(Artist artist, IMenuItem? clicked, PopupMenu popup)
        {
            switch (clicked?.ItemId)
            {
                case Resource.Id.more_artist_play:
                    Playback.Start(artist.Tracks);
                    break;
                case Resource.Id.more_artist_insert:
                    Playback.InsertNextInQueue(artist.Tracks);
                    break;
                case Resource.Id.more_artist_append:
                    Playback.AddToQueue(artist.Tracks);
                    break;
            }
        }
    }
}