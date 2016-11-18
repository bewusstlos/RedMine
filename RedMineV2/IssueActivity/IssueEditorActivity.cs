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
using System.Net;
using System.IO;
using System.Threading.Tasks;
using RedMineV2.MainActivity;
using Android.Content.PM;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;

namespace RedMineV2.IssueActivity
{
    [Activity(Label = "Edit Issue", MainLauncher = false, Icon = "@drawable/icon",
        Theme = "@style/Theme.Material.Orange", ConfigurationChanges = ConfigChanges.Locale)]
    class IssueEditorActivity : AppCompatActivity
    {
        bool isAdmin;
        UsersContainer usersContainer;
        string login;
        string password;

        Toolbar toolbar;
        LinearLayout LLRoot;
        Spinner STracker;
        Spinner SPriority;
        Spinner SAssignee;
        ProgressDialog pd;
        Spinner SStatus;
        ImageButton BStartDate;
        EditText ETStartDate;
        ImageButton BDueDate;
        EditText ETDueDate;
        EditText ETEstimatedHours;
        EditText ETProgress;
        EditText ETSpentHours;
        Spinner SActivity;
        EditText ETDescription;
        EditText ETSubject;
        EditText ETNewNote;
        LinearLayout LLNotesRoot;

        Button BSave;

        IssuesContainer.Issue currIssue;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.issue_editor_layout);
            if (RedMineManager.Get<MainActivity.User>("/users/current.json", "user").status != 0)
                isAdmin = true;
            usersContainer = RedMineManager.Get<UsersContainer>("/users.json?limit=100&nometa=1");

            if (!Intent.GetBooleanExtra("IsNew", false))
                currIssue = RedMineManager.Get<IssuesContainer.Issue>("/issues/" + Intent.GetIntExtra("ID", 0) + ".json?include=journals&nometa=1", "issue");
            BSave = new Button(this);
            BSave.Text = Resources.GetString(Resource.String.Save);
            BSave.Click += BSave_Click;

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.SetTitle(Resource.String.EditIssue);
            toolbar.AddView(BSave);
            SetSupportActionBar(toolbar);

