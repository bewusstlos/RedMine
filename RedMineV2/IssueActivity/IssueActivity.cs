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
using RedMineV2.MainActivity;
using static RedMineV2.MainActivity.IssuesContainer;
using System.Threading.Tasks;
using System.Threading;
using Android.Content.PM;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using SwipeRefreshLayout = Android.Support.V4.Widget.SwipeRefreshLayout;
using Space = Android.Support.V4.Widget.Space;
using Android.Support.V7.App;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using System.Globalization;

namespace RedMineV2.IssueActivity
{
    [Activity(Label = "Issues", MainLauncher = false, Icon = "@drawable/icon",
        Theme = "@style/Theme.Material.Orange", ConfigurationChanges = ConfigChanges.Locale)]
    class IssueActivity : AppCompatActivity
    {
        public IssuesContainer issues;
        SwipeRefreshLayout refresh;
        Random rnd;
        LinearLayout root;
        Toolbar toolbar;
        TextView TVUserFullName;
        ImageView BNew;
        DrawerLayout drawerLayout;
        NavigationView navigationView;
        NavigationView rightNavigationView;
        Spinner SFilterByStatus;
        Spinner SFilterByMonth;
        ArrayAdapter monthsAdapter;
        ArrayAdapter statusAdapter;
        CheckBox CBClosed;

