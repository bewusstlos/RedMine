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
    class ProjectsContainer
    {
        public List<Project> projects { get; set; }
        public int total_count { get; set; }
        public int offset { get; set; }
        public int limit { get; set; }

        public static ProjectsContainer GetProjects(System.Net.WebClient client)
        {
            string jsonProjects = client.DownloadString(client.BaseAddress + @"/projects.json");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ProjectsContainer>(jsonProjects);
        }

        public static List<int> GetProjectsGroupId()
        {
            var query = from r in RedMineManager.Get<ProjectsContainer>("/projects.json").projects
                        where r.parent == null
                        select r.id;
            var result = query.ToList();
            return result;
        }

        public static Dictionary<int, List<int>> GetProjectsItemsId(ProjectsContainer projectContainer)
        {
            var result = new Dictionary<int, List<int>>();
            foreach (var project in projectContainer.projects)
            {
                var query = from r in projectContainer.projects
                            where r.parent != null && r.parent.id == project.id
                            select r.id;
                result.Add(project.id, query.ToList());
            }
            return result;
        }

        public Project GetById(int id)
        {
            var query = from r in projects
                        where r.id == id
                        select r;

            return query.SingleOrDefault();
        }
    }

    public class Parent
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Role
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Membership
    {
        public int id { get; set; }
        public Project project { get; set; }
        public IssuesContainer.User user { get; set; }
        public List<Role> roles { get; set; }
    }

    public class Project
    {
        public int id { get; set; }
        public string name { get; set; }
        public string identifier { get; set; }
        public string description { get; set; }
        public int status { get; set; }
        public bool is_public { get; set; }
        public string created_on { get; set; }
        public string updated_on { get; set; }
        public Parent parent { get; set; }
        public int parent_id { get; set; }
        public List<Membership> memberships { get; set; }

        public IssuesContainer projectIssues { get; set; }

        public Project()
        {
           
        }
    }
}