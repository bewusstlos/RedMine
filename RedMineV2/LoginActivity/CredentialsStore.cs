using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using System.IO;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace RedMineV2.LoginActivity
{
    static class CredentialsStore
    {
        static string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"/redmine.txt";

        public static void Save(string login, string pass)
        {
            
                File.WriteAllLines(path, new string[] {login, pass});
        }

        public static string [] Load()
        {
            if (File.Exists(path))
                return File.ReadAllLines(path);
            else
                return null;
        }

        public static void Delete()
        {
            File.Delete(path);
        }
    }
}