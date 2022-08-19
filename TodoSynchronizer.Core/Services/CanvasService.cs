using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TodoSynchronizer.Core.Helpers;
using TodoSynchronizer.Core.Models;
using TodoSynchronizer.Core.Models.CanvasModels;
using TodoSynchronizer.Core.Service;

namespace TodoSynchronizer.Core.Services
{
    public class CanvasService
    {
        public static string Token { get; set; }

        public static bool IsLogin { get; set; }

        public static UserProfile User { get; set; }

        public static CommonResult Login(string token)
        {
            var headers = new Dictionary<string,string>();
            headers.Add("Authorization", $"Bearer {token}");

            var res = Web.Get("https://oc.sjtu.edu.cn/api/v1/users/self/profile", headers);

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
            var headers = new Dictionary<string, string>();
            var query = new Dictionary<string, string>();
            query.Add("enrollment_state", "active");

            return GetAllPageResult<Course>("https://oc.sjtu.edu.cn/api/v1/courses", headers, query);
        }

        public static List<Assignment> ListAssignments(string course_id)
        {
            var headers = new Dictionary<string, string>();
            var query = new Dictionary<string, string>();

            return GetAllPageResult<Assignment>($"https://oc.sjtu.edu.cn/api/v1/courses/{course_id}/assignments", headers, query);
        }

        public static List<Quiz> ListQuizes(string course_id)
        {
            var headers = new Dictionary<string, string>();
            var query = new Dictionary<string, string>();

            return GetAllPageResult<Quiz>($"https://oc.sjtu.edu.cn/api/v1/courses/{course_id}/quizzes", headers, query);
        }

        public static List<Anouncement> ListAnouncements(string course_id)
        {
            var headers = new Dictionary<string, string>();
            var query = new Dictionary<string, string>();
            query.Add("only_announcements", "true");
            query.Add("filter_by", "all");

            return GetAllPageResult<Anouncement>($"https://oc.sjtu.edu.cn/api/v1/courses/{course_id}/discussion_topics", headers, query);
        }

        public static List<Discussion> ListDiscussions(string course_id)
        {
            var headers = new Dictionary<string, string>();
            var query = new Dictionary<string, string>();

            return GetAllPageResult<Discussion>($"https://oc.sjtu.edu.cn/api/v1/courses/{course_id}/discussion_topics", headers, query);
        }

        public static AssignmentSubmission GetAssignmentSubmisson(string course_id, string assignment_id)
        {
            var headers = new Dictionary<string, string>();
            var query = new Dictionary<string, string>();
            headers.Add("Authorization", $"Bearer {Token}");

            var res = Web.Get($"https://oc.sjtu.edu.cn/api/v1/courses/{course_id}/assignments/{assignment_id}/submissions/self", headers, query);
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
            var headers = new Dictionary<string, string>();
            var query = new Dictionary<string, string>();
            headers.Add("Authorization", $"Bearer {Token}");
            query.Add("page", "1");
            query.Add("per_page", "20");

            var res = Web.Get($"https://oc.sjtu.edu.cn/api/v1/courses/{course_id}/quizzes/{quiz_id}/submissions", headers, query);
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

        public static List<T> GetAllPageResult<T>(string url, Dictionary<string, string> headers, Dictionary<string, string> query)
        {
            headers.Add("Authorization", $"Bearer {Token}");
            query.Add("page", "1");
            query.Add("per_page", "10");

            var res = Web.Get(url, headers, query);
            if (!res.success)
                throw new Exception(res.message);

            if (res.code != System.Net.HttpStatusCode.OK)
                return null;

            try
            {
                var json = JsonConvert.DeserializeObject<List<T>>(res.result);
                if (res.headers.AllKeys.Contains("Link"))
                {
                    var link = res.headers.Get("Link");
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
                            var res2 = Web.Get(url, headers, query); ;
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
