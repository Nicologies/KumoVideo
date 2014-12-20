
using Android.Content;
using Android.Net;

namespace aairvid.Utils
{
    public static class WifiStatus
    {
        public static bool IsWifiEnabled(this Context ctx)
        {
            var connectivityManager = (ConnectivityManager)ctx.GetSystemService(
                Context.ConnectivityService);

            var wifiState = connectivityManager.GetNetworkInfo(ConnectivityType.Wifi)
                .GetState();
            var wifiEnabled = wifiState == NetworkInfo.State.Connected;
            return wifiEnabled;
        }
    }
}