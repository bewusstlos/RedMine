using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using RestSharp;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;
using Android.Content.PM;

namespace RedMineV2.RegisterActivity
{
    [Activity(Label = "Registration", MainLauncher = false, Icon = "@drawable/icon",
        Theme = "@style/Theme.Material.Orange", ConfigurationChanges = ConfigChanges.Locale)]
    class RegisterActivity:AppCompatActivity
    {
        Toolbar toolbar;

        EditText ETLogin;
        EditText ETPassword;
        EditText ETConfirmPassword;
        EditText ETFirstName;
        EditText ETLastName;
        EditText ETEmail;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.register_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.SetTitle(Resource.String.Registration);
            SetSupportActionBar(toolbar);
            Button BSubmit = new Button(this);
            toolbar.AddView(BSubmit);
            BSubmit.Click += BSubmit_Click;

            ETLogin = FindViewById<EditText>(Resource.Id.login);
            ETPassword = FindViewById<EditText>(Resource.Id.password);
            ETConfirmPassword = FindViewById<EditText>(Resource.Id.confirm_password);
            ETFirstName = FindViewById<EditText>(Resource.Id.first_name);
            ETLastName = FindViewById<EditText>(Resource.Id.last_name);
            ETEmail = FindViewById<EditText>(Resource.Id.email);
        }

        private void BSubmit_Click(object sender, EventArgs e)
        {
            RestClient client = new RestClient(@"http://dev.bidon-tech.com:65500/redmine");
            client.Authenticator = new RestSharp.Authenticators.HttpBasicAuthenticator(Intent.GetStringExtra("Login"), Intent.GetStringExtra("Password"));
            string uri = "/users.json";
            var request = new RestRequest(uri, Method.POST);
            request.RootElement = "user";
            request.AddJsonBody(new
            {
                user = new
                {
                    login = ETLogin.Text,
                    firstname = ETFirstName.Text,
                    lastname = ETLastName.Text,
                    password = ETPassword.Text,
                    mail = ETEmail.Text
                }
            });
            request.AddHeader("content-type", "application/json");
            RestResponse response = (RestResponse)client.Execute(request);
            Toast.MakeText(this, response.StatusDescription, ToastLength.Short).Show();
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
                Finish();
        }
    }
}