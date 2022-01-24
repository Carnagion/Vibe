using Android.Support.V4.App;

namespace Vibe.Utility.Extensions
{
    internal static class FragmentExtensions
    {
        internal static void Show(this Fragment fragment, bool instant = false)
        {
            FragmentTransaction? transaction = fragment.FragmentManager?.BeginTransaction()
                .SetReorderingAllowed(true)
                .Show(fragment);
            if (!instant)
            {
                transaction?.SetCustomAnimations(Resource.Animation.abc_slide_in_bottom, Resource.Animation.abc_slide_out_bottom);
            }
            transaction?.Commit();
        }
        
        internal static void Hide(this Fragment fragment, bool instant = false)
        {
            FragmentTransaction? transaction = fragment.FragmentManager?.BeginTransaction()
                .SetReorderingAllowed(true)
                .Hide(fragment);
            if (!instant)
            {
                transaction?.SetCustomAnimations(Resource.Animation.abc_slide_in_bottom, Resource.Animation.abc_slide_out_bottom);
            }
            transaction?.Commit();
        }
    }
}