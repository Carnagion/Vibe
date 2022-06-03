using System.Collections.Generic;

using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace Vibe.Interface.Fragments
{
    internal abstract class SelectableListFragment<T> : Fragment
    {
        public SelectableListFragment()
        {
        }

        public SelectableListFragment(IEnumerable<T> items)
        {
            this.Items.AddRange(items);
        }

        private ListView listView = null!;

        public List<T> Items
        {
            get;
        } = new();

        public T? SelectedItem
        {
            get;
            private set;
        }
        
        protected abstract int FragmentViewId
        {
            get;
        }

        protected abstract int ListViewId
        {
            get;
        }
        
        protected abstract int ListItemViewId
        {
            get;
        }

        // Notify the adapter that changes have been made to the data so it can redraw all views
        public void NotifyDataSetChanged()
        {
            ((SelectableListAdapter<T>)this.listView.Adapter!).NotifyDataSetChanged();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View fragmentView = inflater.Inflate(this.FragmentViewId, container, false)!;

            this.RetainInstance = true;

            // Setup the list view
            this.listView = fragmentView.FindViewById<ListView>(this.ListViewId)!;
            this.listView.Adapter = new SelectableListAdapter<T>(this);
            this.listView.NestedScrollingEnabled = true;

            this.listView.ItemClick += this.OnListViewItemClick;
            this.listView.ItemLongClick += this.OnListViewItemLongClick;

            return fragmentView;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            this.listView.ItemClick -= this.OnListViewItemClick;
            this.listView.ItemLongClick -= this.OnListViewItemLongClick;
        }

        // Called when the adapter is creating/drawing an item view
        protected abstract void OnListItemViewGet(T item, int position, View? view);

        // Called when an item has been clicked
        protected virtual void OnListViewItemClick(T item, int position, View? view)
        {
        }

        // Called when an item has been long clicked
        protected virtual void OnListViewItemLongClick(T item, int position, View? view)
        {
        }

        private void OnListViewItemClick(object source, AdapterView.ItemClickEventArgs eventArgs)
        {
            this.SelectedItem = this.Items[eventArgs.Position];
            this.OnListViewItemClick(this.SelectedItem!, eventArgs.Position, eventArgs.View);
        }

        private void OnListViewItemLongClick(object source, AdapterView.ItemLongClickEventArgs eventArgs)
        {
            this.OnListViewItemClick(this.Items[eventArgs.Position], eventArgs.Position, eventArgs.View);
        }

#pragma warning disable 693
        private sealed class SelectableListAdapter<T> : BaseAdapter<T>
#pragma warning restore 693
        {
            internal SelectableListAdapter(SelectableListFragment<T> listFragment)
            {
                this.listFragment = listFragment;
            }

            private readonly SelectableListFragment<T> listFragment;

            public override int Count
            {
                get
                {
                    return this.listFragment.Items.Count;
                }
            }

            public override T this[int position]
            {
                get
                {
                    return this.listFragment.Items[position];
                }
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override View? GetView(int position, View? convertView, ViewGroup? parent)
            {
                View? view = convertView ?? this.listFragment.LayoutInflater.Inflate(this.listFragment.ListItemViewId, parent, false);
                this.listFragment.OnListItemViewGet(this[position], position, view);
                return view;
            }
        }
    }
}