using System;
using System.Collections.Generic;
using System.Linq;

using Android.Graphics;
using Android.Views;
using Android.Widget;

using Vibe.Music;
using Vibe.Utility.Extensions;

namespace Vibe.Interface.Fragments
{
    internal sealed class PlaylistListFragment : SelectableListFragment<Playlist>
    {
        public PlaylistListFragment(IEnumerable<Playlist> items) : base(items)
        {
        }
        
        private readonly Dictionary<ImageButton, Playlist> menuMappings = new();

        private ImageButton currentClickedMenu = null!;

        protected override int FragmentViewId
        {
            get
            {
                return Resource.Layout.fragment_list_playlist;
            }
        }

        protected override int ListViewId
        {
            get
            {
                return Resource.Id.fragment_list_playlist_list;
            }
        }

        protected override int ListItemViewId
        {
            get
            {
                return Resource.Layout.list_playlist_item;
            }
        }

        public override void OnResume()
        {
            base.OnResume();
            this.RefreshData(from playlist in Library.Playlists
                             orderby playlist.Title
                             select playlist);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            this.menuMappings.Clear();
        }

        protected override void OnListItemViewGet(Playlist item, int position, View? view)
        {
            ImageView image = view!.FindViewById<ImageView>(Resource.Id.list_playlist_item_image)!;
            Bitmap? artwork = item.FirstOrDefault()?.Album.Artwork;
            if (artwork is null)
            {
                image.SetImageResource(Resource.Drawable.album);
            }
            else
            {
                image.SetImageBitmap(artwork);
            }
            
            view.FindViewById<TextView>(Resource.Id.list_playlist_item_title)!.Text = item.Title;
            view.FindViewById<TextView>(Resource.Id.list_playlist_item_info)!.Text = item.Count is 1 ? "1 track" : $"{item.Count} tracks";
            
            ImageButton more = view.FindViewById<ImageButton>(Resource.Id.list_playlist_item_more)!;
            more.Focusable = false;
            this.menuMappings[more] = item;
            more.Click -= this.OnMoreMenuClick;
            more.Click += this.OnMoreMenuClick;
        }

        protected override void OnListViewItemClick(Playlist item, int position, View? view)
        {
            Playback.NewPlayingQueue(item);
            Playback.Start();
        }

        private void OnMoreMenuClick(object source, EventArgs eventArgs)
        {
            ImageButton more = (ImageButton)source;
            PopupMenu popup = more.ShowPopupMenu(Resource.Menu.menu_more_playlist, Resource.Style.Menu_Vibe_Popup, GravityFlags.End);
            this.currentClickedMenu = more;
            popup.MenuItemClick += this.OnCurrentClickedMenuMenuItemClick;
        }

        private void OnCurrentClickedMenuMenuItemClick(object source, PopupMenu.MenuItemClickEventArgs eventArgs)
        {
            Playlist clickedPlaylist = this.menuMappings[this.currentClickedMenu];
            switch (eventArgs.Item?.ItemId)
            {
                case Resource.Id.menu_more_playlist_play:
                    Playback.NewPlayingQueue(clickedPlaylist);
                    Playback.Start();
                    break;
                case Resource.Id.menu_more_playlist_insert:
                    ((IEnumerable<Track>)clickedPlaylist).Reverse().ForEach(Playback.InsertNextInQueue);
                    break;
                case Resource.Id.menu_more_playlist_append:
                    clickedPlaylist.ForEach(Playback.AddToQueue);
                    break;
                case Resource.Id.menu_more_playlist_remove:
                    Library.Playlists.Remove(clickedPlaylist);
                    this.RefreshData(from playlist in Library.Playlists
                                     orderby playlist.Title
                                     select playlist);
                    break;
            }
        }
    }
}