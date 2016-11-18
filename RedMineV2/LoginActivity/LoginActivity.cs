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
using Java.IO;
using RedMineV2.MainActivity;
using Android.Accounts;
using System.Threading.Tasks;
using Android.Content.PM;
using Android.Support.V7.App;

namespace RedMineV2.LoginActivity
{
    [Activity(Label = "RedMine", MainLauncher = true, Icon = "@drawable/icon",
        Theme = "@style/Theme.Material.Orange", ConfigurationChanges = ConfigChanges.Locale)]
    public class LoginActivity:AppCompatActivity
    {
        string login;
        string pass;
        EditText ETUsername;
        EditText ETPassword;
        CheckBox CBRememberMe;
        EditText ETDomen;
        Button BSignIn;
        TextView BRegister;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.login_layout);
            ETUsername = FindViewById<EditText>(Resource.Id.login);
            ETPassword = FindViewById<EditText>(Resource.Id.password);
            ETDomen = FindViewById<EditText>(Resource.Id.domen);
            CBRememberMe = FindViewById<CheckBox>(Resource.Id.remember);
            BSignIn = FindViewById<Button>(Resource.Id.sign_in);
            BRegister = FindViewById<TextView>(Resource.Id.register);
            //Retrieve login and password
            try
            {
                ETUsername.Text = CredentialsStore.Load()[0];
                ETPassword.Text = CredentialsStore.Load()[1];
                new RedMineManager(ETUsername.Text, ETPassword.Text);
                BSignIn_Click(null, null);
            }
            catch
            {

            }
            BSignIn.Click += BSignIn_Click;
            BRegister.Click += BRegister_Click;
        }

        private void BRegister_Click(object sender, EventArgs e)
        {
            Intent i = new Intent(this, typeof(RegisterActivity.RegisterActivity));
            i.PutExtra("Login", ETUsername.Text);
            i.PutExtra("Password", ETPassword.Text);
            StartActivity(i);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
        }

        private void BSignIn_Click(object sender, EventArgs e)
        {
            Intent i = new Intent(this, typeof(IssueActivity.IssueActivity));
            try
            {
                RedMineManager rm = new RedMineManager(ETUsername.Text, ETPassword.Text);
                //Save login and password
                if (CBRememberMe.Checked)
                    CredentialsStore.Save(ETUsername.Text, ETPassword.Text);
                StartActivity(i);
            }
            catch (System.Net.WebException o)
            {
                Toast.MakeText(this, o.Message, ToastLength.Short).Show();
            }
        }
    }
}