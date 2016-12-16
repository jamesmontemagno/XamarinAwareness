using Android.App;
using Android.Widget;
using Android.OS;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;


using Android.Gms.Common.Apis;
using Android.Gms.Awareness;
using Android.Gms.Awareness.State;
using Android.Gms.Extensions;

namespace XamarinAwareness
{
    [Activity(Label = "XamarinAwareness", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : BaseActivity
    {

        protected override int LayoutResource
        {
            get { return Resource.Layout.main; }
        }
        int count = 1;
        GoogleApiClient client;
        protected async override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var text = FindViewById<TextView>(Resource.Id.text_awareness);

            // Get our button from the layout resource,
            // and attach an event to it
            var clickButton = FindViewById<Button>(Resource.Id.button_permissions);

            clickButton.Click += async (sender, args) =>
              {
                  var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                  if (status != PermissionStatus.Granted)
                  {
                      var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
                      if (results[Permission.Location] != PermissionStatus.Granted)
                          return; //handle rejection
                  }
              };

            var button = FindViewById<Button>(Resource.Id.button_awareness);
            

            button.Click += async (s, o) =>
              {
                  if(client == null)
                  {
                   Toast.MakeText (this, "Make sure you have registred correctly with Google Play API console.", ToastLength.Long).Show();
                   return;
                  }
                  var result = await Awareness.SnapshotApi
                                    .GetDetectedActivityAsync(client);

                  var info = string.Empty;

                  if (result?.Status?.IsSuccess ?? false)
                  {
                      var probablyActivity = result?.ActivityRecognitionResult
                                                   ?.MostProbableActivity;

                      info = probablyActivity?.ToString() ?? "No activity"; 
                  }

                  text.Text = info;
                  var headPhones = await Awareness.SnapshotApi.GetHeadphoneStateAsync(client);

                  if (headPhones?.Status?.IsSuccess ?? false)
                      info += "\n Headphones:" + (headPhones.HeadphoneState.State == HeadphoneState.PluggedIn ? "plugged in" : "not plugged in");
                  else
                      info += "\n Can't Get Headphones";

                  text.Text = info;
                  var location = await Awareness.SnapshotApi.GetLocationAsync(client);

                  if (location?.Status?.IsSuccess ?? false)
                      info += "\n Location:" + location.Location.Latitude + "," + location.Location.Longitude;
                  else
                      info += "\n Can't Get Location";

                  text.Text = info;
                  var places = await Awareness.SnapshotApi.GetPlacesAsync(client);
                  if (places?.Status?.IsSuccess ?? false && places.PlaceLikelihoods != null)
                  {
                      foreach (var place in places.PlaceLikelihoods)
                          info += "\n places:" + place?.Place?.NameFormatted.ToString() + " likelihood:" + place.Likelihood;
                  }
                  else
                      info += "\n Can't Get places";

                  text.Text = info;
                  var weather = await Awareness.SnapshotApi.GetWeatherAsync(client);
                  if (weather?.Status?.IsSuccess ?? false && weather.Weather != null)
                  {
                      info += $"\n Temperature: {weather.Weather.GetTemperature(1)} ";
                      info += $"\n Humidity: {weather.Weather.Humidity}%";
                      info += $"\n Feels Like: {weather.Weather.GetFeelsLikeTemperature(1)} ";
                      info += $"\n Dew Point: {weather.Weather.GetDewPoint(1)} ";
                  }
                  else
                      info += "\n Can't Get weather";

                  text.Text = info;
              };


            client = await new GoogleApiClient.Builder(this)
                .AddApi(Awareness.Api)
                .AddConnectionCallbacks(() =>
                {
                    text.Text = "Connected";
                })
                .BuildAndConnectAsync((i) =>
                {
                });
            

            SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            SupportActionBar.SetHomeButtonEnabled(false);

        }
    }
}

