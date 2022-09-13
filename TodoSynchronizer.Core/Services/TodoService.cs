using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using TodoSynchronizer.Core.Helpers;
using TodoSynchronizer.Core.Service;

namespace TodoSynchronizer.Core.Services
{
    public partial class TodoService
    {
        private static string token;
        public static string Token {
            get
            {
                return token;
            }
            set
            {
                token = value;
                var authProvider = new DelegateAuthenticationProvider(async (request) => {
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                });

                client = new GraphServiceClient("https://graph.microsoft.com/beta", authProvider);
            }
        }

        private static GraphServiceClient client;

        public static User GetUserInfo()
        {
            var info = client.Me.Request().GetAsync().Result;
            return info;
        }

        public static Stream GetUserAvatar()
        {
            var s = client.Me.Photo.Content.Request().GetAsync().Result;
            return s;
        }

        public static List<TodoTaskList> ListLists()
        {
            List<TodoTaskList> res = new List<TodoTaskList>();
            var page = client.Me.Todo.Lists.Request();
            while (page != null)
            {
                var pageres = page.GetAsync().Result;
                foreach (var todolist in pageres)
                    res.Add(todolist);
                page = pageres.NextPageRequest;
            }
            return res;
        }

        public static TodoTaskList GetTaskList(string tasklistid)//ListTasks
        {
            var todoTaskList = client.Me.Todo.Lists[$"{tasklistid}"]
                .Request()
                .GetAsync().Result;
            return todoTaskList;
        }

        public static TodoTaskList AddTaskList(TodoTaskList tasklist)
        {
            var todoTaskList = client.Me.Todo.Lists
                .Request()
                .AddAsync(tasklist).Result;
            return todoTaskList;
        }
        public static List<TodoTask> ListTodoTasks(string tasklistid)
        {
            List<TodoTask> res = new List<TodoTask>();
            var page = client.Me.Todo.Lists[$"{tasklistid}"].Tasks
                .Request();
            while (page != null)
            {
                var pageres = page.GetAsync().Result;
                foreach (var todolist in pageres)
                    res.Add(todolist);
                page = pageres.NextPageRequest;
            }
            return res;
        }

        public static TodoTask AddTask(string tasklistid, TodoTask task)
        {
            var res = client.Me.Todo.Lists[$"{tasklistid}"].Tasks
                .Request()
                .AddAsync(task).Result;
            return res;
        }

        public static TodoTask GetTask(string tasklistid, string taskid)
        {
            var todoTask = client.Me.Todo.Lists[$"{tasklistid}"].Tasks[$"{taskid}"]
                 .Request()
                 .GetAsync().Result;
            return todoTask;
        }

        public static TodoTask UpdateTask(string tasklistid, string taskid, TodoTask task)
        {
            var todoTask = client.Me.Todo.Lists[$"{tasklistid}"].Tasks[$"{taskid}"]
                 .Request()
                 .UpdateAsync(task).Result;
            return todoTask;
        }

        public static void DeleteTask(string tasklistid, string taskid)
        {
            client.Me.Todo.Lists[$"{tasklistid}"].Tasks[$"{taskid}"]
                 .Request()
                 .DeleteAsync().GetAwaiter().GetResult();
        }

        public static List<ChecklistItem> ListCheckItems(string tasklistid, string taskid)
        {
            List<ChecklistItem> res = new List<ChecklistItem>();
            var page = client.Me.Todo.Lists[$"{tasklistid}"].Tasks[$"{taskid}"].ChecklistItems
                .Request();
            while (page != null)
            {
                var pageres = page.GetAsync().Result;
                foreach (var todolist in pageres)
                    res.Add(todolist);
                page = pageres.NextPageRequest;
            }
            return res;
        }

        public static ChecklistItem AddCheckItem(string tasklistid, string taskid, ChecklistItem checklistItem)
        {
            var item = client.Me.Todo.Lists[$"{tasklistid}"].Tasks[$"{taskid}"].ChecklistItems
                    .Request()
                    .AddAsync(checklistItem).Result;
            return item;
        }

