
using Android.App;
using Android.Content;
using Android.OS;

namespace aairvid
{
    [Activity(Theme = "@style/Theme.Splash"
        , MainLauncher = true
        , NoHistory = true
        )]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Intent intent = new Intent(this, typeof(MainActivity));
            StartActivityForResult(intent, -1);
            this.Finish();
        }
    }
}