        List<DateTime> dates;
        bool includeClosed;
        bool myIssues;

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

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutBoolean("includeClosed", includeClosed);
            outState.PutBoolean("myIssues", myIssues);
            try
            {
                outState.PutString("StatusFilter", SFilterByStatus.SelectedItem.ToString());
                outState.PutString("MonthFilter", SFilterByMonth.SelectedItem.ToString());
            }
            catch(NullReferenceException e)
            {

            }
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);
            includeClosed = savedInstanceState.GetBoolean("includeClosed");
            myIssues = savedInstanceState.GetBoolean("myIssues");
            SFilterByStatus.SetSelection(statusAdapter.GetPosition(savedInstanceState.GetString("StatusFilter")));
            SFilterByMonth.SetSelection(monthsAdapter.GetPosition(savedInstanceState.GetString("MonthFilter")));
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.issue_layout);
            rnd = new System.Random();
            includeClosed = false;
            if (Intent.GetStringExtra("Kind") == "Project Issues")
                myIssues = false;
            else
                myIssues = true;
            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.SetTitle(Resource.String.Issues);
            if (myIssues)
                toolbar.SetTitle(Resource.String.MyIssues);
            SetSupportActionBar(toolbar);

            //Enable support action bar to display hamburger
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu_white_18dp);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            rightNavigationView = FindViewById<NavigationView>(Resource.Id.right_nav_view);

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
                        Intent i0 = new Intent(this, typeof(IssueActivity));
                        i0.PutExtra("Kind", "My Issues");
                        StartActivity(i0);
                        break;
                    case Resource.Id.nav_projects:
                        if (RedMineManager.currUser.status == 1)
                        {
                            Intent i4 = new Intent(this, typeof(MainActivity.MainActivity));
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

            BNew = new ImageView(this);
            LinearLayout LLControls = FindViewById<LinearLayout>(Resource.Id.right_controls);
            BNew.SetImageResource(Resource.Drawable.ic_add_white_18dp);
            LLControls.AddView(BNew);
            BNew.Click += delegate
            {
                Intent i = new Intent(this, typeof(IssueEditorActivity));
                i.PutExtra("IsNew", true);
                i.PutExtra("Issue", Newtonsoft.Json.JsonConvert.SerializeObject(new Issue { tracker = new Tracker(), assigned_to = new AssignedTo(), author = new Author(), priority = new Priority(), status = new Status(), project = new IssuesContainer.Project() }));
                i.PutExtra("ProjectId", Intent.GetIntExtra("ProjectId", 0));
                StartActivityForResult(i, 0);
            };
            int g = Intent.GetIntExtra("ProjectId", 0);
            issues = RedMineManager.Get<IssuesContainer>("/issues.json?assigned_to_id=me&nometa=1");
            BNew.Visibility = ViewStates.Gone;


            if (Intent.GetStringExtra("Kind") == "Project Issues")
            {
                issues = RedMineManager.Get<IssuesContainer>("/projects/" + g + "/issues.json?nometa=1");
                BNew.Visibility = ViewStates.Visible;
            }

            SetLayout();
            SetRightNavPanel();

            refresh = FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh);
            refresh.SetDistanceToTriggerSync(1);
            refresh.SetColorSchemeResources(Resource.Color.colorAccent, Resource.Color.colorPrimary);
            refresh.Refresh += delegate
            {
                Thread t = new Thread(() =>
                {
                    GetIssuesByFilters(includeClosed, myIssues, Intent.GetIntExtra("ProjectId",0));
                    RunOnUiThread(() =>
                    {
                        Recreate();
                        refresh.Refreshing = false;
                    });

                });
                t.Start();
            };
        }
        public void GetIssuesByFilters(bool includeClosed, bool myIssues = false, int projectId=0)
        {
            if (includeClosed && projectId == 0)
            {
                if (myIssues)
                    issues = RedMineManager.Get<IssuesContainer>("/issues.json?nometa=1&assigned_to_id=me&status_id=*");
                else
                    issues = RedMineManager.Get<IssuesContainer>("/issues.json?nometa=1&status_id=*");
            }
            else if (projectId != 0)
            {
                if (includeClosed)
                    issues = RedMineManager.Get<IssuesContainer>("/issues.json?nometa=1&project_id=" + projectId + "&status_id=*");
                else
                    issues = RedMineManager.Get<IssuesContainer>("/issues.json?nometa=1&project_id=" + projectId);
            }
            else
            {
                if (myIssues)
                    issues = RedMineManager.Get<IssuesContainer>("/issues.json?nometa=1&assigned_to_id=me");
                else
                    issues = RedMineManager.Get<IssuesContainer>("/issues.json?nometa=1");
            }
        }

        public void SetRightNavPanel()
        {
            SFilterByStatus = FindViewById<Spinner>(Resource.Id.filter_by_status);
            SFilterByMonth = FindViewById<Spinner>(Resource.Id.filter_by_month);
            CBClosed = FindViewById<CheckBox>(Resource.Id.closed_filter);

            var valuesForStatusAdapter = new List<string>() { "None" };
            valuesForStatusAdapter.AddRange(Enum.GetNames(typeof(RedMineManager.IssueStatuses)));
            statusAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleDropDownItem1Line, valuesForStatusAdapter);
            SFilterByStatus.Adapter = statusAdapter;

            dates = new List<DateTime>();
            DateTime currDate = DateTime.Now;
            for (int i = 0; i < 24; i++)
            {
                dates.Add(currDate.AddMonths(-i));
            }

            var monthQuery = from date in dates
                             select date.ToString(@"MMMM yyyy", CultureInfo.CurrentCulture);
            List<string> valuesFoMonthAdapter = new List<string>() { "None" };
            valuesFoMonthAdapter.AddRange(monthQuery.ToList());
            monthsAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleDropDownItem1Line, valuesForStatusAdapter);

            SFilterByMonth.Adapter = monthsAdapter;

            SFilterByStatus.ItemSelected += delegate
            {
                if (SFilterByStatus.SelectedItem.ToString() != "None")
                {
                    int statusId = (int)Enum.Parse(typeof(RedMineManager.IssueStatuses), SFilterByStatus.SelectedItem.ToString());
                    if (myIssues)
                        issues = RedMineManager.Get<IssuesContainer>("/issues.json?nometa=1&limit=100&assigned_to_id=me&status_id=" + statusId);
                    else
                        issues = RedMineManager.Get<IssuesContainer>("/issues.json?nometa=1&limit=100&project_id=" + Intent.GetIntExtra("ProjectId", 0) + "&status_id=" + statusId);
                    SetLayout();
                }
                else
                    GetIssuesByFilters(includeClosed, myIssues, Intent.GetIntExtra("ProjectId",0));
            };

            SFilterByMonth.ItemSelected += delegate
            {
                if(SFilterByMonth.SelectedItem.ToString() != "None")
                {
                    string segmentUri = "/issues.xml?nometa=1&limit=100&created_on=%3E%3C";
                    var dateQuery = from date in dates
                                    where date.ToString(@"MMMM yyyy", CultureInfo.CurrentCulture) == SFilterByMonth.SelectedItem.ToString()
                                    select date;
                    DateTime.DaysInMonth(dateQuery.SingleOrDefault().Year, dateQuery.SingleOrDefault().Month);
                    DateTime endTime = dateQuery.SingleOrDefault();
                    segmentUri += endTime.ToString("yyyy-MM-dd") + "|";
                    endTime.AddDays(DateTime.DaysInMonth(dateQuery.SingleOrDefault().Year, dateQuery.SingleOrDefault().Month)-1);
                    segmentUri += endTime.ToString("yyyy-MM-dd");
                    if (myIssues)
                        segmentUri += "&assigned_to_id=me";
                    else
                        segmentUri += "&project_id=" + Intent.GetIntExtra("ProjectId", 0);
                    issues = RedMineManager.Get<IssuesContainer>(segmentUri);
                    SetLayout();
                }
            };
            CBClosed.CheckedChange += delegate
            {
                includeClosed = CBClosed.Checked;
                GetIssuesByFilters(includeClosed, myIssues, Intent.GetIntExtra("ProjectId", 0));
                SetLayout();
            };
        }

        public void SetLayout()
        {
            root = FindViewById<LinearLayout>(Resource.Id.root);
            root.RemoveAllViews();
            foreach (var i in issues.issues)
            {
                root.AddView(SetIssueView(i));
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            this.Recreate();
        }

        TextView TVTimeEntry;
        public View SetIssueView(Issue currIssue)
        {
            Issue issue = RedMineManager.Get<Issue>("/issues/" + currIssue.id + ".json?include=changesheets,journals", "issue");
            LayoutInflater inflater = (LayoutInflater)this.GetSystemService(Context.LayoutInflaterService);
            View v = inflater.Inflate(Resource.Layout.issue_fragment, null);

            LinearLayout root = v.FindViewById<LinearLayout>(Resource.Id.root);

            TextView TVProjectTitle = v.FindViewById<TextView>(Resource.Id.project_title);
            TVProjectTitle.Text = issue.project.name;

            ProgressBar PBProgress = v.FindViewById<ProgressBar>(Resource.Id.issue_progress);
            PBProgress.Max = 100;
            PBProgress.Progress = issue.done_ratio;

            TextView TVProgress = v.FindViewById<TextView>(Resource.Id.issue_progress_value);
            TVProgress.Text = issue.done_ratio.ToString();

            TextView TVTracker = v.FindViewById<TextView>(Resource.Id.tracker);
            TVTracker.Text = issue.tracker.name;
            TextView TVStatus = v.FindViewById<TextView>(Resource.Id.status);
            TVStatus.Text = issue.status.name;
            TextView TVAuthor = v.FindViewById<TextView>(Resource.Id.author);
            TVAuthor.SetMinLines(2);
            TVAuthor.Text = issue.author.name;
            TextView TVAssignedTo = v.FindViewById<TextView>(Resource.Id.assigned_to);
            if (issue.assigned_to != null)
                TVAssignedTo.Text = issue.assigned_to.name;
            TextView TVPriority = v.FindViewById<TextView>(Resource.Id.priority);
            TVPriority.Text = issue.priority.name;
            TextView TVStartDate = v.FindViewById<TextView>(Resource.Id.start_date);
            if (issue.start_date != null)
                TVStartDate.Text = issue.start_date;
            TextView TVDueDate = v.FindViewById<TextView>(Resource.Id.due_date);
            if (issue.due_date != null)
                TVDueDate.Text = issue.due_date;
            TextView TVCreatedOn = v.FindViewById<TextView>(Resource.Id.created_on);
            TVCreatedOn.Text = issue.created_on;
            TextView TVUpdatedOn = v.FindViewById<TextView>(Resource.Id.updated_on);
            TVUpdatedOn.Text = issue.updated_on;
            TextView TVSubjectLabel = v.FindViewById<TextView>(Resource.Id.subject_label);
            TextView TVSubject = v.FindViewById<TextView>(Resource.Id.subject);
            TVSubject.Text = issue.subject;
            TextView TVDescriptionLabel = v.FindViewById<TextView>(Resource.Id.description_label);
            TextView TVDescription = v.FindViewById<TextView>(Resource.Id.description);
            TVDescription.Text = issue.description;
            TextView TVNotesLabel = v.FindViewById<TextView>(Resource.Id.notes_label);
            TVNotesLabel.Visibility = ViewStates.Gone;
            TextView TVNotes = v.FindViewById<TextView>(Resource.Id.notes);
            TVNotes.Visibility = ViewStates.Gone;
            if (issue.journals != null)
                foreach (var journal in issue.journals)
                {
                    TVNotes.Text += journal.notes;
                    if (journal.notes != "")
                    {
                        TVNotes.Text += "\n\n";
                    }
                }

            TextView Expander = v.FindViewById<TextView>(Resource.Id.expander);
            ImageView BEdit = v.FindViewById<ImageView>(Resource.Id.edit);
            ImageView BDelete = v.FindViewById<ImageView>(Resource.Id.delete);
            Expander.Click += delegate
            {
                if (TVSubject.Visibility == ViewStates.Gone)
                {
                    TVSubjectLabel.Visibility = ViewStates.Visible;
                    TVSubject.Visibility = ViewStates.Visible;
                    TVDescriptionLabel.Visibility = ViewStates.Visible;
                    TVDescription.Visibility = ViewStates.Visible;
                    TVNotesLabel.Visibility = ViewStates.Visible;
                    TVNotes.Visibility = ViewStates.Visible;
                    Expander.Text = Resources.GetString(Resource.String.Collapse);
                }
                else
                {
                    TVSubjectLabel.Visibility = ViewStates.Gone;
                    TVSubject.Visibility = ViewStates.Gone;
                    TVDescriptionLabel.Visibility = ViewStates.Gone;
                    TVDescription.Visibility = ViewStates.Gone;
                    TVNotesLabel.Visibility = ViewStates.Gone;
                    TVNotes.Visibility = ViewStates.Gone;
                    Expander.Text = Resources.GetString(Resource.String.Expand);
                }
            };
            BEdit.Click += delegate
            {
                if (!(RedMineManager.ValidateUserForAction(issue.project.id).Contains(6) ||
                RedMineManager.ValidateUserForAction(issue.project.id).Contains(4) || RedMineManager.currUser.status == 1))
                {
                    Toast.MakeText(this, Resources.GetString(Resource.String.PermissionError), ToastLength.Short).Show();
                    return;
                }
                Intent i = new Intent(this, typeof(IssueEditorActivity));
                //i.PutExtra("ID", issue.id);

                i.PutExtra("ID", issue.id);
                StartActivityForResult(i, 0);
            };

            BDelete.Click += delegate
            {
                if (!(RedMineManager.ValidateUserForAction(issue.project.id).Contains(6) || RedMineManager.currUser.status == 1))
                {
                    Toast.MakeText(this, Resources.GetString(Resource.String.PermissionError), ToastLength.Short).Show();
                    return;
                }
                var alert = new AlertDialog.Builder(this);
                alert.SetMessage(Resources.GetString(Resource.String.IssueDeleteConfirmation));
                alert.SetPositiveButton(Resources.GetString(Resource.String.Ok), (s, arg) =>
                 {
                     RedMineManager.Delete("/issues/" + issue.id + ".json");
                 });
                alert.SetNegativeButton(Resources.GetString(Resource.String.Cancel), (s, arg) =>
                {

                });
                RunOnUiThread(() => { alert.Show(); });
                this.Recreate();
            };
            ImageView BTimePlay = v.FindViewById<ImageView>(Resource.Id.play);
            TVTimeEntry = v.FindViewById<TextView>(Resource.Id.time_entry);

            BTimePlay.Click += (s, arg) =>
            {
                Intent i = new Intent(this, typeof(TimeEntryActivity));
                i.PutExtra("IssueID", issue.id);
                StartActivity(i);
            };
            return v;
        }
    }
}