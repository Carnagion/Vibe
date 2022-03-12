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
    internal sealed class TrackListFragment : SelectableListFragment<Track>
    {
        public TrackListFragment(IEnumerable<Track> items) : base(items)
        {
        }
        
        private readonly Dictionary<ImageButton, Track> menuMappings = new();

        private ImageButton currentClickedMenu = null!;

        protected override int FragmentViewId
        {
            get
            {
                return Resource.Layout.fragment_list_track;
            }
        }

        protected override int ListViewId
        {
            get
            {
                return Resource.Id.fragment_list_track_list;
            }
        }

        protected override int ListItemViewId
        {
            get
            {
                return Resource.Layout.list_track_item;
            }
        }

        protected override void OnListItemViewGet(Track item, int position, View? view)
        {
            ImageView image = view!.FindViewById<ImageView>(Resource.Id.list_track_item_image)!;
            Bitmap? artwork = item.Album.Artwork;
            if (artwork is null)
            {
                image.SetPadding(0, 16, 0, 0);
                image.SetImageResource(Resource.Drawable.track);
            }
            else
            {
                image.SetPadding(0, 0, 0, 0);
                image.SetImageBitmap(artwork);
            }
            
            view.FindViewById<TextView>(Resource.Id.list_track_item_title)!.Text = item.Title;
            view.FindViewById<TextView>(Resource.Id.list_track_item_info)!.Text = $"{item.Album.Title} · {item.Artist.Name}";
            
            ImageButton more = view.FindViewById<ImageButton>(Resource.Id.list_track_item_more)!;
            more.Focusable = false;
            this.menuMappings[more] = item;
            more.Click -= this.OnMoreClick;
            more.Click += this.OnMoreClick;
        }

        protected override void OnListViewItemClick(Track item, int position, View? view)
        {
            Playback.NewPlayingQueue(this.Items.After(item).Prepend(item));
            Playback.Start();
        }

        private void OnMoreClick(object source, EventArgs eventArgs)
        {
            ImageButton more = (ImageButton)source;
            PopupMenu popup = more.ShowPopupMenu(Resource.Menu.menu_more_track, Resource.Style.Menu_Vibe_Popup, GravityFlags.End);
            this.currentClickedMenu = more;
            popup.MenuItemClick += this.OnCurrentClickedMenuMenuItemClick;
        }

        private void OnCurrentClickedMenuMenuItemClick(object source, PopupMenu.MenuItemClickEventArgs eventArgs)
        {
            Track track = this.menuMappings[this.currentClickedMenu];
            switch (eventArgs.Item?.ItemId)
            {
                case Resource.Id.menu_more_track_insert:
                    Playback.InsertNextInQueue(track);
                    break;
                case Resource.Id.menu_more_track_append:
                    Playback.AddToQueue(track);
                    break;
                case Resource.Id.menu_more_track_add_to_playlist:
                    Bundle bundle = new();
                    bundle.PutLong("trackId", track.Id);
                    Intent intent = new(Application.Context, typeof(AddToPlaylistActivity));
                    intent.PutExtras(bundle);
                    this.Activity.StartActivity(intent);
                    break;
            }
        }
    }
}