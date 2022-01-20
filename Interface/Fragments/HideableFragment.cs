using Android.Support.V4.App;

namespace Vibe.Interface.Fragments
{
    internal abstract class HideableFragment : Fragment
    {
        public void Show()
        {
            this.FragmentManager?.BeginTransaction()
                .SetReorderingAllowed(true)
                .SetCustomAnimations(Resource.Animation.abc_slide_in_bottom, Resource.Animation.abc_slide_out_bottom)
                .Show(this)
                .Commit();
        }
        
        public void Hide()
        {
            this.FragmentManager?.BeginTransaction()
                .SetReorderingAllowed(true)
                .SetCustomAnimations(Resource.Animation.abc_slide_in_bottom, Resource.Animation.abc_slide_out_bottom)
                .Hide(this)
                .Commit();
        }
    }
}