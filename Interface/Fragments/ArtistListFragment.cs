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
    internal sealed class ArtistListFragment : SelectableListFragment<Artist>
    {
        private readonly Dictionary<ImageButton, Artist> menuMappings = new();

        private ImageButton currentClickedMenu = null!;

        protected override int FragmentViewId
        {
            get
            {
                return Resource.Layout.fragment_list_artist;
            }
        }

        protected override int ListViewId
        {
            get
            {
                return Resource.Id.fragment_list_artist_list;
            }
        }

        protected override int ListItemViewId
        {
            get
            {
                return Resource.Layout.list_artist_item;
            }
        }

        protected override void OnListItemViewGet(Artist item, int position, View? view)
        {
            ImageView image = view!.FindViewById<ImageView>(Resource.Id.list_artist_item_image)!;
            Bitmap? artwork = item.Albums.First().Artwork;
            if (artwork is null)
            {
                image.SetPadding(0, 16, 0, 0);
                image.SetImageResource(Resource.Drawable.artist);
            }
            else
            {
                image.SetPadding(0, 0, 0, 0);
                image.SetImageBitmap(artwork);
            }
            
            view.FindViewById<TextView>(Resource.Id.list_artist_item_name)!.Text = item.Name;
            view.FindViewById<ImageButton>(Resource.Id.list_artist_item_more)!.Focusable = false;
            
            ImageButton more = view.FindViewById<ImageButton>(Resource.Id.list_artist_item_more)!;
            more.Focusable = false;
            this.menuMappings[more] = item;
            more.Click -= this.OnMoreMenuClick;
            more.Click += this.OnMoreMenuClick;
        }

        protected override void OnListViewItemClick(Artist item, int position, View? view)
        {
            Bundle extras = new();
            extras.PutLong("artistId", item.Id);
            Intent intent = new(Application.Context, typeof(ArtistActivity));
            intent.PutExtras(extras);
            this.Activity.StartActivity(intent);
        }

        private void OnMoreMenuClick(object source, EventArgs eventArgs)
        {
            ImageButton more = (ImageButton)source;
            PopupMenu popup = more.ShowPopupMenu(Resource.Menu.menu_more_artist, Resource.Style.Menu_Vibe_Popup, GravityFlags.End);
            this.currentClickedMenu = more;
            popup.MenuItemClick += this.OnCurrentClickedMenuMenuItemClick;
        }

        private void OnCurrentClickedMenuMenuItemClick(object source, PopupMenu.MenuItemClickEventArgs eventArgs)
        {
            Artist artist = this.menuMappings[this.currentClickedMenu];
            switch (eventArgs.Item?.ItemId)
            {
                case Resource.Id.menu_more_artist_play:
                    Playback.NewPlayingQueue(artist.Tracks);
                    Playback.Start();
                    break;
                case Resource.Id.menu_more_artist_insert:
                    artist.Tracks.Reverse().Execute(Playback.InsertNextInQueue);
                    break;
                case Resource.Id.menu_more_artist_append:
                    artist.Tracks.Execute(Playback.AddToQueue);
                    break;
            }
        }
    }
}