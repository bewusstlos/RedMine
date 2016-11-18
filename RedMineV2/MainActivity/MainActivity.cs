using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using System.Threading.Tasks;
using static RedMineV2.MainActivity.IssuesContainer;
using System.Threading;
using Android.Runtime;
using Android.Content.PM;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using ActionBar = Android.Support.V7.App.ActionBar;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Support.V7.App;

namespace RedMineV2.MainActivity
{
    [Activity(Label = "Projects", MainLauncher = false, Icon = "@drawable/icon",
        Theme = "@style/Theme.Material.Orange", ConfigurationChanges = ConfigChanges.Locale)]
    public class MainActivity : AppCompatActivity
    {
        Toolbar toolbar;
        System.Random rnd;
        DrawerLayout drawerLayout;
        NavigationView navigationView;
        ExpandableListView LVProjects;
        ExpandableListViewAdapter mAdapter;
        List<int> projectsGroups;
        Dictionary<int, List<int>> projectsItems;
        LinearLayout LLRoot;
        TextView TVUserFullName;
        ProjectsContainer projectsContainer;

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            this.Recreate();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (drawerLayout.IsDrawerOpen(Android.Support.V4.View.GravityCompat.Start))
                        drawerLayout.CloseDrawers();
                    else
                        drawerLayout.OpenDrawer(Android.Support.V4.View.GravityCompat.Start);
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.main_layout);
            rnd = new System.Random();
            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.SetTitle(Resource.String.Projects);
            SetSupportActionBar(toolbar);

            //Enable support action bar to display hamburger
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu_white_18dp);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);

            LayoutInflater inflater = (LayoutInflater)this.GetSystemService(Context.LayoutInflaterService);
            View v = inflater.Inflate(Resource.Layout.drawer_header, null);
            TVUserFullName = v.FindViewById<TextView>(Resource.Id.user_full_name);
            TVUserFullName.SetBackgroundColor(Android.Graphics.Color.Argb(200, rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255)));

            TVUserFullName.Text = RedMineManager.currUser.firstname + " " + RedMineManager.currUser.lastname ?? "Firstname Lastname";
            navigationView.AddHeaderView(v);
            navigationView.NavigationItemSelected += (sender, e) =>
            {
                e.MenuItem.SetChecked(true);
                switch (e.MenuItem.ItemId)
                {
                    case Resource.Id.nav_my_issues:
                        Intent i0 = new Intent(this, typeof(IssueActivity.IssueActivity));
                        i0.PutExtra("Kind", "My Issues");
                        StartActivity(i0);
                        break;
                    case Resource.Id.nav_projects:
                        if (RedMineManager.currUser.status == 1)
                        {
                            Intent i4 = new Intent(this, typeof(MainActivity));
                            StartActivity(i4);
                        }
                        else
                            Toast.MakeText(this, Resources.GetString(Resource.String.PermissionError), ToastLength.Short).Show();
                        break;
                    case Resource.Id.settings:
                        Intent i = new Intent(this, typeof(SettingsActivity.SettingsActivity));
                        i.PutExtra("Domen", RedMineManager.client.BaseUrl.ToString());
                        StartActivity(i);
                        break;
                    case Resource.Id.administration:
                        if (RedMineManager.currUser.status == 1)
                        {
                            Intent i2 = new Intent(this, typeof(VerifiedUserActivity));
                            StartActivity(i2);
                        }
                        else
                            Toast.MakeText(this, Resources.GetString(Resource.String.PermissionError), ToastLength.Short).Show();
                        break;
                    case Resource.Id.log_out:
                        LoginActivity.CredentialsStore.Delete();
                        Intent i3 = new Intent(this, typeof(LoginActivity.LoginActivity));
                        Finish();
                        StartActivity(i3);
                        break;
                    case Resource.Id.exit:
                        Finish();
                        break;
                }
                drawerLayout.CloseDrawers();
            };

            LinearLayout LLControls = FindViewById<LinearLayout>(Resource.Id.right_controls);
            Thread load = new Thread(() =>
            {
                LVProjects = FindViewById<ExpandableListView>(Resource.Id.list_projects);
                SetData();
                RunOnUiThread(() =>
                {

                    ImageView BNewProject = new ImageView(this);
                    BNewProject.SetImageResource(Resource.Drawable.ic_add_white_18dp);
                    if (RedMineManager.currUser.status != 1)
                        BNewProject.Visibility = ViewStates.Gone;
                    LLControls.AddView(BNewProject);
                    BNewProject.Click += delegate
                    {

                        Intent i = new Intent(this, typeof(NewProjectActivity));
                        i.PutExtra("IsNew", true);
                        StartActivityForResult(i, 0);
                    };

                    LVProjects.GroupClick += (object sender, ExpandableListView.GroupClickEventArgs e) =>
                    {
                        if (mAdapter.GetChildrenCount(e.GroupPosition) == 0)
                        {
                            Intent i = new Intent(this, typeof(OneProjectActivity));
                            i.PutExtra("Kind", "Project Issues");
                            i.PutExtra("ProjectId", (int)mAdapter.GetGroup(e.GroupPosition));
                            StartActivity(i);
                        }
                        else
                            if (!LVProjects.IsGroupExpanded(e.GroupPosition))
                            LVProjects.ExpandGroup(e.GroupPosition);
                        else
                            LVProjects.CollapseGroup(e.GroupPosition);


                    };

                    LVProjects.ChildClick += (object sender, ExpandableListView.ChildClickEventArgs e) =>
                    {
                        Intent i = new Intent(this, typeof(OneProjectActivity));
                        i.PutExtra("Kind", "Project Issues");
                        i.PutExtra("ProjectId", (int)mAdapter.GetChild(e.GroupPosition, e.ChildPosition));
                        StartActivity(i);
                    };
                });
            });
            load.Start();

            SwipeRefreshLayout refresh = FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh);
            refresh.SetDistanceToTriggerSync(50);
            refresh.SetColorSchemeResources(Resource.Color.colorAccent, Resource.Color.colorPrimary);
            refresh.Refresh += delegate
            {
                Thread t = new Thread(() =>
                 {
                     SetData();
                     RunOnUiThread(()=> 
                     {
                         Recreate();
                         refresh.Refreshing = false;
                     });
                     
                 });
                t.Start();
            };
        }
        public void SetData()
        {
            projectsContainer = RedMineManager.Get<ProjectsContainer>("/projects.json");
            projectsGroups = ProjectsContainer.GetProjectsGroupId();
            projectsItems = ProjectsContainer.GetProjectsItemsId(projectsContainer);
            mAdapter = new ExpandableListViewAdapter(this, projectsGroups, projectsItems, projectsContainer.projects);
            RunOnUiThread(()=>LVProjects.SetAdapter(mAdapter));
        }
    }
}