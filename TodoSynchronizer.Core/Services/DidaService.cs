using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using TodoSynchronizer.Core.Helpers;
using TodoSynchronizer.Core.Models;
using TodoSynchronizer.Core.Models.DidaModels;
using TodoSynchronizer.Core.Service;

namespace TodoSynchronizer.Core.Services
{
    public class DidaService
    {
        public static string Cookie { get; set; }

        public static bool IsLogin { get; set; }

        public static HttpClient Client { get; set; }

        private const string BatchCheckPath = "/api/v2/batch/check/0";

        public static CommonResult Login(string info)
        {
            string token;
            try
            {
                var credential = JsonConvert.DeserializeAnonymousType(info, new { token = (string)null });
                if (credential == null)
                    return new CommonResult(false, "滴答清单登录信息为空");
                token = credential.token;
            }
            catch (JsonException)
            {
                return new CommonResult(false, "滴答清单登录信息不是有效的 JSON");
            }

            Client = new HttpClient()
            {
                BaseAddress = new Uri("https://api.dida365.com")
            };
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:95.0) Gecko/20100101 Firefox/95.0");
            var deviceInfo = new
            {
                platform = "web",
                os = "Linux",
                device = "Firefox 95.0",
                name = "TodoSynchronizer",
                version = 8240, // 滴答清单网页版 2026-10-28 的版本号
                id = Guid.NewGuid().ToString("N").Substring(0, 24),
                channel = "website",
                campaign = "",
                websocket = ""
            };
            Client.DefaultRequestHeaders.Add("X-Device", JsonConvert.SerializeObject(deviceInfo));

            try
            {
                return token == null ? LoginWithPassword(Client, info) : LoginWithSession(Client, token);
            }
            catch (AggregateException ex) when (ex.GetBaseException() is OperationCanceledException)
            {
                return new CommonResult(false, "请求滴答清单登录接口超时");
            }
            catch (AggregateException ex) when (ex.GetBaseException() is HttpRequestException)
            {
                return new CommonResult(false, "连接滴答清单失败，请检查网络连接");
            }
        }

        private static CommonResult LoginWithSession(HttpClient client, string token)
        {
            if (string.IsNullOrWhiteSpace(token) || token.StartsWith("t=", StringComparison.Ordinal)
                || token.Any(char.IsWhiteSpace) || token.Any(char.IsControl) || token.Contains(';'))
                return new CommonResult(false, "滴答会话 token 格式错误：请仅填写 Cookie t 的值");

            client.DefaultRequestHeaders.Add("Cookie", $"t={token}");
            var res = Web.Get(client, BatchCheckPath);
            if (res.code != HttpStatusCode.OK)
                return new CommonResult(false, $"滴答会话验证失败，HTTP 状态码：{(int)res.code} ({res.code})");

            try
            {
                ParseBatchCheck(res.result);
                return new CommonResult(true, "滴答会话认证成功");
            }
            catch (InvalidDataException ex)
            {
                return new CommonResult(false, $"滴答会话验证失败：{ex.Message}");
            }
        }

        private static CommonResult LoginWithPassword(HttpClient client, string info)
        {
            var res = Web.Post(client, "/api/v2/user/signon?wc=true&remember=true", info);
            if (res.code != HttpStatusCode.OK)
            {
                var errorCode = GetLoginErrorCode(res.result);
                return new CommonResult(false, $"滴答清单登录失败，HTTP 状态码：{(int)res.code} ({res.code})，错误类型：{errorCode}");
            }

            try
            {
                var login = JsonConvert.DeserializeObject<LoginDto>(res.result);
                if (string.IsNullOrWhiteSpace(login?.Token))
                    return new CommonResult(false, "滴答清单登录失败：接口未返回有效 Token");

                client.DefaultRequestHeaders.Add("Cookie", $"t={login.Token}");
                return new CommonResult(true, "登录成功");
            }
            catch (JsonException)
            {
                return new CommonResult(false, "滴答清单登录失败：无法解析登录响应");
            }
        }

        private static string GetLoginErrorCode(string response)
        {
            try
            {
                var error = JsonConvert.DeserializeAnonymousType(response, new { errorCode = "" });
                return error?.errorCode switch
                {
                    "need_captcha" => "need_captcha",
                    "need_upgrade_client" => "need_upgrade_client",
                    "username_password_not_match" => "username_password_not_match",
                    _ => "unknown"
                };
            }
            catch (JsonException)
            {
                return "unknown";
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
            var res = Web.Get(Client, BatchCheckPath);
            if (res.code != HttpStatusCode.OK)
                throw new InvalidDataException($"滴答清单读取失败，HTTP 状态码：{(int)res.code} ({res.code})");

            return ParseBatchCheck(res.result);
        }

        private static DidaBatchCheckDto ParseBatchCheck(string response)
        {
            DidaBatchCheckDto data;
            try
            {
                data = JsonConvert.DeserializeObject<DidaBatchCheckDto>(response);
            }
            catch (JsonException ex)
            {
                var path = (ex as JsonSerializationException)?.Path ?? (ex as JsonReaderException)?.Path ?? "";
                var field = path.Split('.', '[')[0];
                // 只输出已知字段名，避免将响应中的任务内容写入日志
                var knownFields = new[] { "projectGroups", "tags", "projectProfiles", "syncTaskBean", "checkPoint", "inboxId", "filters" };
                var detail = knownFields.Contains(field) ? $"（字段：{field}）" : "";
                throw new InvalidDataException($"无法解析滴答清单同步响应{detail}");
            }

            if (data?.ProjectProfiles == null || data.SyncTaskBean?.Update == null)
                throw new InvalidDataException("滴答清单同步接口未返回清单或任务数据");

            return data;
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
