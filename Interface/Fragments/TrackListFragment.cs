using System.Linq;

using Android.Graphics;
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
        }

        protected override void OnListViewItemClick(Track item, int position, View? view)
        {
            Playback.NewPlayingQueue(this.Items.After(item).Prepend(item));
            Playback.Start();
        }
    }
}