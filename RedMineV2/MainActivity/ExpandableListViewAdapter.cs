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
using Android.Graphics;
using Android.Content.Res;

namespace RedMineV2.MainActivity
{
    class ExpandableListViewAdapter:BaseExpandableListAdapter
    {
        private Context context;
        private List<int> listGroup;
        private Dictionary<int, List<int>> listChild;
        private List<Project> projects;

        public ExpandableListViewAdapter(Context context, List<int> listGroup, Dictionary<int, List<int>> listChild, List<Project> projects)
        {
            this.context = context;
            this.listGroup = listGroup;
            this.listChild = listChild;
            this.projects = projects;
        }

        public override int GroupCount
        {
            get
            {
                return listGroup.Count;
            }
        }

        public override bool HasStableIds
        {
            get
            {
                return true;
            }
        }

        public override Java.Lang.Object GetChild(int groupPosition, int childPosition)
        {
            var result = new List<int>();
            listChild.TryGetValue(listGroup[groupPosition], out result);
            return result[childPosition];
        }

        public override long GetChildId(int groupPosition, int childPosition)
        {
            return childPosition;
        }

        public override int GetChildrenCount(int groupPosition)
        {
            var result = new List<int>();
            listChild.TryGetValue(listGroup[groupPosition], out result);
            return result.Count;
        }

        public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
        {
            if (convertView == null)
            {
                LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                convertView = inflater.Inflate(Resource.Layout.listview_item_layout, null);
            }
            TextView TVItem = convertView.FindViewById<TextView>(Resource.Id.item);
            var query = from r in projects
                        where r.id == (int)GetChild(groupPosition, childPosition)
                        select r.name;

            TVItem.Text = query.SingleOrDefault();
            return convertView;
        }

        public override Java.Lang.Object GetGroup(int groupPosition)
        {
            return listGroup[groupPosition];
        }

        public override long GetGroupId(int groupPosition)
        {
            return groupPosition;
        }

        public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
        {
            if (convertView == null)
            {
                LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                convertView = inflater.Inflate(Resource.Layout.listview_group_layout, null);
            }
            TextView TVGroup = convertView.FindViewById<TextView>(Resource.Id.group);
            TextView TVGroupItemsCount = convertView.FindViewById<TextView>(Resource.Id.group_item_count);

            var query = from r in projects
                        where r.id == (int)GetGroup(groupPosition)
                        select r.name;
            TVGroup.Text = query.SingleOrDefault();
            TVGroupItemsCount.Text = GetChildrenCount(groupPosition).ToString();

            if(GetChildrenCount(groupPosition) == 0)
                TVGroupItemsCount.Visibility = ViewStates.Invisible;
            return convertView;
        }

        public override bool IsChildSelectable(int groupPosition, int childPosition)
        {
            return true;
        }
    }
}