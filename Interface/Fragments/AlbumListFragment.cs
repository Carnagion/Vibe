using Android.Views;
using Android.Widget;

using Vibe.Music;

namespace Vibe.Interface.Fragments
{
    internal sealed class AlbumListFragment : SelectableListFragment<Album>
    {
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
            view!.FindViewById<TextView>(Resource.Id.list_album_item_title)!.Text = item.Title;
            view.FindViewById<TextView>(Resource.Id.list_album_item_info)!.Text = item.Artist.Name;
        }

        protected override void OnListViewItemClick(Album item, int position, View? view)
        {
            Playback.NewPlayingQueue(item.Tracks);
            Playback.Start();
        }
    }
}