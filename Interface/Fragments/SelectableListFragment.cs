using System.Collections.Generic;

using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace Vibe.Interface.Fragments
{
    internal abstract class SelectableListFragment<T> : Fragment
    {
        private ListView listView = null!;

        private LayoutInflater layoutInflater = null!;
        
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

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.layoutInflater = inflater;
            
            View fragmentView = inflater.Inflate(this.FragmentViewId, container, false)!;

            this.listView = fragmentView.FindViewById<ListView>(this.ListViewId)!;
            this.listView.Adapter = new SelectableListAdapter<T>(this);
            this.listView.NestedScrollingEnabled = true;
            this.listView.ItemClick += this.OnListViewItemClick;

            this.RetainInstance = true;

            return fragmentView;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            this.listView.ItemClick -= this.OnListViewItemClick;
        }

        protected virtual void OnListViewItemClick(T item, int position, View? view)
        {
        }

        protected virtual void OnListItemViewGet(T item, int position, View? view)
        {
        }

        private void OnListViewItemClick(object source, AdapterView.ItemClickEventArgs eventArgs)
        {
            this.SelectedItem = this.Items[eventArgs.Position];
            this.OnListViewItemClick(this.SelectedItem!, eventArgs.Position, eventArgs.View);
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

            public override View? GetView(int position, View? convertView, ViewGroup? parent)
            {
                View? view = convertView ?? this.listFragment.layoutInflater.Inflate(this.listFragment.ListItemViewId, parent, false);
                this.listFragment.OnListItemViewGet(this[position], position, view);
                return view;
            }

            public override long GetItemId(int position)
            {
                return position;
            }
        }
    }
}