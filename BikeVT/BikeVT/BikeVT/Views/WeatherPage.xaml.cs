﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions.Abstractions;
using System.Collections.ObjectModel;
using Plugin.Permissions;





namespace BikeVT.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WeatherPage : ContentPage
    {
        RestService _restService;
        public WeatherPage()
        {
            InitializeComponent();
            _restService = new RestService();
        }

        async void OnGetWeatherButtonClicked(object sender, EventArgs e)
        {
            //TODO: probably want to check for location here if we aren't making sure it isn't enabled elsewhere
            string req_uri = await GenerateRequestUri(Constants.OpenWeatherMapEndpoint);
                WeatherData weatherData = await _restService.GetWeatherData(req_uri);
                BindingContext = weatherData;
        }

        async Task<string> GenerateRequestUri(string endpoint)
        {

            //https://github.com/jamesmontemagno/PermissionsPlugin
            try
            {
                Plugin.Permissions.Abstractions.PermissionStatus device_status = await CrossPermissions.Current.CheckPermissionStatusAsync<LocationPermission>();

                if (device_status != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                {
                    await DisplayAlert("Location required", "Location permissions are required to to look up local weather.", "OK");


                    // if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location))
                    // {
                    //     //Haven't looked into what makes this if-statement true. 
                    //     // We shouldn't need this since we've already displayed an alert explaining why we need permissions
                    //     await DisplayAlert("Need location", "Gunna need that location", "OK");
                    // }

                    //Open prompt to settings:
                    await Utils.CheckPermissions(new LocationPermission());

                    //The next lines dont wait for you to return from the Settings app. It'll think you denied permissions
                    Console.WriteLine("refreshing");
                    device_status = await CrossPermissions.Current.RequestPermissionAsync<LocationPermission>();
                }

                if (device_status == Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                {
                    //Query permission

                    var c_locator = CrossGeolocator.Current;
                    var test_loc = Task.Run(() => c_locator.GetPositionAsync(TimeSpan.FromSeconds(.5))).Result;

                    string requestUri = endpoint;
                    requestUri += "?lat=" + test_loc.Latitude;
                    requestUri += "&lon=" + test_loc.Longitude;
                    requestUri += "&units=imperial"; // or units=metric
                    requestUri += $"&APPID={Constants.OpenWeatherMapAPIKey}";
                    return requestUri;
                }
                else // if (device_status != Plugin.Permissions.Abstractions.PermissionStatus.Unknown)
                {
                    //location denied.
                    // or permission is unknown. 

                    await DisplayAlert("Permission denied", "Location permissions have been denied. " +
                        "Using Blacksburg as the default location.\n" +
                        "If you just allowed location, press the 'Get Weather' button again.\n", "OK");
                    Console.WriteLine("Get weather location Default action");

                    string uri = endpoint;
                    uri += "?lat=" + "37.229342";
                    uri += "&lon=" + "-80.413928";
                    uri += "&units=imperial"; // or units=metric
                    uri += $"&APPID={Constants.OpenWeatherMapAPIKey}";
                    return uri;
                }

            }
            catch (Exception ex)
            {

                //Something went wrong
                await DisplayAlert("Error getting location","Something went wrong. Defaulting location to Blacksburg.", "OK");
                Console.WriteLine("Something went wrong when getting location for weather. Defaulting to Blacksburg.\n" + ex);

                string uri = endpoint;
                uri += "?lat=" + "37.229342";
                uri += "&lon=" + "-80.413928";
                uri += "&units=imperial"; // or units=metric
                uri += $"&APPID={Constants.OpenWeatherMapAPIKey}";
                return uri;
            }
        }
    }

}