using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TodoSynchronizer.Core.Helpers;
using TodoSynchronizer.Core.Models;
using TodoSynchronizer.Core.Models.CanvasModels;
using TodoSynchronizer.Core.Models.DidaModels;
using TodoSynchronizer.Core.Service;
using YamlDotNet.Core.Tokens;

namespace TodoSynchronizer.Core.Services
{
    public class DidaService
    {
        public static string Cookie { get; set; }

        public static bool IsLogin { get; set; }

        public static HttpClient Client { get; set; }

        public static CommonResult Login(string info)
        {
            Client = new HttpClient()
            {
                BaseAddress = new Uri("https://api.dida365.com")
            };

            // The V2 session API expects web-client metadata on login and subsequent requests.
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:95.0) Gecko/20100101 Firefox/95.0");
            var deviceInfo = new
            {
                platform = "web",
                os = "Linux",
                device = "Firefox 95.0",
                name = "TodoSynchronizer",
                version = 8240, // Dida web client version observed on 2026-10-28.
                id = Guid.NewGuid().ToString("N").Substring(0, 24),
                channel = "website",
                campaign = "",
                websocket = ""
            };
            Client.DefaultRequestHeaders.Add("X-Device", JsonConvert.SerializeObject(deviceInfo));

            var authStage = "credential_json";
            try
            {
                var credential = JsonConvert.DeserializeAnonymousType(info, new { token = (string)null });
                if (credential?.token != null)
                {
                    if (string.IsNullOrWhiteSpace(credential.token) || credential.token.Any(char.IsWhiteSpace) || credential.token.Contains(";"))
                        return new CommonResult(false, "滴答会话 token 格式错误：请仅填写 Cookie t 的值");
                    authStage = "cookie_header";
                    Client.DefaultRequestHeaders.Add("Cookie", $"t={credential.token}");
                    authStage = "session_request";
                    var check = Web.Get(Client, "/api/v2/batch/check/0");
                    if (check.code != System.Net.HttpStatusCode.OK)
                        return new CommonResult(false, $"滴答会话验证失败，HTTP 状态码：{(int)check.code}；请检查或更新 DIDA_CREDENTIAL 中的 token");
                    authStage = check.result?.TrimStart().StartsWith("<") == true ? "session_html" : "session_json";
                    var data = JsonConvert.DeserializeObject<DidaBatchCheckDto>(check.result);
                    if (data?.ProjectProfiles == null || data.SyncTaskBean?.Update == null)
                        return new CommonResult(false, "滴答会话验证失败：同步接口未返回有效数据");
                    return new CommonResult(true, "滴答会话认证成功");
                }
            }
            catch (Exception ex)
            {
                var cause = ex is JsonException ? ex : ex.GetBaseException();
                var kind = cause is JsonException ? "json_parse" : cause is HttpRequestException ? "network" : cause is TaskCanceledException ? "timeout" : cause is FormatException ? "format" : "unexpected";
                var field = "unknown";
                var path = (cause as JsonSerializationException)?.Path ?? (cause as JsonReaderException)?.Path ?? "";
                field = new[] { "projectGroups", "tags", "projectProfiles", "syncTaskBean", "checkPoint", "inboxId" }.FirstOrDefault(name => path == name || path.StartsWith(name + "[") || path.StartsWith(name + ".")) ?? "unknown";
                // Only fixed categories are logged; exception messages can contain private task data.
                return new CommonResult(false, $"滴答会话验证失败：阶段={authStage}，类型={kind}，字段={field}；未尝试密码登录");
            }

            var res = Web.Post(Client, "/api/v2/user/signon?wc=true&remember=true", info);

            if (!res.success)
                return new CommonResult(false, res.message);
            if (res.code != System.Net.HttpStatusCode.OK)
            {
                var detail = "empty_response";
                if (!string.IsNullOrWhiteSpace(res.result))
                {
                    try
                    {
                        var failure = JsonConvert.DeserializeAnonymousType(res.result, new { errorCode = "" });
                        var code = failure?.errorCode;
                        var credential = JsonConvert.DeserializeAnonymousType(info, new { phone = "", password = "" });
                        // Log only a short symbolic error code, never the response body or credentials.
                        detail = !string.IsNullOrEmpty(code) && System.Text.RegularExpressions.Regex.IsMatch(code, "^[a-z][a-z_]{0,63}$") && (string.IsNullOrEmpty(credential?.phone) || !code.Contains(credential.phone)) && (string.IsNullOrEmpty(credential?.password) || !code.Contains(credential.password))
                            ? code : "json_without_safe_error_code";
                    }
                    catch (JsonException)
                    {
                        detail = res.result.TrimStart().StartsWith("<") ? "html_response" : "non_json_response";
                    }
                }
                return new CommonResult(false, $"滴答清单登录失败，HTTP 状态码：{(int)res.code} ({res.code})，错误类型：{detail}");
            }
            try
            {
                var json = JsonConvert.DeserializeObject<LoginDto>(res.result);
                if (string.IsNullOrWhiteSpace(json?.Token))
                    return new CommonResult(false, "滴答清单登录失败：接口未返回有效 Token");
                Client.DefaultRequestHeaders.Add("Cookie", $"t={json.Token}");
                return new CommonResult(true, "登录成功");
            }
            catch (Exception ex)
            {
                return new CommonResult(false, ex.Message);
            }
        }

