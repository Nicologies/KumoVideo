
using Android.App;
using Android.Content;
using Android.OS;
using IO.Vov.Vitamio;

namespace aairvid
{
    [Activity(Theme = "@style/Theme.Splash"
        , MainLauncher = true
        , ScreenOrientation= Android.Content.PM.ScreenOrientation.SensorLandscape
        , NoHistory = true
        )]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            if (!LibsChecker.CheckVitamioLibs(this))
                return;

            Intent intent = new Intent(this, typeof(MainActivity));
            StartActivityForResult(intent, -1);
            this.Finish();
        }
    }
}