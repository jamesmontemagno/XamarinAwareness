using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Gms.Common.Apis;
using Android.Gms.Awareness;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
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
                  await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
              };

            var navigationButton = FindViewById<Button>(Resource.Id.button_awareness);

            navigationButton.Click += async (sender, args) =>
              {
                  var result = await Awareness.SnapshotApi.GetDetectedActivityAsync(client);
                  var info = "Probably: ";
                  if (result.Status.IsSuccess)
                  {
                      var ar = result.ActivityRecognitionResult;
                      var probablyActivity = ar.MostProbableActivity;

                      info += probablyActivity.ToString();
                  }
                  else
                  {
                      info += "cant' get probably doing.";
                  }

                  var headPhones = await Awareness.SnapshotApi.GetHeadphoneStateAsync(client);

                  if (headPhones.Status.IsSuccess)
                      info += "\n Headphones:" + (headPhones.HeadphoneState.State == HeadphoneState.PluggedIn ? "plugged in" : "not plugged in");
                  else
                      info += "\n Can't Get Headphones";

                  var location = await Awareness.SnapshotApi.GetLocationAsync(client);

                  if (location.Status.IsSuccess)
                      info += "\n Location:" + location.Location.Latitude + "," + location.Location.Longitude;
                  else
                      info += "\n Can't Get Location";

                  var places = await Awareness.SnapshotApi.GetPlacesAsync(client);
                  if (places.Status.IsSuccess)
                  {
                      foreach (var place in places.PlaceLikelihoods)
                          info += "\n places:" + place.Place.NameFormatted.ToString() + " likelihood:" + place.Likelihood;
                  }
                  else
                      info += "\n Can't Get places";

                  var weather = await Awareness.SnapshotApi.GetWeatherAsync(client);
                  if (weather.Status.IsSuccess)
                      info += "\n weather:" + weather.Weather.ToString();
                  else
                      info += "\n Can't Get weather";

                  text.Text = info;
              };


            client = await new GoogleApiClient.Builder(this)
                .AddApi(Awareness.Api)
                .EnableAutoManage(this, (r) =>
                {
                })
                .BuildAndConnectAsync((i) =>
                {

                });
            

            SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            SupportActionBar.SetHomeButtonEnabled(false);

        }
    }
}

