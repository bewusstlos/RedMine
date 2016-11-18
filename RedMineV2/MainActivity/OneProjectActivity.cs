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
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Content.PM;

namespace RedMineV2.MainActivity
{
    [Activity(Label = "Project", MainLauncher = false, Icon = "@drawable/icon",
        Theme = "@style/Theme.Material.Orange", ConfigurationChanges = ConfigChanges.Locale)]
    public class OneProjectActivity : AppCompatActivity
    {
        Toolbar toolbar;
        LinearLayout LLRootRoles;
        List<Membership> memberships;

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.one_project_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.SetTitle(Resource.String.Project);
            SetSupportActionBar(toolbar);

            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_arrow_back_white_18dp);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            LLRootRoles = FindViewById<LinearLayout>(Resource.Id.root_roles);

             memberships = RedMineManager.Get<List<Membership>>("/projects/" + Intent.GetIntExtra("ProjectId", 0) + "/memberships.json?nometa=1", "memberships");

            foreach (var membership in memberships)
            {

                LinearLayout LLMemberShip = new LinearLayout(this);
                LLMemberShip.Orientation = Orientation.Horizontal;

                TextView TVUser = new TextView(this);
                if (membership.user != null)
                    TVUser.Text = membership.user.name + " - " ?? "";
                TVUser.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Black));
                LLMemberShip.AddView(TVUser);

                TextView TVRole = new TextView(this);
                foreach (var role in membership.roles)
                {
                    TVRole.SetTextColor(Resources.GetColor(Resource.Color.colorPrimary));
                    TVRole.Text += role.name + " ";
                }
                LLMemberShip.AddView(TVRole);
                LLRootRoles.AddView(LLMemberShip);
            }

            Button BSetChange = new Button(this);
            BSetChange.Text = Resources.GetString(Resource.String.SetChange);
            BSetChange.Click += delegate
            {
                Intent i = new Intent(this, typeof(SetMembershipActivity));
                i.PutExtra("ProjectId", Intent.GetIntExtra("ProjectId", 0));
                StartActivityForResult(i, 0);
            };
            LLRootRoles.AddView(BSetChange);
            RelativeLayout RLIssues = FindViewById<RelativeLayout>(Resource.Id.issues);
            RLIssues.Click += delegate
            {
                Intent i = new Intent(this, typeof(IssueActivity.IssueActivity));
                i.PutExtra("Kind", "Project Issues");
                i.PutExtra("ProjectId", Intent.GetIntExtra("ProjectId", 0));
                StartActivity(i);
            };
            TextView TVCountOfIssues = FindViewById<TextView>(Resource.Id.count_of_issues);
            TVCountOfIssues.Text = RedMineManager.Get<IssuesContainer>("/issues.json?project_id=" + Intent.GetIntExtra("ProjectId", 0)).total_count.ToString();
        }
    }
}