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
using System.Threading;
using System.Threading.Tasks;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;
using Android.Content.PM;

namespace RedMineV2.MainActivity
{
    [Activity(Label = "Projects", MainLauncher = false, Icon = "@drawable/icon",
        Theme = "@style/Theme.Material.Orange", ConfigurationChanges = ConfigChanges.Locale)]
    class TimeEntryActivity : AppCompatActivity
    {
        IssuesContainer.Issue issue;

        Toolbar toolbar;
        ProgressDialog pd;
        Thread progress;
        Handler handler = new Handler();
        ProgressBar PBProgress;
        TextView TVSpentTime;
        TextView TVEstimatedTime;
        ImageView BPlay;
        ImageView BStop;
        ImageView BPause;

        RelativeLayout RLConfirm;
        EditText ETComment;
        Button BSubmit;

        DateTime startTime;
        TimeSpan spentTime;
        TimeSpan estimatedTime;
        TimeSpan newSpentTime;

        LinearLayout LLRoot;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.time_entry_layout);
            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.SetTitle(Resource.String.TimeEntries);
            SetSupportActionBar(toolbar);
            pd = new ProgressDialog(this);
            issue = RedMineManager.Get<IssuesContainer.Issue>("/issues/" + Intent.GetIntExtra("IssueID", 0) + ".json", "issue");
            issue.timeEntries = RedMineManager.Get<List<IssuesContainer.TimeEntry>>("/issues/" + Intent.GetIntExtra("IssueID", 0) + "/time_entries.json?nometa=1", "time_entries");

            PBProgress = FindViewById<ProgressBar>(Resource.Id.progress);
            TVSpentTime = FindViewById<TextView>(Resource.Id.spent_time);
            TVEstimatedTime = FindViewById<TextView>(Resource.Id.estimated_time);
            BPlay = FindViewById<ImageView>(Resource.Id.play);
            BStop = FindViewById<ImageView>(Resource.Id.stop);
            BPause = FindViewById<ImageView>(Resource.Id.pause);
            LLRoot = FindViewById<LinearLayout>(Resource.Id.root);

            RLConfirm = FindViewById<RelativeLayout>(Resource.Id.confirm);
            RLConfirm.Visibility = ViewStates.Gone;
            ETComment = FindViewById<EditText>(Resource.Id.comment);
            BSubmit = FindViewById<Button>(Resource.Id.submit);

            spentTime = TimeSpan.FromHours(issue.spent_hours);
            newSpentTime = new TimeSpan();

            estimatedTime = TimeSpan.FromHours(issue.estimated_hours);

            float r = (float)spentTime.Ticks / (float)estimatedTime.Ticks;
            PBProgress.Progress = Convert.ToInt32((float)r * (float)100);

            ShowTimeEntries();

            progress = new Thread(() =>
            {
                HandleSpentTime();
            });

            BPlay.Click += delegate
            {
                progress.Start();
                BPlay.Visibility = ViewStates.Gone;
                BStop.Visibility = ViewStates.Visible;
            };

            BPause.Click += delegate
            {
                if (progress.IsAlive)
                    progress.Suspend();
                else
                    progress.Resume();
            };

            BStop.Click += delegate
            {
                RLConfirm.Visibility = ViewStates.Visible;
                progress.Abort();
            };

            BSubmit.Click += delegate
            {
                IssuesContainer.TimeEntry t = new IssuesContainer.TimeEntry();
                t.issue = issue;
                t.comments = ETComment.Text;
                t.hours = newSpentTime.TotalHours;

                BPlay.Visibility = ViewStates.Visible;
                BStop.Visibility = ViewStates.Gone;

                Toast.MakeText(this, RedMineManager.PutPostTimeEntry(t, true), ToastLength.Short).Show();

                Recreate();
            };

            TVSpentTime.Text = spentTime.ToString();
            TVEstimatedTime.Text = estimatedTime.ToString();
        }

        public void HandleSpentTime()
        {

            startTime = DateTime.Now;
            DateTime dueTime = startTime + estimatedTime;
            while (DateTime.Now < dueTime)
            {
                spentTime = (DateTime.Now - startTime) + TimeSpan.FromHours(issue.spent_hours);
                newSpentTime = DateTime.Now - startTime;

                RunOnUiThread(() =>
                {
                    TVSpentTime.Text = spentTime.ToString(@"hh\:mm\:ss");
                    float r = (float)spentTime.Ticks / (float)estimatedTime.Ticks;
                    PBProgress.Progress = Convert.ToInt32((float)r * (float)100);
                });
            }
        }

        public void ShowTimeEntries()
        {
            foreach (var entry in issue.timeEntries)
            {
                LayoutInflater inflater = (LayoutInflater)this.GetSystemService(Context.LayoutInflaterService);
            View v = inflater.Inflate(Resource.Layout.time_entry_fragment, null);

            
                TextView TVHours = v.FindViewById<TextView>(Resource.Id.hours);
                TextView TVComment = v.FindViewById<TextView>(Resource.Id.comment);
                TextView TVSpentOn = v.FindViewById<TextView>(Resource.Id.spent_on);
                TextView TVPerformer = v.FindViewById<TextView>(Resource.Id.performer);

                TVHours.Text = entry.hours.ToString() + " h";
                TVComment.Text = entry.comments;
                TVSpentOn.Text = entry.spent_on;
                TVPerformer.Text = entry.user.name;

                LLRoot.AddView(v);
            }
        }
    }
}