        //public static List<DidaTaskList> ListLists()
        //{
        //    var query = new Dictionary<string, string>();

        //    var res = Web.Get(Client, $"/api/v2/projects", query);
        //    if (!res.success)
        //        throw new Exception(res.message);

        //    if (res.code != System.Net.HttpStatusCode.OK)
        //        return null;

        //    try
        //    {
        //        var json = JsonConvert.DeserializeObject<List<DidaTaskList>>(res.result);
        //        return json;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        public static DidaTaskList AddTaskList(string name)
        {
            var query = new Dictionary<string, string>();
            var postjson = "{\"name\":\"{1}\"}".Replace("{1}", name);

            var res = Web.Post(Client, $"/api/v2/project", postjson);
            if (!res.success)
                throw new Exception(res.message);

            if (res.code != System.Net.HttpStatusCode.OK)
                return null;

            try
            {
                var json = JsonConvert.DeserializeObject<DidaTaskList>(res.result);
                return json;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static DidaBatchCheckDto BatchCheck()
        {
            var query = new Dictionary<string, string>();

            var res = Web.Get(Client, $"/api/v2/batch/check/0", query);
            if (!res.success)
                throw new Exception(res.message);

            if (res.code != System.Net.HttpStatusCode.OK)
                throw new Exception($"滴答清单读取失败，HTTP 状态码：{(int)res.code} ({res.code})");

            try
            {
                var json = JsonConvert.DeserializeObject<DidaBatchCheckDto>(res.result);
                return json ?? throw new Exception("滴答清单接口返回了空数据");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static List<DidaTask> GetCompleted(string listid)
        {
            var query = new Dictionary<string, string>();
            query.Add("limit", "9999");

            var res = Web.Get(Client, $"/api/v2/project/{listid}/completed/", query);
            if (!res.success)
                throw new Exception(res.message);

            if (res.code != System.Net.HttpStatusCode.OK)
                return null;

            try
            {
                var json = JsonConvert.DeserializeObject<List<DidaTask>>(res.result);
                return json;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static void AddTask(DidaTask task)
        {
            var query = new Dictionary<string, string>();
            var batchdto = new DidaBatchDto();
            batchdto.add.Add(task);

            var res = Web.Post(Client, $"/api/v2/batch/task", JsonConvert.SerializeObject(batchdto));
            if (!res.success)
                throw new Exception(res.message);

            if (res.code != System.Net.HttpStatusCode.OK)
                //return null;
                throw new Exception(res.message);

            //try
            //{
            //    var json = JsonConvert.DeserializeObject<DidaTaskList>(res.result);
            //    return json;
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(ex.Message);
            //}
        }

        public static void UpdateTask(DidaTask task)
        {
            var query = new Dictionary<string, string>();
            var batchdto = new DidaBatchDto();
            batchdto.update.Add(task);

            var res = Web.Post(Client, $"/api/v2/batch/task", JsonConvert.SerializeObject(batchdto));
            if (!res.success)
                throw new Exception(res.message);

            if (res.code != System.Net.HttpStatusCode.OK)
                //return null;
                throw new Exception(res.message);

            //try
            //{
            //    var json = JsonConvert.DeserializeObject<DidaTaskList>(res.result);
            //    return json;
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(ex.Message);
            //}
        }

        public static List<DidaTaskComment> ListTaskComments(string listid, string taskid)
        {
            var query = new Dictionary<string, string>();

            var res = Web.Get(Client, $"/api/v2/project/{listid}/task/{taskid}/comments", query);
            if (!res.success)
                throw new Exception(res.message);

            if (res.code != System.Net.HttpStatusCode.OK)
                return null;

            try
            {
                var json = JsonConvert.DeserializeObject<List<DidaTaskComment>>(res.result);
                return json;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static void AddTaskComment(string listid, string taskid, string content)
        {
            var query = new Dictionary<string, string>();
            var comment = new DidaTaskComment();
            comment.TaskId = taskid;
            comment.ProjectId = listid;
            comment.IsNew = true;
            comment.Title = content;
            comment.Id = Common.GetRandomString(24, true, false, false, false, "abcdef");
            comment.UserProfile = new Models.DidaModels.UserProfile() { IsMyself = true };

            var res = Web.Post(Client, $"/api/v2/project/{listid}/task/{taskid}/comment", JsonConvert.SerializeObject(comment));
            if (!res.success)
                throw new Exception(res.message);

            if (res.code != System.Net.HttpStatusCode.OK)
                //return null;
                throw new Exception(res.message);
        }
    }
}
