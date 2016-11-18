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
using System.Net;
using RestSharp;
using System.Threading.Tasks;

namespace RedMineV2.MainActivity
{
    public class User
    {
        public int id { get; set; }
        public string login { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string mail { get; set; }
        public string created_on { get; set; }
        public string last_login_on { get; set; }
        public int status { get; set; }
    }

    public class UsersContainer
    {
        public List<User> users { get; set; }
        public int total_count { get; set; }
        public int offset { get; set; }
        public int limit { get; set; }

        public static UsersContainer GetUsers(RedMineManager.GETPROPERTIES property,ProgressDialog pd, out bool isAdmin)
        {
            try
            {
                UsersContainer result;
                result = RedMineManager.Get<UsersContainer>("/users.json?limit=100");
                isAdmin = true;
                return result;
            }
            catch
            {

                isAdmin = false;
                return null;
            }
        }
    }
}