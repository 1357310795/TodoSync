using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using TodoSynchronizer.Core.Models.DidaModels;

namespace TodoSynchronizer.Core.Services
{
    public sealed class DidaService : IDisposable
    {
        private const int CompletedPageSize = 200;
        private const int RateLimitRetries = 4;
        private const string DateFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'+0000'";
        private readonly HttpClient client;
        private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            DateFormatString = DateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Ignore
        };

        public DidaService(string apiToken) : this(apiToken, new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false })
        {
        }

        internal DidaService(string apiToken, HttpMessageHandler handler)
        {
            if (string.IsNullOrWhiteSpace(apiToken) || apiToken.Trim().Any(char.IsWhiteSpace))
                throw new ArgumentException("请配置滴答清单 API 口令 DIDA_API");

            client = new HttpClient(handler) { BaseAddress = new Uri("https://api.dida365.com/open/v1/") };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken.Trim());
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        internal byte[] GetSourceIndexKey()
        {
            var material = Encoding.UTF8.GetBytes("TodoSync Dida source index v1\0" + client.DefaultRequestHeaders.Authorization.Parameter);
            return SHA512.HashData(material);
        }

        public List<DidaTaskList> ListProjects()
        {
            return Request<List<DidaTaskList>>(HttpMethod.Get, "project", "读取清单");
        }

        public DidaTaskList AddTaskList(string name)
        {
            var project = Request<DidaTaskList>(HttpMethod.Post, "project", "创建清单", new { name, kind = "TASK" });
            if (string.IsNullOrWhiteSpace(project.Id))
                throw new InvalidDataException("滴答 Open API 未返回新清单 ID");

            return project;
        }

        public List<DidaTask> GetTasks(string projectId)
        {
            var data = Request<DidaProjectData>(HttpMethod.Get, ProjectPath(projectId) + "/data", "读取未完成任务");
            return data.Tasks ?? throw new InvalidDataException("滴答 Open API 未返回任务列表");
        }

        public IEnumerable<DidaTask> GetCompleted(string projectId)
        {
            return GetCompleted(projectId, DateTime.UnixEpoch, DateTime.UtcNow);
        }

        private IEnumerable<DidaTask> GetCompleted(string projectId, DateTime start, DateTime end)
        {
            var tasks = Request<List<DidaTask>>(HttpMethod.Post, "task/completed", "读取已完成任务", new
            {
                projectIds = new[] { projectId },
                startDate = start,
                endDate = end
            });
            if (tasks.Count < CompletedPageSize)
                return tasks;

            // 完成时间筛选包含两端。重叠的边界任务由调用方按任务 ID 去重。
            var middle = start.AddMilliseconds(Math.Floor((end - start).TotalMilliseconds / 2));
            if (middle <= start || middle >= end)
                throw new InvalidDataException("滴答 Open API 已完成任务超过同一时间范围的查询上限");

            return GetCompleted(projectId, start, middle).Concat(GetCompleted(projectId, middle, end));
        }

        public List<DidaTaskComment> ListTaskComments(string projectId, string taskId)
        {
            var path = ProjectPath(projectId) + "/task/" + Uri.EscapeDataString(taskId) + "/comments";
            return Request<List<DidaTaskComment>>(HttpMethod.Get, path, "读取任务评论");
        }

        public void AddTaskComment(string projectId, string taskId, string title)
        {
            var path = ProjectPath(projectId) + "/task/" + Uri.EscapeDataString(taskId) + "/comment";
            Request<JToken>(HttpMethod.Post, path, "添加任务评论", new { title }, readResponse: false);
        }

        public void SaveTask(DidaTask task)
        {
            var isNew = task.Id == null;
            var path = isNew ? "task" : "task/" + Uri.EscapeDataString(task.Id);
            var serializer = JsonSerializer.Create(jsonSettings);
            var body = JObject.FromObject(task, serializer);
            // 更新时显式传空日期，避免 Canvas 移除截止日期后继续保留旧日期。
            if (!isNew && task.DueDate == null && task.StartDate == null)
            {
                body["startDate"] = JValue.CreateNull();
                body["dueDate"] = JValue.CreateNull();
            }

            var saved = Request<DidaTask>(HttpMethod.Post, path, isNew ? "创建任务" : "更新任务", body);
            if (string.IsNullOrWhiteSpace(saved.Id) || (!isNew && saved.Id != task.Id))
                throw new InvalidDataException("滴答 Open API 返回的任务 ID 无效");

            task.Id = saved.Id;
        }

        private static string ProjectPath(string projectId)
        {
            return "project/" + Uri.EscapeDataString(projectId);
        }

        private T Request<T>(HttpMethod method, string path, string operation, object body = null, bool readResponse = true)
            where T : class
        {
            var payload = body == null ? null : JsonConvert.SerializeObject(body, jsonSettings);
            for (var attempt = 0; ; attempt++)
            {
                using var request = new HttpRequestMessage(method, path);
                if (payload != null)
                    request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

                using var response = client.SendAsync(request).GetAwaiter().GetResult();
                if (response.StatusCode == HttpStatusCode.TooManyRequests && attempt < RateLimitRetries)
                {
                    var retryAfter = response.Headers.RetryAfter;
                    var delay = retryAfter?.Delta
                        ?? (retryAfter?.Date - DateTimeOffset.UtcNow)
                        ?? TimeSpan.FromMinutes(1);
                    if (delay <= TimeSpan.FromMinutes(2))
                    {
                        response.Dispose();
                        if (delay > TimeSpan.Zero)
                            Thread.Sleep(delay);
                        continue;
                    }
                }

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"滴答 Open API {operation}失败，HTTP {(int)response.StatusCode}", null, response.StatusCode);

                if (!readResponse)
                    return null;

                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                try
                {
                    return JsonConvert.DeserializeObject<T>(json, jsonSettings)
                        ?? throw new InvalidDataException($"滴答 Open API {operation}未返回数据");
                }
                catch (JsonException)
                {
                    throw new InvalidDataException($"无法解析滴答 Open API {operation}响应");
                }
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
