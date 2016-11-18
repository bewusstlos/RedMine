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
    [Activity(Label = "Projects", MainLauncher = false, Icon = "@drawable/icon",
        Theme = "@style/Theme.Material.Orange", ConfigurationChanges = ConfigChanges.Locale)]
    class SetMembershipActivity:AppCompatActivity
    {
        Toolbar toolbar;
        UsersContainer usersContainer;
        List<Membership> memberships;
        LinearLayout LLRoot;

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
            SetContentView(Resource.Layout.set_membership_layout);
            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            ImageView BSave = new ImageView(this);
            LinearLayout LLControls = FindViewById<LinearLayout>(Resource.Id.right_controls);
            BSave.SetImageResource(Resource.Drawable.ic_save_white_18dp);
            LLControls.AddView(BSave);
            SetSupportActionBar(toolbar);

            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_arrow_back_white_18dp);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            LLRoot = FindViewById<LinearLayout>(Resource.Id.root);

            usersContainer = RedMineManager.Get<UsersContainer>("/users.json?limit=100&nometa=1");

            Spinner SUser = FindViewById<Spinner>(Resource.Id.user);
            var query = from r in usersContainer.users
                        select r.firstname + " " + r.lastname;
            ArrayAdapter assigneeAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleDropDownItem1Line, query.ToList());
            assigneeAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleDropDownItem1Line);
            SUser.Adapter = assigneeAdapter;

            Spinner SRole = FindViewById<Spinner>(Resource.Id.role);
            var statusAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleDropDownItem1Line, Enum.GetNames(typeof(RedMineManager.UserRoles)));
            SRole.Adapter = statusAdapter;
            statusAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleDropDownItem1Line);

            Button BAdd = FindViewById<Button>(Resource.Id.add);
            BAdd.Click += delegate
             {
                 int role = (int)Enum.Parse(typeof(RedMineManager.UserRoles), SRole.SelectedItem.ToString());
                 var queryUser = from r in usersContainer.users
                             where r.firstname + " " + r.lastname == SUser.SelectedItem.ToString()
                             select r.id;
                 RedMineManager.PostMemberShip("/projects/" + Intent.GetIntExtra("ProjectId", 0) + "/memberships.json", new
                 {
                     membership = new
                     {
                         user_id = queryUser.SingleOrDefault(),
                         role_ids = new int[]{ role }
                     }
                 });
                 Recreate();
             }; 

            memberships = RedMineManager.Get<List<Membership>>("/projects/"+Intent.GetIntExtra("ProjectId",0).ToString() + "/memberships.json?nometa=1", "memberships");
            SetMemberShipsLayout();
        }

        public void SetMemberShipsLayout()
        {
            
            foreach (var membership in memberships)
            {
                RelativeLayout LLMemberShip = new RelativeLayout(this);
                RelativeLayout.LayoutParams lpForLLMemberShip = new RelativeLayout.LayoutParams(-1,-2);
                lpForLLMemberShip.SetMargins(0, 5, 0, 0);
                LLMemberShip.SetBackgroundResource(Resource.Color.colorBackgroundCard);

                LinearLayout LLRole = new LinearLayout(this);
                LLRole.SetPadding(10, 0, 0, 0);
                LLRole.Orientation = Orientation.Horizontal;

                TextView TVUsername = new TextView(this);
                TVUsername.SetTextColor(Android.Graphics.Color.Black);
                TVUsername.Text = membership.user.name +" - ";
                LLRole.AddView(TVUsername);

                TextView TVRoles = new TextView(this);
                TVRoles.SetTextColor(Resources.GetColor(Resource.Color.colorPrimary));
                foreach(var role in membership.roles)
                {
                    TVRoles.Text += role.name;
                }

                LLRole.AddView(TVRoles);

                LLMemberShip.AddView(LLRole);

                ImageView BDelete = new ImageView(this);
                RelativeLayout.LayoutParams lpForBDelete = new RelativeLayout.LayoutParams(-2, -2);
                lpForBDelete.AddRule(LayoutRules.AlignParentRight);
                lpForBDelete.RightMargin = 5;
                BDelete.SetImageResource(Resource.Drawable.ic_delete_black_18dp);
                BDelete.Click += delegate
                {
                    RedMineManager.Delete("/memberships/" + membership.id + ".json");
                    memberships.Remove(membership);
                    Recreate();
                };
                LLMemberShip.AddView(BDelete, lpForBDelete);
                LLRoot.AddView(LLMemberShip, lpForLLMemberShip);
            }
        }
    }
}