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
        string configpath = "", graphtokenkey = "";
        for(int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-canvastoken")
                if (i + 1 < args.Length)
                    canvastoken = args[i + 1].Trim();
            if (args[i] == "-graphtokenfile")
                if (i + 1 < args.Length)
                    graphtokenpath = args[i + 1].Trim();
            if (args[i] == "-configfile")
                if (i + 1 < args.Length)
                    configpath = args[i + 1].Trim();
            if (args[i] == "-graphtokenkey")
                if (i + 1 < args.Length)
                    graphtokenkey = args[i + 1].Trim();
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
        if (graphtokenkey == "")
        {
            Console.WriteLine("未指定 Graph Token 秘钥！");
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
            var graphtokenenc = File.ReadAllText(graphtokenpath);
            var graphtoken = AesHelper.Decrypt(graphtokenkey, graphtokenenc);

            //var headers = new Dictionary<string, string>();
            //headers.Add("Content-Type", "application/x-www-form-urlencoded");
            var forms = new List<KeyValuePair<string, string>>();
            forms.Add(new KeyValuePair<string, string>("client_id", "49694ef2-8751-4ac9-8431-8817c27350b4"));
            forms.Add(new KeyValuePair<string, string>("scope", "Tasks.ReadWrite User.Read offline_access"));
            forms.Add(new KeyValuePair<string, string>("refresh_token", graphtoken));
            forms.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));

            FormUrlEncodedContent form = new FormUrlEncodedContent(forms);

            HttpClient client = new HttpClient();
            var posttask = client.PostAsync("https://login.microsoftonline.com/common/oauth2/v2.0/token", form);
            posttask.Wait();
            var refreshres = posttask.GetAwaiter().GetResult();

            if (!refreshres.IsSuccessStatusCode)
            {
                Console.WriteLine("获取 Graph Token 失败！");
                Console.WriteLine(refreshres.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                Environment.Exit(-1);
            }
            RefreshModel refreshModel = JsonConvert.DeserializeObject<RefreshModel>(refreshres.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            TodoService.Token = refreshModel.AccessToken;

            graphtokenenc = AesHelper.Encrypt(graphtokenkey, refreshModel.RefreshToken);
            File.WriteAllText(graphtokenpath, graphtokenenc);
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
