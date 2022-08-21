using TodoSynchronizer.Core.Service;
using TodoSynchronizer.Core.Services;
using Newtonsoft.Json;
using TodoSynchronizer.Core.Config;
using YamlDotNet.Serialization;
using TodoSynchronizer.Core.Yaml;
using File = System.IO.File;

namespace TodoSynchronizer.CLI;
class Program
{
    static bool error;
    static void Main(string[] args)
    {
        Console.WriteLine("TodoSynchronizer v0.1 beta");

        string canvastoken = "", graphtokenpath = "";
        string configpath = "";
        for(int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-canvastoken")
                if (i + 1 < args.Length)
                    canvastoken = args[i + 1];
            if (args[i] == "-graphtokenfile")
                if (i + 1 < args.Length)
                    graphtokenpath = args[i + 1];
            if (args[i] == "-configfile")
                if (i + 1 < args.Length)
                    configpath = args[i + 1];
        }

        if (canvastoken == "")
        {
            Console.WriteLine("未指定 Canvas Token！");
            Environment.Exit(-1);
        }
        if (graphtokenpath == "")
        {
            Console.WriteLine("未指定 Graph Token 文件！");
            Environment.Exit(-1);
        }
        if (configpath == "")
        {
            Console.WriteLine("未指定配置文件！");
            Environment.Exit(-1);
        }
        var res1 = CanvasService.Login(canvastoken);
        if (!res1.success)
        {
            Console.WriteLine("Canvas 认证失败！");
            Console.WriteLine(res1.result);
            Environment.Exit(-1);
        }
        Console.WriteLine($"Canvas 认证成功");

        try
        {
            var graphtoken = File.ReadAllText(graphtokenpath);

            var headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/x-www-form-urlencoded");
            var forms = new Dictionary<string, string>();
            forms.Add("client_id", "49694ef2-8751-4ac9-8431-8817c27350b4");
            forms.Add("scope", "Tasks.ReadWrite%20User.Read%20offline_access");
            forms.Add("refresh_token", graphtoken);
            forms.Add("grant_type", "refresh_token");

            var refreshres = Web.Post("https://login.microsoftonline.com/common/oauth2/v2.0/token", headers, forms, false);
            if (!refreshres.success || refreshres.code != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("获取 Graph Token 失败！");
                Console.WriteLine(refreshres.result ?? refreshres.message);
                Environment.Exit(-1);
            }
            RefreshModel refreshModel = JsonConvert.DeserializeObject<RefreshModel>(refreshres.result);
            TodoService.Token = refreshModel.AccessToken;
            File.WriteAllText(graphtokenpath, refreshModel.RefreshToken);
            var userinfo = TodoService.GetUserInfo();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Graph 认证失败！");
            Console.WriteLine(ex.ToString());
            Environment.Exit(-1);
        }
        Console.WriteLine("Graph 认证成功");

        try
        {
            var yml = File.ReadAllText(configpath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
                .WithTypeInspector(n => new IgnoreCaseTypeInspector(n))
                .IgnoreUnmatchedProperties()
                .Build();
            SyncConfig.Default = deserializer.Deserialize<SyncConfig>(yml);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"读取配置失败：{ex.Message}");
            Environment.Exit(-1);
        }
        Console.WriteLine("读取配置成功");

        SyncService sync = new SyncService();
        sync.OnReportProgress += OnReportProgress;
        sync.Go();
    }

    private static void OnReportProgress(SyncState state)
    {
        Console.WriteLine(state.Message);
        if (state.State == SyncStateEnum.Finished)
        {
            Environment.Exit(error ? -1 : 0);
            //Finish();
        }
        if (state.State == SyncStateEnum.Error && !SyncConfig.Default.IgnoreErrors)
        {
            error = true;
        }
    }

    public partial class RefreshModel
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
}
