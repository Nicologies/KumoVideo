
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

            var activeNetwork = connectivityManager.ActiveNetworkInfo;
            return activeNetwork.Type == ConnectivityType.Wifi && activeNetwork.IsConnected;
        }
    }
}