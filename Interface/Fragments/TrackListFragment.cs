using System.Collections.Generic;
using System.Linq;

using Android.Views;
using Android.Widget;

using Vibe.Music;

namespace Vibe.Interface.Fragments
{
    internal sealed class TrackListFragment : SelectablePopupListFragment<Track>
    {
        public TrackListFragment()
        {
        }
        
        public TrackListFragment(IEnumerable<Track> tracks) : base(tracks)
        {
        }
        
        protected override int FragmentViewId
        {
            get
            {
                return Resource.Layout.fragment_tracks;
            }
        }

        protected override int ListViewId
        {
            get
            {
                return Resource.Id.tracks;
            }
        }

        protected override int ListItemViewId
        {
            get
            {
                return Resource.Layout.item_track;
            }
        }

        protected override int MoreButtonId
        {
            get
            {
                return Resource.Id.item_track_more;
            }
        }

        protected override int MoreMenuId
        {
            get
            {
                return Resource.Menu.more_track;
            }
        }

        protected override int MoreMenuStyle
        {
            get
            {
                return Resource.Style.Menu_Vibe_Popup;
            }
        }

        protected override void OnListItemViewGet(Track track, int position, View? view)
        {
            base.OnListItemViewGet(track, position, view);
            
            ImageView image = view!.FindViewById<ImageView>(Resource.Id.item_track_image)!;
            image.SetImageBitmap(track.Album.Artwork);
            image.ClipToOutline = true;
            
            view.FindViewById<TextView>(Resource.Id.item_track_title)!.Text = track.Title;
            
            view.FindViewById<TextView>(Resource.Id.item_track_info)!.Text = $"{track.Artist.Name} · {track.Album.Title}";
        }

        protected override void OnListViewItemClick(Track track, int position, View? view)
        {
            IEnumerable<Track> tracks = this.Items
                .After(track)
                .Prepend(track);
            Playback.Start(tracks);
        }

        protected override void OnMorePopupItemClick(Track track, IMenuItem? clicked, PopupMenu popup)
        {
            switch (clicked?.ItemId)
            {
                case Resource.Id.more_track_play:
                    Playback.Start(track.Yield());
                    break;
                case Resource.Id.more_track_insert:
                    Playback.InsertNextInQueue(track);
                    break;
                case Resource.Id.more_track_append:
                    Playback.AddToQueue(track);
                    break;
            }
        }
    }
}