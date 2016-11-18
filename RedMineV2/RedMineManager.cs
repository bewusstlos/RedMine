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
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace RedMineV2
{

    public class RedMineManager
    {
        public enum Priorities { Low = 1, Normal = 2, High = 3, Urgent = 4, Immediate = 5 }
        public enum Activities { Design = 0, Development = 1 }
        public enum Trackers { Bug = 1, Feature = 2, Support = 3, Design = 6, Documentation = 7, Development = 9 }
        public enum IssueStatuses { New = 1, InProgress = 2, Resolved = 3, Feedback = 4, Closed = 5, Rejected = 6 }
        public enum UserRoles { Manager = 3, Developer = 4, Reporter = 5, Administrator = 6, Client = 7 }

        public enum GETPROPERTIES
        {
            issues = 0, projects = 1, users = 2
        }

        public static List<string[]> PROPERTIES = new List<string[]> { new string[2] {"/issues.json?include=journals,changesheets","issues"},
        new string[2] {"/projects.json?limit=100","projects"},new string[2] {"/users.json?limit=100","users"}};



        public static RestClient client;
        public static MainActivity.User currUser;

        public RedMineManager(string login, string pass, string domen = @"http://dev.bidon-tech.com:65500/redmine")
        {
            client = new RestClient(domen);
            client.Authenticator = new RestSharp.Authenticators.HttpBasicAuthenticator(login, pass);
            currUser = RedMineManager.Get<MainActivity.User>("/users/current.json","user");
        }

        public static T Get<T>(string segmentUri, string rootObj = null)
        {
            var request = new RestRequest(segmentUri, Method.GET);
            request.AddHeader("content-type", "application/json");
            request.RootElement = rootObj;
            
            var response = client.Execute(request);
            JsonSerializerSettings s = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Serialize };
            if (rootObj != null)
                return JsonConvert.DeserializeObject<T>(response.Content.RemoveRoot(rootObj));
            else
                return JsonConvert.DeserializeObject<T>(response.Content, s);
        }

        public static string Delete(string segmentUri)
        {
            var request = new RestRequest(segmentUri, Method.DELETE);
            var response = client.Execute(request);
            return response.StatusCode +": " + response.StatusDescription;
        }

        public static string PostMemberShip(string segmentUri, object memberships)
        {
            RestRequest request = new RestRequest(segmentUri,Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddJsonBody(memberships);
            var response = client.Execute(request);
            return response.StatusCode.ToString();
        }

        public static string PutPostTimeEntry(MainActivity.IssuesContainer.TimeEntry timeEntry, bool isNew = false)
        {
            RestRequest request;
            if (isNew)
                request = new RestRequest("/time_entries.json", Method.POST);
            else
                request = new RestRequest("/issues/" + timeEntry.issue.id + "/time_entries/" + timeEntry.id + ".json", Method.PUT);

            request.AddHeader("content-type", "application/json");
            request.RequestFormat = DataFormat.Json;
            request.RootElement = "time_entries";
            request.AddJsonBody(new
            {
                time_entry = new
                {
                    issue_id = timeEntry.issue.id,
                    hours = timeEntry.hours,
                    activity_id = 9,
                    comments = timeEntry.comments
                } 
            });
            var response = client.Execute(request);
            return response.Content;
        }

        public static string PutPostIssue(MainActivity.IssuesContainer.Issue currIssue, bool isNew = false, string addNotes = null)
        {
            RestRequest request;
            if (isNew == false)
                request = new RestRequest("/issues/" + currIssue.id + ".json", Method.PUT);
            else
                request = new RestRequest("/issues.json", Method.POST);
            request.RequestFormat = DataFormat.Json;
                request.AddJsonBody(new
                {
                    issue = new
                    {
                        project_id = currIssue.project.id,
                        priority_id = currIssue.priority.id,
                        status_id = currIssue.status.id,
                        start_date = currIssue.start_date,
                        tracker_id = currIssue.tracker.id,
                        due_date = currIssue.due_date,
                        assigned_to_id = currIssue.assigned_to.id,
                        subject = currIssue.subject,
                        description = currIssue.description,
                        estimated_hours = currIssue.estimated_hours,
                        done_ratio = currIssue.done_ratio,
                        notes = addNotes ?? null
                    }
                });
            request.RootElement = "issue";
            request.AddHeader("content-type", "application/json");
            RestResponse response = (RestResponse)client.Execute(request);
            return response.Content;
        }

        public static int [] ValidateUserForAction(int projectId)
        {
            int [] result;
            List<MainActivity.Membership> memberships = RedMineManager.Get<List<MainActivity.Membership>>("/projects/" + projectId + "/memberships.json?nometa=1", "mebmerships");
            var query = from r in memberships
                        where r.user.id == currUser.id
                        select r.roles;
            try
            {
                var intQuery = from g in query.SingleOrDefault()
                               select g.id;
                result = intQuery.ToArray();
                return result;
            }
            catch
            {
                return new int[1] {0};
            }
            
        }

        public static string PutPostProject(MainActivity.Project currProj, bool isNew = false)
        {
            RestRequest request;
            if (isNew == false)
                request = new RestRequest("/projects/" + currProj.id + ".json", Method.PUT);
            else
                request = new RestRequest("/projects.json", Method.POST);
            request.AddHeader("content-type", "application/json");
            if(currProj.parent_id!=0)
            request.AddJsonBody(new
            {
                project = new
                {
                    name = currProj.name,
                    identifier = currProj.identifier,
                    is_public = currProj.is_public,
                    description = currProj.description ?? null,
                    parent_id = currProj.parent_id
                }
            });
            else
                request.AddJsonBody(new
                {
                    project = new
                    {
                        name = currProj.name,
                        identifier = currProj.identifier,
                        is_public = currProj.is_public,
                        description = currProj.description ?? null,
                    }
                });

            var response = client.Execute(request);
            return response.Content;
        }
    }
}