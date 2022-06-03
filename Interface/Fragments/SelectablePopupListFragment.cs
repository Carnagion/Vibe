using System;
using System.Collections.Generic;

using Android.Views;
using Android.Widget;

using Vibe.Utility.Extensions;

namespace Vibe.Interface.Fragments
{
    internal abstract class SelectablePopupListFragment<T> : SelectableListFragment<T>
    {
        public SelectablePopupListFragment()
        {
        }

        public SelectablePopupListFragment(IEnumerable<T> items) : base(items)
        {
        }

        private readonly Dictionary<ImageButton, T> itemMoreAssociation = new();

        private ImageButton currentMore = null!;

        protected abstract int MoreButtonId
        {
            get;
        }

        protected abstract int MoreMenuId
        {
            get;
        }

        protected abstract int MoreMenuStyle
        {
            get;
        }

        protected override void OnListItemViewGet(T item, int position, View? view)
        {
            ImageButton more = view!.FindViewById<ImageButton>(this.MoreButtonId)!;
            more.Focusable = false; // This must be set in code as the XML attribute is bugged. Well done, Android
            this.itemMoreAssociation[more] = item;
            more.Click -= this.OnMoreClick;
            more.Click += this.OnMoreClick;
        }

        protected abstract void OnMorePopupItemClick(T item, IMenuItem? clicked, PopupMenu popup);

        private void OnMoreClick(object source, EventArgs eventArgs)
        {
            this.currentMore = (ImageButton)source;
            this.currentMore.ShowPopupMenu(this.MoreMenuId, this.MoreMenuStyle, GravityFlags.End).MenuItemClick += this.OnCurrentMenuPopupClick;
        }

        private void OnCurrentMenuPopupClick(object source, PopupMenu.MenuItemClickEventArgs eventArgs)
        {
            T item = this.itemMoreAssociation[this.currentMore];
            PopupMenu popup = (PopupMenu)source;
            this.OnMorePopupItemClick(item, eventArgs.Item, popup);
        }
    }
}