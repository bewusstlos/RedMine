using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using static RedMineV2.MainActivity.IssuesContainer;
using RedMineV2.IssueActivity;

namespace RedMineV2.MainActivity
{
    class IssueListViewAdapter : BaseAdapter<Issue>
    {
        private Context mContext;
        private List<IssuesContainer.Issue> issues;

        public IssueListViewAdapter(Context mContext, List<Issue> issues)
        {
            this.mContext = mContext;
            this.issues = issues;
        }

        public override int Count
        {
            get
            {
                return issues.Count;
            }
        }

        public override Issue this[int position]
        {
            get
            {
                return issues[position];
            }
        }

        public bool IsEnabled(int position)
        {
            throw new NotImplementedException();
        }

        public void RegisterDataSetObserver(DataSetObserver observer)
        {
            throw new NotImplementedException();
        }

        public void UnregisterDataSetObserver(DataSetObserver observer)
        {
            throw new NotImplementedException();
        }

        public override long GetItemId(int position)
        {
            throw new NotImplementedException();
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (convertView == null)
            {
                LayoutInflater inflater = (LayoutInflater)mContext.GetSystemService(Context.LayoutInflaterService);
                convertView = inflater.Inflate(Resource.Layout.issue_fragment, null);
            }
            Issue issue = RedMineManager.Get<Issue>("/issues/" + this[position].id + ".json?include=changesheets,journals", "issue");
            LinearLayout root = convertView.FindViewById<LinearLayout>(Resource.Id.root);

            TextView TVProjectTitle = convertView.FindViewById<TextView>(Resource.Id.project_title);
            TVProjectTitle.Text = issue.project.name;

            ProgressBar PBProgress = convertView.FindViewById<ProgressBar>(Resource.Id.issue_progress);
            PBProgress.Progress = issue.done_ratio;

            TextView TVProgress = convertView.FindViewById<TextView>(Resource.Id.issue_progress_value);
            TVProgress.Text = issue.done_ratio.ToString();

            TextView TVTracker = convertView.FindViewById<TextView>(Resource.Id.tracker);
            TVTracker.Text = issue.tracker.name;
            TextView TVStatus = convertView.FindViewById<TextView>(Resource.Id.status);
            TVStatus.Text = issue.status.name;
            TextView TVAuthor = convertView.FindViewById<TextView>(Resource.Id.author);
            TVAuthor.SetMinLines(2);
            TVAuthor.Text = issue.author.name;
            TextView TVAssignedTo = convertView.FindViewById<TextView>(Resource.Id.assigned_to);
            if (issue.assigned_to != null)
                TVAssignedTo.Text = issue.assigned_to.name;
            TextView TVPriority = convertView.FindViewById<TextView>(Resource.Id.priority);
            TVPriority.Text = issue.priority.name;
            TextView TVStartDate = convertView.FindViewById<TextView>(Resource.Id.start_date);
            if (issue.start_date != null)
                TVStartDate.Text = issue.start_date;
            TextView TVDueDate = convertView.FindViewById<TextView>(Resource.Id.due_date);
            if (issue.due_date != null)
                TVDueDate.Text = issue.due_date;
            TextView TVCreatedOn = convertView.FindViewById<TextView>(Resource.Id.created_on);
            TVCreatedOn.Text = issue.created_on;
            TextView TVUpdatedOn = convertView.FindViewById<TextView>(Resource.Id.updated_on);
            TVUpdatedOn.Text = issue.updated_on;
            TextView TVSubjectLabel = convertView.FindViewById<TextView>(Resource.Id.subject_label);
            TextView TVSubject = convertView.FindViewById<TextView>(Resource.Id.subject);
            TVSubject.Text = issue.subject;
            TextView TVDescriptionLabel = convertView.FindViewById<TextView>(Resource.Id.description_label);
            TextView TVDescription = convertView.FindViewById<TextView>(Resource.Id.description);
            TVDescription.Text = issue.description;
            TextView TVNotesLabel = convertView.FindViewById<TextView>(Resource.Id.notes_label);
            TVNotesLabel.Visibility = ViewStates.Gone;
            TextView TVNotes = convertView.FindViewById<TextView>(Resource.Id.notes);
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

            TextView Expander = convertView.FindViewById<TextView>(Resource.Id.expander);
            ImageView BEdit = convertView.FindViewById<ImageView>(Resource.Id.edit);
            ImageView BDelete = convertView.FindViewById<ImageView>(Resource.Id.delete);
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
                    Expander.Text = mContext.Resources.GetString(Resource.String.Collapse);
                }
                else
                {
                    TVSubjectLabel.Visibility = ViewStates.Gone;
                    TVSubject.Visibility = ViewStates.Gone;
                    TVDescriptionLabel.Visibility = ViewStates.Gone;
                    TVDescription.Visibility = ViewStates.Gone;
                    TVNotesLabel.Visibility = ViewStates.Gone;
                    TVNotes.Visibility = ViewStates.Gone;
                    Expander.Text = mContext.Resources.GetString(Resource.String.Expand);
                }
            };
            BEdit.Click += delegate
            {
                if (!(RedMineManager.ValidateUserForAction(issue.project.id).Contains(6) ||
                RedMineManager.ValidateUserForAction(issue.project.id).Contains(4)) || RedMineManager.currUser.status != 1)
                {
                    Toast.MakeText(mContext, mContext.Resources.GetString(Resource.String.PermissionError), ToastLength.Short).Show();
                    return;
                }
                Intent i = new Intent(mContext, typeof(IssueEditorActivity));
                //i.PutExtra("ID", issue.id);

                i.PutExtra("ID", issue.id);
                //(Activity)StartActivityForResult(i, 0);
            };

            BDelete.Click += delegate
            {
                if (!(RedMineManager.ValidateUserForAction(issue.project.id).Contains(6) || RedMineManager.currUser.status == 1))
                {
                    Toast.MakeText(mContext, mContext.Resources.GetString(Resource.String.PermissionError), ToastLength.Short).Show();
                    return;
                }
                var alert = new AlertDialog.Builder(mContext);
                alert.SetMessage(mContext.Resources.GetString(Resource.String.IssueDeleteConfirmation));
                alert.SetPositiveButton(mContext.Resources.GetString(Resource.String.Ok), (s, arg) =>
                {
                    RedMineManager.Delete("/issues/" + issue.id + ".json");
                });
                alert.SetNegativeButton(mContext.Resources.GetString(Resource.String.Cancel), (s, arg) =>
                {

                });
                //RunOnUiThread(() => { alert.Show(); });
                //mContext.Recreate();
            };
            ImageView BTimePlay = convertView.FindViewById<ImageView>(Resource.Id.play);
            TextView TVTimeEntry = convertView.FindViewById<TextView>(Resource.Id.time_entry);

            BTimePlay.Click += (s, arg) =>
            {
                Intent i = new Intent(mContext, typeof(TimeEntryActivity));
                i.PutExtra("IssueID", issue.id);
                //StartActivity(i);
            };
            return convertView;
        }
    }
}