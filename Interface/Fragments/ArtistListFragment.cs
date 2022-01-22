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
    internal sealed class ArtistListFragment : SelectableListFragment<Artist>
    {
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
            view!.FindViewById<ImageView>(Resource.Id.list_artist_item_image)!.SetImageBitmap(item.Albums.First().Artwork);
            view.FindViewById<TextView>(Resource.Id.list_artist_item_name)!.Text = item.Name;
        }

        protected override void OnListViewItemClick(Artist item, int position, View? view)
        {
            Bundle extras = new();
            extras.PutLong("artistId", item.Id);
            Intent intent = new(Application.Context, typeof(ArtistActivity));
            intent.PutExtras(extras);
            this.Activity.StartActivity(intent);
        }
    }
}