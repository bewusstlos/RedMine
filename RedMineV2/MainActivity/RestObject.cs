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

namespace RedMineV2.MainActivity
{
    public class RestObject
    {
        public int id { get; set; }

        public override string ToString()
        {
            return "/" + this.GetType().Name.ToLower() + "s/" + id + ".json";
        }
    }
}