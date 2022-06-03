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

        private static readonly string[] necessaryPermissions =
        {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.AccessMediaLocation,
            Manifest.Permission.WakeLock,
            Manifest.Permission.ForegroundService,
        };
        
        // Prevent the back button from canceling the startup process
        public override void OnBackPressed()
        {
        }
        
        // Check whether all permissions have been granted, and either close the app or start the main activity depending on the result
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode is not SplashActivity.permissionsRequestCode)
            {
                return;
            }
            
            if (grantResults.Contains(Permission.Denied))
            {
                ActivityManager? activityManager = (ActivityManager?)Application.Context.GetSystemService(SplashActivity.ActivityService);
                activityManager?.ClearApplicationUserData();
                this.FinishAffinity();
            }
            else
            {
                this.StartMainActivity();
            }
        }
        
        // On startup, check whether all permissions have been granted, and either request for permissions or start the main activity depending on the result
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            string[] deniedPermissions = (from permission in SplashActivity.necessaryPermissions
                                          where ContextCompat.CheckSelfPermission(this, permission) is Permission.Denied
                                          select permission).ToArray();
            if (deniedPermissions.Any())
            {
                ActivityCompat.RequestPermissions(this, deniedPermissions, SplashActivity.permissionsRequestCode);
            }
            else
            {
                this.StartMainActivity();
                
            }
        }

        // Load data into the library and start the main activity
        private void StartMainActivity()
        {
            new Task(() =>
            {
                Library.UpdateDatabase();
                this.StartActivity(new Intent(Application.Context, typeof(MainActivity)));
            }).Start();
        }
    }
}