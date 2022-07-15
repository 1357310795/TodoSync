using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoSynchronizer.Helpers;
using TodoSynchronizer.Models;
using TodoSynchronizer.Models.CanvasModels;
using TodoSynchronizer.Service;

namespace TodoSynchronizer.Services
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

        public static CommonResult TryCacheLogin()
        {
            var token = IniHelper.GetKeyValue("canvas", "token", "");
            if (token == "")
                return new CommonResult(false, "无缓存");
            return Login(token);
        }
    }
}
