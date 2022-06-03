using System;
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
    internal sealed class AlbumListFragment : SelectablePopupListFragment<Album>
    {
        public AlbumListFragment()
        {
        }
        
        public AlbumListFragment(IEnumerable<Album> albums) : base(albums)
        {
        }
        
        protected override int FragmentViewId
        {
            get
            {
                return Resource.Layout.fragment_albums;
            }
        }

        protected override int ListViewId
        {
            get
            {
                return Resource.Id.albums;
            }
        }

        protected override int ListItemViewId
        {
            get
            {
                return Resource.Layout.item_album;
            }
        }

        protected override int MoreButtonId
        {
            get
            {
                return Resource.Id.item_album_more;
            }
        }

        protected override int MoreMenuId
        {
            get
            {
                return Resource.Menu.more_album;
            }
        }

        protected override int MoreMenuStyle
        {
            get
            {
                return Resource.Style.Menu_Vibe_Popup;
            }
        }

        protected override void OnListItemViewGet(Album album, int position, View? view)
        {
            base.OnListItemViewGet(album, position, view);
            
            ImageView image = view!.FindViewById<ImageView>(Resource.Id.item_album_image)!;
            image.SetImageBitmap(album.Artwork);
            image.ClipToOutline = true;
            
            view.FindViewById<TextView>(Resource.Id.item_album_title)!.Text = album.Title;
            
            int trackCount = album.Tracks.Count();
            view.FindViewById<TextView>(Resource.Id.item_album_info)!.Text = $"{album.Artist.Name} · {trackCount} {(trackCount is 1 ? "track" : "tracks")}";
        }

        protected override void OnListViewItemClick(Album album, int position, View? view)
        {
            Bundle extras = new();
            extras.PutLong("albumId", album.Id);
            extras.PutLong("artistId", album.Artist.Id);
            
            Intent intent = new(Application.Context, typeof(AlbumActivity));
            intent.PutExtras(extras);
            
            this.Activity.StartActivity(intent);
        }

        protected override void OnMorePopupItemClick(Album album, IMenuItem? clicked, PopupMenu popup)
        {
            switch (clicked?.ItemId)
            {
                case Resource.Id.more_album_play:
                    Playback.Start(album.Tracks);
                    break;
                case Resource.Id.more_album_insert:
                    Playback.InsertNextInQueue(album.Tracks);
                    break;
                case Resource.Id.more_album_append:
                    Playback.AddToQueue(album.Tracks);
                    break;
            }
        }
    }
}