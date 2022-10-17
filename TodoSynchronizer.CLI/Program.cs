using TodoSynchronizer.Core.Service;
using TodoSynchronizer.Core.Services;
using Newtonsoft.Json;
using TodoSynchronizer.Core.Config;
using YamlDotNet.Serialization;
using TodoSynchronizer.Core.Yaml;
using File = System.IO.File;
using TodoSynchronizer.Core.Models;

namespace TodoSynchronizer.CLI;
class Program
{
    static bool error;
    static ISimpleLogger logger;
    static void Main(string[] args)
    {
        string canvastoken = "", graphtokenpath = "";
        string configpath = "", graphtokenkey = "", offlinetokenfile = "";
        bool local = false;
        OfflineTokenDto offlineToken = null;

        for (int i = 0; i < args.Length; i++)
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
            if (args[i] == "-local")
                local = true;
        }

        if (!local)
        {
            logger = new ConsoleAdapter();
            Log("TodoSynchronizer v0.1 beta");
            Log(DateTime.Now.ToString("G"));

            if (canvastoken == "")
            {
                Log("未指定 Canvas Token！");
                Environment.Exit(-1);
            }
            if (graphtokenpath == "")
            {
                Log("未指定 Graph Token 文件！");
                Environment.Exit(-1);
            }
            if (graphtokenkey == "")
            {
                Log("未指定 Graph Token 秘钥！");
                Environment.Exit(-1);
            }
        }
        else
        {
            var logfilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{DateTime.Now.ToString("yyyyMMdd")}.log");
            logger = new LogFileAdapter(logfilepath);
            Log("TodoSynchronizer v0.1 beta");
            Log(DateTime.Now.ToString("G"));

            configpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"config.yaml");
            offlinetokenfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"token.json");
            offlineToken = JsonConvert.DeserializeObject<OfflineTokenDto>(File.ReadAllText(offlinetokenfile));
            canvastoken = offlineToken.CanvasToken;
        }
        
        if (configpath == "")
        {
            Log("未指定配置文件！");
            Environment.Exit(-1);
        }
        
        var res1 = CanvasService.Login(canvastoken);
        if (!res1.success)
        {
            Log("Canvas 认证失败！");
            Log(res1.result);
            Environment.Exit(-1);
        }
        Log($"Canvas 认证成功");

        try
        {
            string graphtokenenc = "", graphtoken = "";
            if (offlineToken != null)
            {
                graphtoken = offlineToken.GraphToken;
            }
            else
            {
                graphtokenenc = File.ReadAllText(graphtokenpath);
                graphtoken = AesHelper.Decrypt(graphtokenkey, graphtokenenc);
            }
            
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
                Log("获取 Graph Token 失败！");
                Log(refreshres.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                Environment.Exit(-1);
            }
            RefreshModel refreshModel = JsonConvert.DeserializeObject<RefreshModel>(refreshres.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            TodoService.Token = refreshModel.AccessToken;

            if (offlineToken != null)
            {
                offlineToken.GraphToken = refreshModel.RefreshToken;
            }
            else
            {
                graphtokenenc = AesHelper.Encrypt(graphtokenkey, refreshModel.RefreshToken);
                File.WriteAllText(graphtokenpath, graphtokenenc);
            }
            
            var userinfo = TodoService.GetUserInfo();
        }
        catch (Exception ex)
        {
            Log("Graph 认证失败！");
            Log(ex.ToString());
            Environment.Exit(-1);
        }
        Log("Graph 认证成功");

        try
        {
            var offlineTokenDto = JsonConvert.SerializeObject(offlineToken);
            File.WriteAllText(offlinetokenfile, offlineTokenDto);
        }
        catch(Exception ex)
        {
            Log("更新 Graph Token失败！请检查是否有文件的写入权限！");
            Log(ex.ToString());
            Environment.Exit(-1);
        }

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
            Log($"读取配置失败：{ex.Message}");
            Environment.Exit(-1);
        }
        Log("读取配置成功");

        SyncService sync = new SyncService();
        sync.OnReportProgress += OnReportProgress;
        sync.Go();
    }

    private static void OnReportProgress(SyncState state)
    {
        Log(state.Message);
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

    private static void Log(string message)
    {
        logger.Log(message);
    }
}
