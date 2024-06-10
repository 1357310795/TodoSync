using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TodoSynchronizer.Core.Helpers;
using TodoSynchronizer.Core.Models;
using TodoSynchronizer.Core.Models.CanvasModels;
using TodoSynchronizer.Core.Service;
using YamlDotNet.Core.Tokens;

namespace TodoSynchronizer.Core.Services
{
    public class CanvasService
    {
        public static string Token { get; set; }

        public static bool IsLogin { get; set; }

        public static UserProfile User { get; set; }
        
        public static HttpClient Client { get; set; }

        public static CommonResult Login(string token)
        {
            Client = new HttpClient()
            {
                BaseAddress = new Uri("https://oc.sjtu.edu.cn")
            };
            Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var res = Web.Get(Client, "/api/v1/users/self/profile");

            if (!res.success)
                return new CommonResult(false, res.message);
            if (res.code == System.Net.HttpStatusCode.Unauthorized)
                return new CommonResult(false, "AccessToken无效");
            try
            {
                var json = JsonConvert.DeserializeObject<UserProfile>(res.result);
                IsLogin = true;
                Token = token;
                User = json;
                return new CommonResult(true, "登录成功");
            }
            catch(Exception ex)
            {
                return new CommonResult(false, ex.Message);
            }
        }

        //public static CommonResult TryCacheLogin()
        //{
        //    var token = IniHelper.GetKeyValue("canvas", "token", "");
        //    if (token == "")
        //        return new CommonResult(false, "无缓存");
        //    return Login(token);
        //}

        public static List<Course> ListCourses()
        {
            var query = new Dictionary<string, string>();
            query.Add("enrollment_state", "active");

            return GetAllPageResult<Course>("/api/v1/courses", query);
        }

        public static List<Assignment> ListAssignments(string course_id)
        {
            var query = new Dictionary<string, string>();

            return GetAllPageResult<Assignment>($"/api/v1/courses/{course_id}/assignments", query);
        }

        public static List<Quiz> ListQuizes(string course_id)
        {
            var query = new Dictionary<string, string>();

            return GetAllPageResult<Quiz>($"/api/v1/courses/{course_id}/quizzes", query);
        }

        public static List<Announcement> ListAnnouncements(string course_id)
        {
            var query = new Dictionary<string, string>();
            query.Add("only_announcements", "true");
            query.Add("filter_by", "all");

            return GetAllPageResult<Announcement>($"/api/v1/courses/{course_id}/discussion_topics", query);
        }

        public static List<Discussion> ListDiscussions(string course_id)
        {
            var query = new Dictionary<string, string>();

            return GetAllPageResult<Discussion>($"/api/v1/courses/{course_id}/discussion_topics", query);
        }

        public static AssignmentSubmission GetAssignmentSubmisson(string course_id, string assignment_id)
        {
            var query = new Dictionary<string, string>();
            query.Add("include[]", "submission_comments");

            var res = Web.Get(Client, $"/api/v1/courses/{course_id}/assignments/{assignment_id}/submissions/self", query);
            if (!res.success)
                throw new Exception(res.message);

            if (res.code != System.Net.HttpStatusCode.OK)
                return null;

            try
            {
                var json = JsonConvert.DeserializeObject<AssignmentSubmission>(res.result);
                return json;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static List<QuizSubmission> ListQuizSubmissons(string course_id, string quiz_id)
        {
            var query = new Dictionary<string, string>();
            query.Add("page", "1");
            query.Add("per_page", "20");

            var res = Web.Get(Client, $"/api/v1/courses/{course_id}/quizzes/{quiz_id}/submissions", query);
            if (!res.success)
                throw new Exception(res.message);

            if (res.code != System.Net.HttpStatusCode.OK)
                return null;

            try
            {
                var json = JsonConvert.DeserializeObject<QuizSubmissionDto>(res.result);
                return json.QuizSubmissions;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static List<Notification> ListNotifications()
        {
            var query = new Dictionary<string, string>();

            var res = Web.Get(Client, $"/api/v1/accounts/self/account_notifications", query);
            if (!res.success)
                throw new Exception(res.message);

            if (res.code != System.Net.HttpStatusCode.OK)
                return null;

            try
            {
                var json = JsonConvert.DeserializeObject<List<Notification>>(res.result);
                return json;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static List<T> GetAllPageResult<T>(string url, Dictionary<string, string> query)
        {
            query.Add("page", "1");
            query.Add("per_page", "10");

            var res = Web.Get(Client, url, query);
            if (!res.success)
                throw new Exception(res.message);

            if (res.code != System.Net.HttpStatusCode.OK)
                return null;

            try
            {
                var json = JsonConvert.DeserializeObject<List<T>>(res.result);
                if (res.headers1.Contains("Link"))
                {
                    var link = res.headers1.FirstOrDefault(x => x.Key == "Link").Value.FirstOrDefault();
                    var reg = new Regex(@"https:\/\/[.\w\-\@?^=%&/~\+#]+?page=(\d).+?rel=""(\b\w+?\b)""");
                    var matchres = reg.Matches(link);
                    int pagecount = 0;
                    foreach (Match match in matchres)
                        if (match.Groups[2].Value == "last")
                            pagecount = int.Parse(match.Groups[1].Value);
                    if (pagecount != 0)
                    {
                        for (int i = 2; i <= pagecount; i++)
                        {
                            query["page"] = i.ToString();
                            var res2 = Web.Get(Client, url, query);
                            if (!res2.success)
                                throw new Exception(res2.message);

                            if (res2.code != System.Net.HttpStatusCode.OK)
                                return null;
                            var json2 = JsonConvert.DeserializeObject<List<T>>(res2.result);

                            json = json.Concat(json2).ToList();
                        }
                    }
                }
                return json;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
