using System.Linq;
using System.Threading.Tasks;

using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;

using Vibe.Music;

namespace Vibe.Interface.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.Vibe.Splash", MainLauncher = true, NoHistory = true)]
    internal sealed class SplashActivity : AppCompatActivity
    {
        private const int permissionsRequestCode = 9;

        public override void OnBackPressed()
        {
        }
        
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode is not SplashActivity.permissionsRequestCode)
            {
                return;
            }
            
            if (grantResults.Any((permission) => permission is Permission.Denied))
            {
                ActivityManager? activityManager = (ActivityManager?)Application.Context.GetSystemService(SplashActivity.ActivityService);
                activityManager?.ClearApplicationUserData();
                this.FinishAffinity();
            }
            else
            {
                this.ExecuteTasks();
            }
        }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            string[] necessaryPermissions =
            {
                Manifest.Permission.ReadExternalStorage,
                Manifest.Permission.WriteExternalStorage,
                Manifest.Permission.AccessMediaLocation,
                Manifest.Permission.WakeLock,
                Manifest.Permission.ForegroundService,
            };
            string[] deniedPermissions = (from string permission in necessaryPermissions
                                          where ContextCompat.CheckSelfPermission(this, permission) is Permission.Denied
                                          select permission).ToArray();
            if (deniedPermissions.Any())
            {
                ActivityCompat.RequestPermissions(this, deniedPermissions, SplashActivity.permissionsRequestCode);
            }
            else
            {
                this.ExecuteTasks();
            }
        }

        private void ExecuteTasks()
        {
            new Task(() =>
            {
                Library.UpdateDatabase();
                this.StartActivity(new Intent(Application.Context, typeof(MainActivity)));
            }).Start();
        }
    }
}