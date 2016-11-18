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
using Android.Support.V7.App;

namespace RedMineV2.SettingsActivity
{
    [Activity(Label = "Settings", MainLauncher = false, Icon = "@drawable/icon",
        Theme = "@style/Theme.Material.Orange")]
    class SettingsActivity:AppCompatActivity
    {
        EditText ETDomen;
        Button BSetDomen;
        Button BLogOut;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.settings_layout);

            ETDomen = FindViewById<EditText>(Resource.Id.domen);
            BSetDomen = FindViewById<Button>(Resource.Id.set_domen);
            BLogOut = FindViewById<Button>(Resource.Id.log_out);

            ETDomen.Text = Intent.GetStringExtra("Domen");
            BSetDomen.Click += BSetDomen_Click;
            BLogOut.Click += BLogOut_Click;
        }

        private void BSetDomen_Click(object sender, EventArgs e)
        {
            
        }

        private void BLogOut_Click(object sender, EventArgs e)
        {
            
        }
    }
}