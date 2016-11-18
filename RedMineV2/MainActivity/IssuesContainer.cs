using System.Collections.Generic;

namespace RedMineV2.MainActivity
{
    public enum PRIORITY { Low = 1, Normal = 2, High = 3, Urgent = 4, Immediate = 5 }
    public enum ACTIVITY { Design = 1, Development = 2 }
    public enum STATUS { New = 1, Progress = 2, Resolved = 3, Feedback = 4, Closed = 5, Rejected = 6 }
    public enum TRACKER { Bug = 1, Feature = 2, Support = 3, Design = 4, Documentation = 5, Development = 6 }

    public class IssuesContainer
    {
        public List<Issue> issues { get; set; }
        public int total_count { get; set; }
        public int offset { get; set; }
        public int limit { get; set; }

        public static IssuesContainer GetIssues(System.Net.WebClient client)
        {
            string jsonIssues = client.DownloadString(client.BaseAddress + @"/issues.json?include=journals,changesets");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<IssuesContainer>(jsonIssues);

        }

        public static Issue GetIssue(System.Net.WebClient client, int issueId)
        {
            string jsonIssue = client.DownloadString(client.BaseAddress + @"/issues/" + issueId.ToString() + ".json?include=journals");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Issue>(jsonIssue);
        }


        public class Project
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Tracker
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Status
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Priority
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Author
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class AssignedTo
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class User
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Journal
        {
            public int id { get; set; }
            public User user { get; set; }
            public string notes { get; set; }
            public string created_on { get; set; }
            public List<object> details { get; set; }
        }

        public class activity
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class TimeEntry
        {
            public int id { get; set; }
            public Project project { get; set; }
            public Issue issue { get; set; }
            public User user { get; set; }
            public activity activity { get; set; }
            public double hours { get; set; }
            public string comments { get; set; }
            public string spent_on { get; set; }
            public string created_on { get; set; }
            public string updated_on { get; set; }
        }

        public class Issue
        {

            public int id = new int();
            public Project project { get; set; }
            public int project_id{get;set;}
            public Tracker tracker { get; set; }
            public Status status { get; set; }
            public Priority priority { get; set; }
            public Author author { get; set; }
            public AssignedTo assigned_to { get; set; }
            public string subject { get; set; }
            public string description { get; set; }
            public string start_date { get; set; }
            public string due_date { get; set; }
            public int done_ratio { get; set; }
            public double estimated_hours { get; set; }
            public double spent_hours { get; set; }
            public string created_on { get; set; }
            public string updated_on { get; set; }
            public List<Journal> journals { get; set; }
            public List<TimeEntry> timeEntries { get; set; }

        }
    }
}