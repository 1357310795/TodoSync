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

            var res = Web.Post(Client, "/api/v2/user/signon?wc=true&remember=true", info);

            if (!res.success)
                return new CommonResult(false, res.message);
            if (res.code == System.Net.HttpStatusCode.Unauthorized)
                return new CommonResult(false, "手机号或密码错误");
            try
            {
                var json = JsonConvert.DeserializeObject<LoginDto>(res.result);
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
                return null;

            try
            {
                var json = JsonConvert.DeserializeObject<DidaBatchCheckDto>(res.result);
                return json;
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
