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

namespace RedMineV2
{
    public static class Utils
    {
        public static string RemoveRoot(this string s, string root)
        {
            return s.Remove(s.Length - 1, 1).Remove(0, 4 + root.Length);
        }

    }
}