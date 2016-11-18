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
using RestSharp;
using System.Threading.Tasks;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;

namespace RedMineV2.MainActivity
{
    [Activity(Label = "Projects", MainLauncher = false, Icon = "@drawable/icon",
        Theme = "@style/Theme.Material.Orange")]
    class NewProjectActivity:AppCompatActivity
    {
        ProjectsContainer projects;
        Toolbar toolbar;
        EditText ETName;
        EditText ETIdentifier;
        EditText ETHomePage;
        ProgressDialog pd;
        CheckBox CBIsPublic;
        Spinner SParent;
        EditText ETDescrtiption;
        Button BSubmit;

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
            SetContentView(Resource.Layout.new_project_layout);
            projects = RedMineManager.Get<ProjectsContainer>("/projects.json");

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.SetTitle(Resource.String.NewProject);
            SetSupportActionBar(toolbar);

            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_arrow_back_white_18dp);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            ImageView BSubmit = new ImageView(this);
            BSubmit.SetImageResource(Resource.Drawable.ic_save_white_18dp);

            LinearLayout LLControls = FindViewById<LinearLayout>(Resource.Id.right_controls);
            LLControls.AddView(BSubmit);

            ETName = FindViewById<EditText>(Resource.Id.name);
            ETIdentifier = FindViewById<EditText>(Resource.Id.identifier);
            ETHomePage = FindViewById<EditText>(Resource.Id.home_page);
            CBIsPublic = FindViewById<CheckBox>(Resource.Id.is_public);
            SParent = FindViewById<Spinner>(Resource.Id.parent);
            ETDescrtiption = FindViewById<EditText>(Resource.Id.description);

            BSubmit.Click += BSubmit_Click;

            var query = from r in projects.projects
                        select r.name;

            List<string> parents = new List<string> { "None" };
            parents.AddRange(query.ToList());

            var parentAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleDropDownItem1Line, parents);
            SParent.Adapter = parentAdapter;
            SParent.SetSelection(parentAdapter.GetPosition("None"));
        }

        private void BSubmit_Click(object sender, EventArgs e)
        {
            if (ETName.Text == "")
            {
                Toast.MakeText(this, $"Field {ETName.Hint} cannot be empty", ToastLength.Short).Show();
                return;
            }
            if(ETIdentifier.Text == "")
            {
                Toast.MakeText(this, $"Field {ETIdentifier.Hint} cannot be empty", ToastLength.Short).Show();
                return;
            }
            var query = from r in projects.projects
                        where r.name == SParent.SelectedItem.ToString()
                        select r.id;

                Toast.MakeText(this, RedMineManager.PutPostProject(new Project { name = ETName.Text, identifier = ETIdentifier.Text, is_public = CBIsPublic.Checked, description = ETDescrtiption.Text, parent_id = query.SingleOrDefault() }, Intent.GetBooleanExtra("IsNew", false)), ToastLength.Short).Show();
            Finish();
        }
    }
}