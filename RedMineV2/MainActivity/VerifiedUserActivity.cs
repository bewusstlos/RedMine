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
using Android.Views.Animations;
using System.Threading;
using System.Threading.Tasks;
using Android.Support.V7.App;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Content.PM;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using System.Globalization;

namespace RedMineV2.MainActivity
{
    [Activity(Label = "Log in", MainLauncher = false, Icon = "@drawable/icon",
        Theme = "@style/Theme.Material.Orange", ConfigurationChanges = ConfigChanges.Locale)]
    class VerifiedUserActivity : AppCompatActivity
    {
        UsersContainer usersContainer;
        ProjectsContainer projectsContainer;
        IssuesContainer issuesContainer;
        List<DateTime> dates;

        Toolbar toolbar;
        DrawerLayout drawerLayout;
        NavigationView navigationView;
        ProgressDialog PDLoading;
        LinearLayout LLControls;
        LinearLayout LLRoot;
        Spinner SFilterByUser;
        Spinner SFilterByMonth;
        Spinner SFilterByProject;
        Spinner SFilterByIssue;
        CheckBox CBClosed;
        Button BApplyFilter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.verified_user_layout);
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            navigationView = FindViewById<NavigationView>(Resource.Id.filter_nav_menu);
            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            LLControls = FindViewById<LinearLayout>(Resource.Id.right_controls);
            LLRoot = FindViewById<LinearLayout>(Resource.Id.root);
            SFilterByUser = FindViewById<Spinner>(Resource.Id.filter_by_user);
            SFilterByMonth = FindViewById<Spinner>(Resource.Id.filter_by_month);
            SFilterByProject = FindViewById<Spinner>(Resource.Id.filter_by_project);
            SFilterByIssue = FindViewById<Spinner>(Resource.Id.filter_by_issue);
            CBClosed = FindViewById<CheckBox>(Resource.Id.closed);
            BApplyFilter = FindViewById<Button>(Resource.Id.apply_filter);
            //adding filter button to toolbar
            ImageView IVFilter = new ImageView(this);
            IVFilter.SetImageResource(Resource.Drawable.ic_filter_list_white_18dp);
            //action handler for filter button
            IVFilter.Click += delegate
            {
                if (drawerLayout.IsDrawerOpen(navigationView))
                    drawerLayout.CloseDrawers();
                else
                    drawerLayout.OpenDrawer(navigationView);
            };
            LLControls.AddView(IVFilter);

            toolbar.SetTitle(Resource.String.Administration);
            SetSupportActionBar(toolbar);