        public static ChecklistItem UpdateCheckItem(string tasklistid, string taskid, string checklistitemid, ChecklistItem checklistItem)
        {
            var item = client.Me.Todo.Lists[$"{tasklistid}"].Tasks[$"{taskid}"].ChecklistItems[checklistitemid]
                    .Request()
                    .UpdateAsync(checklistItem).Result;
            return item;
        }

        public static void DeleteCheckItem(string tasklistid, string taskid, string checklistitemid)
        {
            client.Me.Todo.Lists[$"{tasklistid}"].Tasks[$"{taskid}"].ChecklistItems[checklistitemid]
                    .Request()
                    .DeleteAsync().GetAwaiter().GetResult();
        }

        public static List<LinkedResource> ListLinkedResources(string tasklistid, string taskid)
        {
            List<LinkedResource> res = new List<LinkedResource>();
            var page = client.Me.Todo.Lists[$"{tasklistid}"].Tasks[$"{taskid}"].LinkedResources
                .Request();
            while (page != null)
            {
                var pageres = page.GetAsync().Result;
                foreach (var todolist in pageres)
                    res.Add(todolist);
                page = pageres.NextPageRequest;
            }
            return res;
        }

        public static LinkedResource AddLinkedResource(string tasklistid, string taskid, LinkedResource linkedResource)
        {
            var item = client.Me.Todo.Lists[$"{tasklistid}"].Tasks[$"{taskid}"].LinkedResources
                    .Request()
                    .AddAsync(linkedResource).Result;
            return item;
        }

        public static LinkedResource UpdateLinkedResource(string tasklistid, string taskid, string linkedResourceid, LinkedResource linkedResource)
        {
            var item = client.Me.Todo.Lists[$"{tasklistid}"].Tasks[$"{taskid}"].LinkedResources[linkedResourceid]
                    .Request()
                    .UpdateAsync(linkedResource).Result;
            return item;
        }

        public static void DeleteLinkedResource(string tasklistid, string taskid, string linkedResourceid)
        {
            client.Me.Todo.Lists[$"{tasklistid}"].Tasks[$"{taskid}"].LinkedResources[linkedResourceid]
                    .Request()
                    .DeleteAsync().GetAwaiter().GetResult();
        }

        public static List<AttachmentBase> ListAttachments(string tasklistid, string taskid)
        {
            List<AttachmentBase> res = new List<AttachmentBase>();
            var attachmentsRequest = client.Me.Todo.Lists[$"{tasklistid}"].Tasks[$"{taskid}"].Attachments.Request();

            while (attachmentsRequest != null)
            {
                var pageres = attachmentsRequest.GetAsync().Result;
                foreach (var todolist in pageres)
                    res.Add(todolist);
                attachmentsRequest = pageres.NextPageRequest;
            }
            return res;
        }

        public static Stream GetAttachment(string tasklistid, string taskid, string attachmentid)
        {
            var attachmentRequest = client.Me.Todo.Lists[$"{tasklistid}"].Tasks[$"{taskid}"].Attachments[attachmentid].Content.Request().GetAsync();

            return attachmentRequest.Result;
        }

        public static AttachmentSession UploadAttachment(string tasklistid, string taskid, AttachmentInfo attachmentInfo, Stream ms)
        {
            var attachmentsRequestBuilder = client.Me.Todo.Lists[$"{tasklistid}"].Tasks[$"{taskid}"].Attachments;

            var uploadSession = attachmentsRequestBuilder.CreateUploadSession(attachmentInfo).Request().PostAsync().Result;

            //var task = new LargeFileUploadTask<AttachmentSession>(uploadSession, ms, 12*320*1024, todoTaskRequestBuilder.Client);
            //var res = task.UploadAsync().Result;
            var task = new Helpers.LargeFileUploadTask<AttachmentSession>(uploadSession, ms, 12 * 320 * 1024, client);
            var res = task.UploadAsync().Result;

            return res.ItemResponse;
        }

    }
}