            LLRoot = FindViewById<LinearLayout>(Resource.Id.root);
            STracker = FindViewById<Spinner>(Resource.Id.tracker);
            ArrayAdapter trackerAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleDropDownItem1Line, Enum.GetNames(typeof(RedMineManager.Trackers)));
            STracker.Adapter = trackerAdapter;
            trackerAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

            SPriority = FindViewById<Spinner>(Resource.Id.priority);
            var priorityAdapter = ArrayAdapter.CreateFromResource(this, Resource.Array.priorities_array, Android.Resource.Layout.SimpleDropDownItem1Line);
            priorityAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleDropDownItem1Line);
            SPriority.Adapter = priorityAdapter;

            SAssignee = FindViewById<Spinner>(Resource.Id.assigned_to);
                var query = from r in usersContainer.users
                            select r.firstname + " " + r.lastname;
            ArrayAdapter assigneeAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleDropDownItem1Line, query.ToList());
                assigneeAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleDropDownItem1Line);
                SAssignee.Adapter = assigneeAdapter;
            if(!isAdmin)
                SAssignee.Enabled = false;

            SStatus = FindViewById<Spinner>(Resource.Id.status);
            var statusAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleDropDownItem1Line, Enum.GetNames(typeof(RedMineManager.IssueStatuses)));
            SStatus.Adapter = statusAdapter;
            if (!isAdmin)
                SStatus.Enabled = false;
            statusAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleDropDownItem1Line);
            

            BStartDate = FindViewById<ImageButton>(Resource.Id.start_date);
            BStartDate.Click += ChangeDateClick;

            ETStartDate = FindViewById<EditText>(Resource.Id.txt_start_date);

            BDueDate = FindViewById<ImageButton>(Resource.Id.due_date);
            BDueDate.Click += DueDateClick;

            ETDueDate = FindViewById<EditText>(Resource.Id.txt_due_date);

            ETEstimatedHours = FindViewById<EditText>(Resource.Id.estimated_hours);
            
            ETProgress = FindViewById<EditText>(Resource.Id.done_ratio);
            if (Intent.GetBooleanExtra("IsNew", false))
                ETProgress.Enabled = false;
            

            ETDescription = FindViewById<EditText>(Resource.Id.description);
            
            ETSubject = FindViewById<EditText>(Resource.Id.subject);


            LLNotesRoot = FindViewById<LinearLayout>(Resource.Id.notes_root);
            ETNewNote = new EditText(this);
            ETNewNote.Hint = Resources.GetString(Resource.String.NewNote);
            LLNotesRoot.AddView(ETNewNote);
            SetNotesLayout();



            if (currIssue != null)
            {
                SStatus.SetSelection(statusAdapter.GetPosition(currIssue.status.name));
                ETSubject.Text = currIssue.subject;
                ETDescription.Text = currIssue.description;
                ETEstimatedHours.Text = currIssue.estimated_hours.ToString();
                ETProgress.Text = currIssue.done_ratio.ToString();
                ETEstimatedHours.Text = currIssue.estimated_hours.ToString();
                if(currIssue.assigned_to != null)
                SAssignee.SetSelection(assigneeAdapter.GetPosition(currIssue.assigned_to.name));
                if(currIssue.tracker != null)
                STracker.SetSelection(trackerAdapter.GetPosition(currIssue.tracker.name));
                if(currIssue.priority != null)
                SPriority.SetSelection(priorityAdapter.GetPosition(currIssue.priority.name));
                ETStartDate.Text = currIssue.start_date;
                ETDueDate.Text = currIssue.due_date;
            }
            else
                currIssue = new IssuesContainer.Issue { project = new IssuesContainer.Project { id = Intent.GetIntExtra("ProjectId", 0)} };
            
            if (Intent.GetBooleanExtra("IsNew", false))
                ClearIssueFields();
        }

        public void SetNotesLayout()
        {
            LayoutInflater inflater = (LayoutInflater)this.GetSystemService(Context.LayoutInflaterService);
            

            foreach (var journal in currIssue.journals)
            {
                if (journal.notes == "")
                    continue;
                View v = inflater.Inflate(Resource.Layout.note_fragment, null);
                TextView TVNote = v.FindViewById<TextView>(Resource.Id.note_label);
                TVNote.Text = journal.notes;

                EditText ETNote = v.FindViewById<EditText>(Resource.Id.note);
                ETNote.Text = journal.notes;

                ImageView IVEdit = v.FindViewById<ImageView>(Resource.Id.edit);
                ImageView IVDone = v.FindViewById<ImageView>(Resource.Id.done);

                IVEdit.Click += delegate
                {
                    TVNote.Visibility = ViewStates.Gone;
                    ETNote.Visibility = ViewStates.Visible;

                    IVEdit.Visibility = ViewStates.Gone;
                    IVDone.Visibility = ViewStates.Visible;
                };

                IVDone.Click += delegate
                {
                    journal.notes = ETNote.Text;

                    TVNote.Text = ETNote.Text;
                    TVNote.Visibility = ViewStates.Visible;
                    ETNote.Visibility = ViewStates.Gone;

                    IVEdit.Visibility = ViewStates.Visible;
                    IVDone.Visibility = ViewStates.Gone;
                };
                LLRoot.AddView(v);
            }
        }

        private void BSave_Click(object sender, EventArgs e)
        {
            currIssue.priority = new IssuesContainer.Priority { id = (int)Enum.Parse(typeof(RedMineManager.Priorities), SPriority.SelectedItem.ToString()), name = SPriority.SelectedItem.ToString()};
            currIssue.status = new IssuesContainer.Status {id = (int)Enum.Parse(typeof(RedMineManager.IssueStatuses), SStatus.SelectedItem.ToString()),name=SStatus.SelectedItem.ToString()};
            currIssue.tracker = new IssuesContainer.Tracker {id = (int)Enum.Parse(typeof(RedMineManager.Trackers), STracker.SelectedItem.ToString()), name = STracker.SelectedItem.ToString()};
            var query = from r in usersContainer.users
                        where r.firstname + " " + r.lastname == SAssignee.SelectedItem.ToString()
                        select r;

            currIssue.assigned_to = new IssuesContainer.AssignedTo {id = query.SingleOrDefault().id, name = query.SingleOrDefault().firstname + " " + query.SingleOrDefault().lastname };
            if(ETEstimatedHours.Text !="")
            currIssue.estimated_hours = Convert.ToDouble(ETEstimatedHours.Text);
            if(ETProgress.Text != "")
                currIssue.done_ratio = int.Parse(ETProgress.Text);
            currIssue.subject = ETSubject.Text;
            currIssue.description = ETDescription.Text;

            var queryUser = from r in usersContainer.users
                            where r.login == login
                            select r;

            if (ETNewNote.Text != "")
            {
                currIssue.journals = currIssue.journals ?? new List<IssuesContainer.Journal>();
                currIssue.journals.Add(new IssuesContainer.Journal { created_on = DateTime.Now.ToShortDateString(), details = null, id = currIssue.journals.Count, notes = ETNewNote.Text, user = new IssuesContainer.User{id = query.Single().id,name=query.Single().firstname + " "+query.Single().lastname} });
            }
            string message = "";
                bool isNew = Intent.GetBooleanExtra("IsNew", false);
                message = RedMineManager.PutPostIssue(currIssue, isNew, ETNewNote.Text);
                Toast.MakeText(this, message, ToastLength.Short).Show();
            Finish();
        }

        private void DueDateClick(object sender, EventArgs e)
        {
            date = new DatePickerDialog(this, Resource.Style.Theme_Material_Orange, DatePickerCallbackDue, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            date.Show();
        }

        string dt;
        Dialog date;

        private void DatePickerCallback(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            dt = "";
            dt += e.Date.ToString("yyyy-MM-dd");
            ETStartDate.Text = dt;
            currIssue.start_date = dt;
        }

        private void DatePickerCallbackDue(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            dt = "";
            dt += e.Date.ToString("yyyy-MM-dd");
            ETDueDate.Text = dt;
            currIssue.due_date = dt;
        }

        private void ChangeDateClick(object sender, EventArgs e)
        {
            date = new DatePickerDialog(this, Resource.Style.Theme_Material_Orange, DatePickerCallback, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            date.Show();
        }

        public void ClearIssueFields()
        {
            ETDescription.Text = "";
            ETEstimatedHours.Text = "";
            ETProgress.Text = "";
            ETSubject.Text = "";
        }
        
     
    }
}