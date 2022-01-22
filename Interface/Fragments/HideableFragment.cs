using Android.Support.V4.App;

namespace Vibe.Interface.Fragments
{
    internal abstract class HideableFragment : Fragment
    {
        public void Show(bool instant = false)
        {
            FragmentTransaction? transaction = this.FragmentManager?.BeginTransaction()
                .SetReorderingAllowed(true)
                .Show(this);
            if (!instant)
            {
                transaction?.SetCustomAnimations(Resource.Animation.abc_slide_in_bottom, Resource.Animation.abc_slide_out_bottom);
            }
            transaction?.Commit();
        }
        
        public void Hide(bool instant = false)
        {
            FragmentTransaction? transaction = this.FragmentManager?.BeginTransaction()
                .SetReorderingAllowed(true)
                .Hide(this);
            if (!instant)
            {
                transaction?.SetCustomAnimations(Resource.Animation.abc_slide_in_bottom, Resource.Animation.abc_slide_out_bottom);
            }
            transaction?.Commit();
        }
    }
}