            //
            SettingUpViews();
        }

        public void SettingUpViews()
        {
            PDLoading = new ProgressDialog(this);
            PDLoading.SetTitle(Resources.GetString(Resource.String.LoadingAllRequiredData));
            PDLoading.SetMessage(Resources.GetString(Resource.String.PleaseWait));
            PDLoading.Show();
            Thread loadAllData = new Thread(() =>
            {
                usersContainer = RedMineManager.Get<UsersContainer>("/users.json?nometa=1&limit=100");
                projectsContainer = RedMineManager.Get<ProjectsContainer>("/projects.json?nometa=1&limit=100");
                issuesContainer = RedMineManager.Get<IssuesContainer>("/issues.json?nometa=1&limit=100");
                foreach (var issue in issuesContainer.issues)
                {
                    issue.timeEntries = RedMineManager.Get<List<IssuesContainer.TimeEntry>>("/issues/" + issue.id + "/time_entries.json?nometa=1", "time_entries");
                    foreach (var timeEntry in issue.timeEntries)
                    {
                        issue.spent_hours += timeEntry.hours;
                    }
                }
                foreach (var project in projectsContainer.projects)
                {
                    project.memberships = RedMineManager.Get<List<Membership>>("/projects/" + project.id + "/memberships.json?nometa=1", "memberships");
                }
                //formating data for adapters and setting dates
                var usersQuery = from user in usersContainer.users
                                 select user.firstname + " " + user.lastname;
                List<string> usersForAdapter = new List<string>() { "None" };
                usersForAdapter.AddRange(usersQuery.ToList());
                var projectsQuery = from project in projectsContainer.projects
                                    select project.name;
                List<string> projectsForAdapter = new List<string>() { "None" };
                projectsForAdapter.AddRange(projectsQuery.ToList());
                var issuesQuery = from issue in issuesContainer.issues
                                  select issue.subject;
                List<string> issuesForAdapter = new List<string>() { "None" };
                issuesForAdapter.AddRange(issuesQuery.ToList());
                dates = new List<DateTime>();
                DateTime currDate = DateTime.Now;
                for (int i = 0; i < 24; i++)
                {
                    dates.Add(currDate.AddMonths(-i));
                }

                var monthQuery = from date in dates
                                 select date.ToString(@"MMMM yyyy", CultureInfo.CurrentCulture);
                List<string> monthsForAdapter = new List<string>() { "None" };
                monthsForAdapter.AddRange(monthQuery.ToList());
                //creating adapters for spinners
                var userAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleDropDownItem1Line, usersForAdapter);
                var monthAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleDropDownItem1Line, monthsForAdapter);
                var projectAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleDropDownItem1Line, projectsForAdapter);
                var issuesAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, issuesForAdapter);

                RunOnUiThread(() =>
                {
                    SFilterByUser.Adapter = userAdapter;
                    SFilterByProject.Adapter = projectAdapter;
                    SFilterByMonth.Adapter = monthAdapter;
                    SFilterByIssue.Adapter = issuesAdapter;
                });
                //When project selected, set only this project`s issues for issue spinner
                SFilterByProject.ItemSelected += delegate
                {
                    var issueAdapterQuery = from issue in issuesContainer.issues
                                            where issue.project.name == SFilterByProject.SelectedItem.ToString()
                                            select issue.subject;
                    issuesAdapter.Clear();
                    issuesAdapter.Add("None");
                    issuesAdapter.AddAll(issueAdapterQuery.ToList());
                };
                SFilterByUser.ItemSelected += delegate
                {
                    if (SFilterByUser.SelectedItem.ToString() != "None")
                    {
                        //When user selected set only projects, which user involved
                        var projectQuery = from project in projectsContainer.projects
                                           from membership in project.memberships
                                           where membership?.user?.name == SFilterByUser.SelectedItem.ToString()
                                           select project.name;
                        projectAdapter.Clear();
                        projectAdapter.Add("None");
                        projectAdapter.AddAll(projectQuery.ToList());

                        //Also set issues, where user assigned to
                        var issueQuery = from issue in issuesContainer.issues
                                         where issue?.assigned_to?.name == SFilterByUser.SelectedItem.ToString()
                                         select issue.subject;
                        issuesAdapter.Clear();
                        issuesAdapter.Add("None");
                        issuesAdapter.AddAll(issueQuery.ToList());
                    }
                };

                CBClosed.CheckedChange += delegate
                {
                    RunOnUiThread(() => PDLoading.Show());
                    Thread innerThread = new Thread(() =>
                    {
                        if (CBClosed.Checked)
                            issuesContainer = RedMineManager.Get<IssuesContainer>("/issues.json?nometa=1&limit=100&status_id=*");
                        else
                            issuesContainer = RedMineManager.Get<IssuesContainer>("/issues.json?nometa=1&limit=100");
                        var issueQuery = from issue in issuesContainer.issues
                                         select issue.subject;

                        foreach (var issue in issuesContainer.issues)
                        {
                            issue.timeEntries = RedMineManager.Get<List<IssuesContainer.TimeEntry>>("/issues/" + issue.id + "/time_entries.json?nometa=1", "time_entries");
                            foreach (var timeEntry in issue.timeEntries)
                            {
                                issue.spent_hours += timeEntry.hours;
                            }
                        }

                        RunOnUiThread(() =>
                        {
                            issuesAdapter.Clear();
                            issuesAdapter.Add("None");
                            issuesAdapter.AddAll(issueQuery.ToList());
                            PDLoading.Hide();
                        });
                    });
                    SFilterByIssue.SetSelection(0);
                    SFilterByMonth.SetSelection(0);
                    SFilterByProject.SetSelection(0);
                    SFilterByUser.SetSelection(0);
                    innerThread.Start();
                };

                //Click for applying filter
                BApplyFilter.Click += delegate
                {
                    if (SFilterByUser.SelectedItem.ToString() != "None")
                    {
                        var userQuery = from user in usersContainer.users
                                    where user.firstname + " " + user.lastname == SFilterByUser.SelectedItem.ToString()
                                    select user;
                        usersContainer.users = userQuery.ToList();
                        var projectQuery = from project in projectsContainer.projects
                                           from membership in project.memberships
                                           where membership?.user?.name == SFilterByUser.SelectedItem.ToString()
                                           select project;
                        projectsContainer.projects = projectQuery.ToList();
                        var issueQuery = from issue in issuesContainer.issues
                                         where issue.assigned_to?.name == SFilterByUser.SelectedItem.ToString()
                                         select issue;
                        issuesContainer.issues = issueQuery.ToList();
                    }
                    if (SFilterByMonth.SelectedItem.ToString() != "None")
                    {
                        var query = from date in dates
                                    where date.ToString(@"MMMM yyyy") == SFilterByMonth.SelectedItem.ToString()
                                    from issue in issuesContainer.issues
                                    where DateTime.Parse(issue.created_on).Month == date.Month
                                    select issue;
                        issuesContainer.issues = query.ToList();
                    }
                    if (SFilterByProject.SelectedItem.ToString() != "None")
                    {
                        var issueQuery = from issue in issuesContainer.issues
                                         where issue.project.name == SFilterByProject.SelectedItem.ToString()
                                         select issue;
                        issuesContainer.issues = issueQuery.ToList();
                        var projectQuery = from project in projectsContainer.projects
                                           where project.name == SFilterByProject.SelectedItem.ToString()
                                           select project;
                        projectsContainer.projects = projectQuery.ToList();

                    }
                    if (SFilterByIssue.SelectedItem.ToString() != "None")
                    {
                        var query = from issue in issuesContainer.issues
                                    where issue.subject == SFilterByIssue.SelectedItem.ToString()
                                    select issue;
                        issuesContainer.issues = query.ToList();
                    }
                    SetView();
                };
                RunOnUiThread(() => PDLoading.Hide());
            });
            loadAllData.Start();
        }

        public void SetView()
        {
            LLRoot.RemoveAllViews();
            foreach (var project in projectsContainer.projects)
            {
                LayoutInflater inflater = (LayoutInflater)this.GetSystemService(Context.LayoutInflaterService);
                View projectView = inflater.Inflate(Resource.Layout.project_stat_fragment, null);
                TextView TVProjectTitle = projectView.FindViewById<TextView>(Resource.Id.project_title);
                TVProjectTitle.Text = project.name;
                LinearLayout LLIssueRoot = projectView.FindViewById<LinearLayout>(Resource.Id.issue_root);
                var query = from issue in issuesContainer.issues
                            where issue.project.id == project.id
                            select issue;


                foreach (var issue in query.ToList())
                {
                    LayoutInflater issueInflater = (LayoutInflater)this.GetSystemService(Context.LayoutInflaterService);
                    View issueView = issueInflater.Inflate(Resource.Layout.issue_stat_layout, null);
                    TextView TVIssueSubject = issueView.FindViewById<TextView>(Resource.Id.issue_subject);
                    TVIssueSubject.Text = issue.subject;

                    TextView TVIssueTime = issueView.FindViewById<TextView>(Resource.Id.issue_time);
                    TVIssueTime.Text = issue.spent_hours.ToString() + @"/" + issue.estimated_hours.ToString() + " h";

                    LLIssueRoot.AddView(issueView);

                    LinearLayout LLUsersTime = issueView.FindViewById<LinearLayout>(Resource.Id.user_time_root);

                    var userTimeEntry = from timeEntry in issue.timeEntries
                                        orderby timeEntry?.user?.name
                                        select timeEntry;
                    foreach (var timeEntry in userTimeEntry.ToList())
                    {
                        LinearLayout LLUserTime = new LinearLayout(this);
                        LLUserTime.SetBackgroundResource(Resource.Color.colorBackgroundCard);
                        LinearLayout.LayoutParams lpforLLuserTime = new LinearLayout.LayoutParams(-1, -2);
                        lpforLLuserTime.SetMargins(2, 2, 2, 2);
                        LLUserTime.SetPadding(5, 5, 5, 5);
                        LLUserTime.Orientation = Orientation.Horizontal;

                        TextView TVUsername = new TextView(this);
                        TVUsername.SetMaxWidth(Resources.DisplayMetrics.WidthPixels / 5 * 4);
                        TVUsername.SetTextColor(Resources.GetColor(Resource.Color.abc_primary_text_material_light));
                        TVUsername.Text = timeEntry.user.name;

                        TextView TVTime = new TextView(this);
                        LinearLayout.LayoutParams lpForTVTime = new LinearLayout.LayoutParams(-1, -2);
                        TVTime.Gravity = GravityFlags.Right;
                        TVTime.SetTextColor(Resources.GetColor(Resource.Color.colorPrimary));
                        TVTime.Text = timeEntry.hours.ToString() + " h";

                        LLUserTime.AddView(TVUsername);
                        LLUserTime.AddView(TVTime, lpForTVTime);

                        LLUsersTime.AddView(LLUserTime, lpforLLuserTime);
                    }
                }
                LLRoot.AddView(projectView);
            }
        }
    }
}