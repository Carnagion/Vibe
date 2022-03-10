using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;

using Vibe.Interface.Activities;
using Vibe.Music;
using Vibe.Utility.Extensions;

namespace Vibe.Interface.Fragments
{
    internal sealed class AlbumListFragment : SelectableListFragment<Album>
    {
        private readonly Dictionary<ImageButton, Album> menuMappings = new();

        private ImageButton currentClickedMenu = null!;

        protected override int FragmentViewId
        {
            get
            {
                return Resource.Layout.fragment_list_album;
            }
        }

        protected override int ListViewId
        {
            get
            {
                return Resource.Id.fragment_list_album_list;
            }
        }

        protected override int ListItemViewId
        {
            get
            {
                return Resource.Layout.list_album_item;
            }
        }

        protected override void OnListItemViewGet(Album item, int position, View? view)
        {
            ImageView image = view!.FindViewById<ImageView>(Resource.Id.list_album_item_image)!;
            Bitmap? artwork = item.Artwork;
            if (artwork is null)
            {
                image.SetImageResource(Resource.Drawable.album);
            }
            else
            {
                image.SetImageBitmap(artwork);
            }
            
            view.FindViewById<TextView>(Resource.Id.list_album_item_title)!.Text = item.Title;
            view.FindViewById<TextView>(Resource.Id.list_album_item_info)!.Text = item.Artist.Name;
            
            ImageButton more = view.FindViewById<ImageButton>(Resource.Id.list_album_item_more)!;
            more.Focusable = false;
            this.menuMappings[more] = item;
            more.Click -= this.OnMoreMenuClick;
            more.Click += this.OnMoreMenuClick;
        }

        protected override void OnListViewItemClick(Album item, int position, View? view)
        {
            Bundle extras = new();
            extras.PutLong("albumId", item.Id);
            Intent intent = new(Application.Context, typeof(AlbumActivity));
            intent.PutExtras(extras);
            this.Activity.StartActivity(intent);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            this.menuMappings.Clear();
        }

        private void OnMoreMenuClick(object source, EventArgs eventArgs)
        {
            ImageButton more = (ImageButton)source;
            PopupMenu popup = more.ShowPopupMenu(Resource.Menu.menu_more_album, Resource.Style.Menu_Vibe_Popup, GravityFlags.End);
            this.currentClickedMenu = more;
            popup.MenuItemClick += this.OnCurrentClickedMenuMenuItemClick;
        }

        private void OnCurrentClickedMenuMenuItemClick(object source, PopupMenu.MenuItemClickEventArgs eventArgs)
        {
            Album album = this.menuMappings[this.currentClickedMenu];
            switch (eventArgs.Item?.ItemId)
            {
                case Resource.Id.menu_more_album_play:
                    Playback.NewPlayingQueue(album.Tracks);
                    Playback.Start();
                    break;
                case Resource.Id.menu_more_album_insert:
                    album.Tracks.Reverse().ForEach(Playback.InsertNextInQueue);
                    break;
                case Resource.Id.menu_more_album_append:
                    album.Tracks.ForEach(Playback.AddToQueue);
                    break;
                case Resource.Id.menu_more_album_add_to_playlist:
                    Bundle bundle = new();
                    bundle.PutLong("albumId", album.Id);
                    Intent intent = new(Application.Context, typeof(AddToPlaylistActivity));
                    intent.PutExtras(bundle);
                    this.Activity.StartActivity(intent);
                    break;
            }
        }
    }
}