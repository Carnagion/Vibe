using System.Linq;

using Android.Views;
using Android.Widget;

using Vibe.Music;

namespace Vibe.Interface.Fragments
{
    internal sealed class TrackListFragment : SelectableListFragment<Track>
    {
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
            view!.FindViewById<TextView>(Resource.Id.list_track_item_title)!.Text = item.Title;
            view.FindViewById<TextView>(Resource.Id.list_track_item_info)!.Text = $"{item.Album.Title} · {item.Artist.Name}";
        }

        protected override void OnListViewItemClick(Track item, int position, View? view)
        {
            Playback.NewPlayingQueue(item.Yield().Concat(Library.Tracks));
            Playback.Start();
        }
    }
}