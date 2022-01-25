using Android.App;
using Android.Views;
using Android.Widget;

namespace Vibe.Utility.Extensions
{
    internal static class ViewExtensions
    {
        internal static PopupMenu ShowPopupMenu(this View view, int id, int theme, GravityFlags gravity = default)
        {
            ContextThemeWrapper wrapper = new(Application.Context, theme);
            PopupMenu popup = new(wrapper, view, gravity);
            popup.MenuInflater?.Inflate(id, popup.Menu);
            popup.Show();
            return popup;
        }
    }
}