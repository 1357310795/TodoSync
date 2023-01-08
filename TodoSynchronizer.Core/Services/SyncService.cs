using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using TodoSynchronizer.Core.Config;
using TodoSynchronizer.Core.Extensions;
using TodoSynchronizer.Core.Helpers;
using TodoSynchronizer.Core.Models;
using TodoSynchronizer.Core.Models.CanvasModels;

namespace TodoSynchronizer.Core.Services
{
    public class SyncService
    {
        public Dictionary<string, TodoTask> dicUrl = null;
        public Dictionary<string, TodoTaskList> dicCategory = null;
        public Dictionary<Course, TodoTaskList> dicCourse = null;
        public List<TodoTask> canvasTasks = null;
        public List<Course> courses = null;
        public int CourseCount, ItemCount, UpdateCount, FailedCount;

        private string message;
        public string Message
        {
            get { return message; }
            set { 
                message = value;
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Progress, value));
            }
        }

        public delegate void ReportProgressDelegate(SyncState state);

        public event ReportProgressDelegate OnReportProgress;

        public void Go()
        {
            #region 初始化
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Utc };
            CourseCount = 0;
            ItemCount = 0;
            UpdateCount = 0;
            FailedCount = 0;
            #endregion

            #region 读取 Canvas 课程列表
            Message = "读取 Canvas 课程列表";
            try
            {
                courses = CanvasService.ListCourses().Shuffle();
                if (courses == null)
                    throw new Exception("Canvas 课程列表为空");
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
            #endregion

            #region 检查 Todo 列表
            Message = "检查 Todo 列表";
            try
            {
                dicCategory = new Dictionary<string, TodoTaskList>();
                dicCourse = new Dictionary<Course, TodoTaskList>();
                var todoTaskLists = TodoService.ListLists();

                void FindList(string cat, string name)
                {
                    var taskList = todoTaskLists.Find(x => x.DisplayName.CleanEmoji() == name);

                    if (taskList == null)
                        taskList = TodoService.AddTaskList(new TodoTaskList() { DisplayName = name });

                    if (taskList == null)
                        throw new Exception("创建 Todo 列表失败");
                    else
                        Message = $"找到 Todo 列表：{taskList.DisplayName}";
                    dicCategory.Add(cat, taskList);
                }

                if (SyncConfig.Default.NotificationConfig.Enabled)
                    FindList("notification", CanvasStringTemplateHelper.GetNotificationListName());

                if (SyncConfig.Default.ListNameMode == ListNameMode.Category)
                {
                    if (SyncConfig.Default.QuizConfig.Enabled)
                        FindList("quiz", SyncConfig.Default.ListNamesForCategory.QuizListName);
                    if (SyncConfig.Default.DiscussionConfig.Enabled)
                        FindList("discussion", SyncConfig.Default.ListNamesForCategory.DiscussionListName);
                    if (SyncConfig.Default.AssignmentConfig.Enabled)
                        FindList("assignment", SyncConfig.Default.ListNamesForCategory.AssignmentListName);
                    if (SyncConfig.Default.AnouncementConfig.Enabled)
                        FindList("anouncement", SyncConfig.Default.ListNamesForCategory.AnouncementListName);
                }
                else
                {
                    foreach(var c in courses)
                    {
                        var name = CanvasStringTemplateHelper.GetListNameForCourse(c);
                        var taskList = todoTaskLists.Find(x => x.DisplayName == name);

                        if (taskList == null)
                            taskList = TodoService.AddTaskList(new TodoTaskList() { DisplayName = name });

                        if (taskList == null)
                            throw new Exception("创建 Todo 列表失败");
                        else
                            Message = $"找到 Todo 列表：{(SyncConfig.Default.VerboseMode ?  taskList.DisplayName : taskList.Id.GetHashCode())}";
                        dicCourse.Add(c, taskList);
                    }
                }
                
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
            #endregion

            #region 读取 Todo 项目
            Message = "读取 Todo 项目";
            try
            {
                IEnumerable<TodoTask> ListTodoTasksInternal()
                {
                    if (SyncConfig.Default.ListNameMode == ListNameMode.Category)
                    {
                        foreach(var item in dicCategory)
                        {
                            var tmplist = TodoService.ListTodoTasks(item.Value.Id);
                            foreach (var task in tmplist)
                                yield return task;
                        }
                    }
                    else
                    {
                        foreach (var item in dicCourse)
                        {
                            var tmplist = TodoService.ListTodoTasks(item.Value.Id);
                            foreach (var task in tmplist)
                                yield return task;
                        }
                    }
                }
                canvasTasks = ListTodoTasksInternal().ToList();
                if (canvasTasks == null)
                    throw new Exception("Todo 项目为空");
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
            #endregion

            #region 处理 LinkedResources
            Message = "建立字典";
            try
            {
                dicUrl = new Dictionary<string, TodoTask>();
                foreach (var todoTask in canvasTasks)
                {
                    if (todoTask.LinkedResources != null)
                        if (todoTask.LinkedResources.Count > 0)
                        {
                            var url = todoTask.LinkedResources.First().WebUrl;
                            Uri uri;
                            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                                dicUrl.Add(url, todoTask);
                        }
                }
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
            #endregion

            #region 处理全局通知
            if (SyncConfig.Default.NotificationConfig.Enabled)
            {
                ProcessNotifications();
            }
            #endregion

            #region Main
            try
            {
                if (SyncConfig.Default.ListNameMode == ListNameMode.Category)
                {
                    foreach (var course in courses)
                    {
                            CourseCount++;
                        if (SyncConfig.Default.AssignmentConfig.Enabled)
                            ProcessAssignments(GetCourseMessage(course), course, dicCategory["assignment"]);
                        if (SyncConfig.Default.AnouncementConfig.Enabled)
                            ProcessAnouncements(GetCourseMessage(course), course, dicCategory["anouncement"]);
                        if (SyncConfig.Default.QuizConfig.Enabled)
                            ProcessQuizes(GetCourseMessage(course), course, dicCategory["quiz"]);
                        if (SyncConfig.Default.DiscussionConfig.Enabled)
                            ProcessDiscussions(GetCourseMessage(course), course, dicCategory["discussion"]);
                    }
                }
                else//Course
                {
                    foreach (var course in courses)
                    {
                        CourseCount++;
                        if (SyncConfig.Default.AssignmentConfig.Enabled)
                            ProcessAssignments(GetCourseMessage(course), course, dicCourse[course]);
                        if (SyncConfig.Default.AnouncementConfig.Enabled)
                            ProcessAnouncements(GetCourseMessage(course), course, dicCourse[course]);
                        if (SyncConfig.Default.QuizConfig.Enabled)
                            ProcessQuizes(GetCourseMessage(course), course, dicCourse[course]);
                        if (SyncConfig.Default.DiscussionConfig.Enabled)
                            ProcessDiscussions(GetCourseMessage(course), course, dicCourse[course]);
                    }
                }
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
            #endregion

            #region 完成
            OnReportProgress.Invoke(new SyncState(
                SyncStateEnum.Finished,
                $"完成！已处理 {CourseCount} 门课程中的 {ItemCount} 个项目，更新 {UpdateCount} 个项目"
            ));
            #endregion
        }

        #region Assignments
        public void ProcessAssignments(string message_prefix, Course course, TodoTaskList taskList)
        {
            Message = message_prefix + "作业";
            try
            {
                var assignments = CanvasService.ListAssignments(course.Id.ToString());
                if (assignments == null)
                    return;
                if (assignments.Count == 0)
                    return;

                foreach (var assignment in assignments)
                {
                    if (assignment.IsQuizAssignment) continue;
                    if (SyncConfig.Default.IgnoreTooOldItems)
                        if (assignment?.DueAt?.ToUniversalTime() < DateTime.Now.AddDays(-14).ToUniversalTime())
                            continue;
                    var updated = false;
                    ItemCount++;
                    Message = message_prefix + GetItemMessage(assignment);
                    TodoTask todoTask = null;
                    if (dicUrl.ContainsKey(assignment.HtmlUrl))
                        todoTask = dicUrl[assignment.HtmlUrl];
                    else
                        todoTask = null;
                    var isnew = todoTask == null;

                    //---Self & LinkedResource---//
                    TodoTask todoTaskNew = new TodoTask();
                    var res1 = UpdateCanvasItem(course, assignment, todoTask, todoTaskNew, SyncConfig.Default.AssignmentConfig);
                    if (res1)
                    {
                        if (todoTask is null)
                        {
                            todoTask = TodoService.AddTask(taskList.Id.ToString(), todoTaskNew);
                            TodoService.AddLinkedResource(taskList.Id.ToString(), todoTask.Id.ToString(), new LinkedResource() { DisplayName = assignment.HtmlUrl, WebUrl = assignment.HtmlUrl, ApplicationName = "Canvas" });
                            dicUrl.Add(assignment.HtmlUrl, todoTask);
                        }
                        else
                        {
                            todoTask = TodoService.UpdateTask(taskList.Id.ToString(), todoTask.Id.ToString(), todoTaskNew);
                        }
                        updated = true;
                    }

                    //---Submissions -> CheckItems---//
                    if (SyncConfig.Default.AssignmentConfig.CreateScoreAndCommit && isnew
                        || SyncConfig.Default.AssignmentConfig.UpdateScoreAndCommit && !isnew)
                    if (assignment.HasSubmittedSubmissions)
                    {
                        var links = TodoService.ListCheckItems(taskList.Id.ToString(), todoTask.Id.ToString());
                        
                        var submission = CanvasService.GetAssignmentSubmisson(course.Id.ToString(), assignment.Id.ToString());

                        if (submission == null)
                           throw new Exception("获取 submission 失败");

                        ChecklistItem checkitem1 = null;
                        if (links.Count >= 1)
                            checkitem1 = links[0];
                        else
                            checkitem1 = null;

                        ChecklistItem checkitem1New = new ChecklistItem();
                        var res2 = UpdateSubmissionInfo(assignment, submission, checkitem1, checkitem1New, CanvasStringTemplateHelper.GetSubmissionDesc);
                        if (res2)
                        {
                            if (checkitem1 == null)
                            {
                                TodoService.AddCheckItem(taskList.Id.ToString(), todoTask.Id.ToString(), checkitem1New);
                            }
                            else
                            {
                                TodoService.UpdateCheckItem(taskList.Id.ToString(), todoTask.Id.ToString(), checkitem1.Id.ToString(), checkitem1New);
                            }
                            updated = true;
                        }

                        ChecklistItem checkitem2 = null;
                        if (links.Count >= 2)
                            checkitem2 = links[1];
                        else
                            checkitem2 = null;

                        ChecklistItem checkitem2New = new ChecklistItem();
                        var res3 = UpdateSubmissionInfo(assignment, submission, checkitem2, checkitem2New, CanvasStringTemplateHelper.GetGradeDesc);
                        if (res3)
                        {
                            if (checkitem2 == null)
                            {
                                TodoService.AddCheckItem(taskList.Id.ToString(), todoTask.Id.ToString(), checkitem2New);
                            }
                            else
                            {
                                TodoService.UpdateCheckItem(taskList.Id.ToString(), todoTask.Id.ToString(), checkitem2.Id.ToString(), checkitem2New);
                            }
                            updated = true;
                        }

                        //---Comments---//
                        if (SyncConfig.Default.AssignmentConfig.CreateComments && isnew
                            || SyncConfig.Default.AssignmentConfig.UpdateComments && !isnew)
                        {
                                if (submission.SubmissionComments?.Count > 0)
                                {
                                    int i = 2;
                                    foreach (var comment in submission.SubmissionComments)
                                    {
                                        ChecklistItem checkitem3 = null;
                                        if (links.Count >= i + 1)
                                            checkitem3 = links[i];
                                        else
                                            checkitem3 = null;
                                        i++;

                                        ChecklistItem checkitem3New = new ChecklistItem();
                                        var res4 = UpdateSubmissionComment(comment, checkitem3, checkitem3New);
                                        if (res4)
                                        {
                                            if (checkitem3 == null)
                                            {
                                                TodoService.AddCheckItem(taskList.Id.ToString(), todoTask.Id.ToString(), checkitem3New);
                                            }
                                            else
                                            {
                                                TodoService.UpdateCheckItem(taskList.Id.ToString(), todoTask.Id.ToString(), checkitem3.Id.ToString(), checkitem3New);
                                            }
                                            updated = true;
                                        }
                                    }
                                }
                        }
                    }

                    //---Attachments---//
                    if (SyncConfig.Default.AssignmentConfig.CreateAttachments)
                    {
                        var files = new List<Models.CanvasModels.Attachment>();

                        if (assignment.Content != null)
                        {
                            CheckAttachments(assignment.Content, files);
                        }

                        if (files.Count > 0)
                        {
                            updated |= UploadAttachments(taskList, todoTask, files);
                        }
                    }
                        
                    if (updated)
                        UpdateCount++;
                }
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
        }

        private static void CheckAttachments(string content, List<Models.CanvasModels.Attachment> files)
        {
            var file_reg = new Regex(@"<a.+?instructure_file_link.+?title=""(.+?)"".+?href=""(.+?)"".+?</a>");
            var file_matches = file_reg.Matches(content);
            var img_reg = new Regex(@"<img.+?src=""(.+?)"".+?alt=""(.+?)"".+?>");
            var img_matches = img_reg.Matches(content);
            foreach (Match match in file_matches)
            {
                var filename = match.Groups[1].Value;
                var filepath = match.Groups[2].Value;
                files.Add(new Core.Models.CanvasModels.Attachment() { DisplayName = filename, Url = filepath, Locked = false });
            }
            foreach (Match match in img_matches)
            {
                var filename = match.Groups[2].Value;
                var filepath = match.Groups[1].Value;
                files.Add(new Core.Models.CanvasModels.Attachment() { DisplayName = filename, Url = filepath, Locked = false });
            }
        }
        #endregion

        #region Discussions
        private void ProcessDiscussions(string message_prefix, Course course, TodoTaskList taskList)
        {
            Message = message_prefix + "讨论";
            try
            {
                var discussions = CanvasService.ListDiscussions(course.Id.ToString());
                if (discussions == null)
                    return;
                if (discussions.Count == 0)
                    return;

                foreach (var discussion in discussions)
                {
                    if (SyncConfig.Default.IgnoreTooOldItems)
                        if (discussion?.PostedAt?.ToUniversalTime() < DateTime.Now.AddDays(-14).ToUniversalTime())
                            continue;
                    var updated = false;
                    ItemCount++;
                    Message = message_prefix + GetItemMessage(discussion);
                    TodoTask todoTask = null;
                    if (dicUrl.ContainsKey(discussion.HtmlUrl))
                        todoTask = dicUrl[discussion.HtmlUrl];
                    else
                        todoTask = null;

                    //---Self & LinkedResource---//
                    TodoTask todoTaskNew = new TodoTask();
                    var res1 = UpdateCanvasItem(course, discussion, todoTask, todoTaskNew, SyncConfig.Default.DiscussionConfig);
                    if (res1)
                    {
                        if (todoTask is null)
                        {
                            todoTask = TodoService.AddTask(taskList.Id.ToString(), todoTaskNew);
                            TodoService.AddLinkedResource(taskList.Id.ToString(), todoTask.Id.ToString(), new LinkedResource() { DisplayName = discussion.HtmlUrl, WebUrl = discussion.HtmlUrl, ApplicationName = "Canvas" });
                            dicUrl.Add(discussion.HtmlUrl, todoTask);
                        }
                        else
                        {
                            todoTask = TodoService.UpdateTask(taskList.Id.ToString(), todoTask.Id.ToString(), todoTaskNew);
                        }
                        updated = true;
                    }

                    //---Attachments---//
                    if (SyncConfig.Default.DiscussionConfig.CreateAttachments)
                    {
                        var files = discussion.Attachments;
                        
                        if (discussion.Content != null)
                        {
                            CheckAttachments(discussion.Content, files);
                        }

                        if (files.Count > 0)
                        {
                            updated |= UploadAttachments(taskList, todoTask, files);
                        }
                    }
                        
                    if (updated)
                        UpdateCount++;
                }
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
        }
        #endregion

        #region Quizes
        private void ProcessQuizes(string message_prefix, Course course, TodoTaskList taskList)
        {
            Message = message_prefix + "测验";
            try
            {
                var assignments = CanvasService.ListAssignments(course.Id.ToString());
                if (assignments == null)
                    return;
                if (assignments.Count == 0)
                    return;

                foreach (var assignment in assignments)
                {
                    if (!assignment.IsQuizAssignment) continue;
                    if (SyncConfig.Default.IgnoreTooOldItems)
                        if (assignment?.DueAt?.ToUniversalTime() < DateTime.Now.AddDays(-14).ToUniversalTime())
                            continue;
                    var updated = false;
                    ItemCount++;
                    Message = message_prefix + GetItemMessage(assignment);
                    TodoTask todoTask = null;
                    if (dicUrl.ContainsKey(assignment.HtmlUrl))
                        todoTask = dicUrl[assignment.HtmlUrl];
                    else
                        todoTask = null;
                    var isnew = todoTask == null;

                    //---Self & LinkedResource---//
                    TodoTask todoTaskNew = new TodoTask();
                    var res1 = UpdateCanvasItem(course, assignment, todoTask, todoTaskNew, SyncConfig.Default.QuizConfig);
                    if (res1)
                    {
                        if (todoTask is null)
                        {
                            todoTask = TodoService.AddTask(taskList.Id.ToString(), todoTaskNew);
                            TodoService.AddLinkedResource(taskList.Id.ToString(), todoTask.Id.ToString(), new LinkedResource() { DisplayName = assignment.HtmlUrl, WebUrl = assignment.HtmlUrl, ApplicationName = "Canvas" });
                            dicUrl.Add(assignment.HtmlUrl, todoTask);
                        }
                        else
                        {
                            todoTask = TodoService.UpdateTask(taskList.Id.ToString(), todoTask.Id.ToString(), todoTaskNew);
                        }
                        updated = true;
                    }

                    //---Submissions -> CheckItems---//
                    if (SyncConfig.Default.QuizConfig.CreateScoreAndCommit && isnew
                        || SyncConfig.Default.QuizConfig.UpdateScoreAndCommit && !isnew)
                    if (assignment.HasSubmittedSubmissions)
                    {
                        var links = TodoService.ListCheckItems(taskList.Id.ToString(), todoTask.Id.ToString());
                        var quizsubmissions = CanvasService.ListQuizSubmissons(course.Id.ToString(), assignment.QuizId.ToString());

                        if (quizsubmissions != null)
                        {
                            for (int i = 0; i < quizsubmissions.Count; i++)
                            {
                                ChecklistItem checkitem0 = null;
                                if (links.Count >= i + 1)
                                    checkitem0 = links[i];
                                else
                                    checkitem0 = null;

                                ChecklistItem checkitem0New = new ChecklistItem();
                                var res4 = UpdateSubmissionInfo(assignment, quizsubmissions[i], checkitem0, checkitem0New, CanvasStringTemplateHelper.GetSubmissionDesc);
                                if (res4)
                                {
                                    if (checkitem0 == null)
                                    {
                                        TodoService.AddCheckItem(taskList.Id.ToString(), todoTask.Id.ToString(), checkitem0New);
                                    }
                                    else
                                    {
                                        TodoService.UpdateCheckItem(taskList.Id.ToString(), todoTask.Id.ToString(), checkitem0.Id.ToString(), checkitem0New);
                                    }
                                    updated = true;
                                }
                            }
                        }
                        
                    }
                    //---Attachments---//
                    if (SyncConfig.Default.QuizConfig.CreateAttachments)
                    {
                        var files = new List<Models.CanvasModels.Attachment>();

                        if (assignment.Content != null)
                        {
                            CheckAttachments(assignment.Content, files);
                        }
                        
                        if (files.Count > 0)
                        {
                            updated |= UploadAttachments(taskList, todoTask, files);
                        }
                    }
                    
                    if (updated)
                        UpdateCount++;
                }
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
        }
        #endregion

        #region Anouncements
        private void ProcessAnouncements(string message_prefix, Course course, TodoTaskList taskList)
        {
            Message = message_prefix + "公告";
            try
            {
                var anouncements = CanvasService.ListAnouncements(course.Id.ToString());
                if (anouncements == null)
                    return;
                if (anouncements.Count == 0)
                    return;

                foreach (var anouncement in anouncements)
                {
                    if (SyncConfig.Default.IgnoreTooOldItems)
                        if (anouncement?.PostedAt?.ToUniversalTime() < DateTime.Now.AddDays(-14).ToUniversalTime())
                            continue;
                    var updated = false;
                    ItemCount++;
                    Message = message_prefix + GetItemMessage(anouncement);
                    TodoTask todoTask = null;
                    if (dicUrl.ContainsKey(anouncement.HtmlUrl))
                        todoTask = dicUrl[anouncement.HtmlUrl];
                    else
                        todoTask = null;

                    //---Self & LinkedResource---//
                    TodoTask todoTaskNew = new TodoTask();
                    var res1 = UpdateCanvasItem(course, anouncement, todoTask, todoTaskNew, SyncConfig.Default.AnouncementConfig);
                    if (res1)
                    {
                        if (todoTask is null)
                        {
                            todoTask = TodoService.AddTask(taskList.Id.ToString(), todoTaskNew);
                            TodoService.AddLinkedResource(taskList.Id.ToString(), todoTask.Id.ToString(), new LinkedResource() { DisplayName = anouncement.HtmlUrl, WebUrl = anouncement.HtmlUrl, ApplicationName = "Canvas" });
                            dicUrl.Add(anouncement.HtmlUrl, todoTask);
                        }
                        else
                        {
                            todoTask = TodoService.UpdateTask(taskList.Id.ToString(), todoTask.Id.ToString(), todoTaskNew);
                        }
                        updated = true;
                    }

                    //---Attachments---//
                    if (SyncConfig.Default.AnouncementConfig.CreateAttachments)
                    {
                        var files = anouncement.Attachments;
                        var file_reg = new Regex(@"<a.+?instructure_file_link.+?title=""(.+?)"".+?href=""(.+?)"".+?</a>");

                        if (anouncement.Content != null)
                        {
                            CheckAttachments(anouncement.Content, files);
                        }
                        
                        if (files.Count > 0)
                        {
                            updated |= UploadAttachments(taskList, todoTask, files);
                        }
                    }
                        
                    if (updated)
                        UpdateCount++;
                }
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
        }
        #endregion

        #region Notification
        public void ProcessNotifications()
        {
            try
            {
                Message = "处理全局通知";

                var notifications = CanvasService.ListNotifications();
                if (notifications == null)
                    return;
                if (notifications.Count == 0)
                    return;

                
                var notilist = dicCategory["notification"];
                var tmplist = TodoService.ListTodoTasks(notilist.Id);
                var dic = new Dictionary<string, TodoTask>();

                foreach (var todoTask in tmplist)
                {
                    if (todoTask.LinkedResources != null)
                        if (todoTask.LinkedResources.Count > 0)
                        {
                            var url = todoTask.LinkedResources.First().DisplayName;
                            dic.Add(url, todoTask);
                        }
                }

                foreach (var notification in notifications)
                {
                    var updated = false;
                    ItemCount++;
                    Message = "处理全局通知 " + notification.Subject;
                    TodoTask todoTask = null;
                    todoTask = tmplist.FirstOrDefault(x => x.Title == notification.Subject);

                    if (dic.ContainsKey(notification.Id.ToString()))
                        todoTask = dic[notification.Id.ToString()];
                    else
                        todoTask = null;

                    //---Self---//
                    TodoTask todoTaskNew = new TodoTask();
                    var res1 = UpdateCanvasItem(null, notification, todoTask, todoTaskNew, SyncConfig.Default.NotificationConfig);

                    if (res1)
                    {
                        if (todoTask is null)
                        {
                            todoTask = TodoService.AddTask(notilist.Id.ToString(), todoTaskNew);
                            TodoService.AddLinkedResource(notilist.Id.ToString(), todoTask.Id.ToString(), new LinkedResource() { ApplicationName = notification.Icon.ToString(), DisplayName = notification.Id.ToString() });
                            dic.Add(notification.Id.ToString(), todoTask);
                        }
                        else
                        {
                            todoTask = TodoService.UpdateTask(notilist.Id.ToString(), todoTask.Id.ToString(), todoTaskNew);
                        }
                        updated = true;
                    }

                    if (updated)
                        UpdateCount++;
                }
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
        }
        #endregion

        #region Common
        private bool UpdateSubmissionInfo<T>(Assignment assignment, T submission, ChecklistItem checklistitemOld, ChecklistItem checklistitemNew, Func<Assignment, T, string> func)
        {
            var modified = false;
            var desc = func(assignment, submission);
            var check = !desc.Contains("未") && !desc.Contains("正在");

            checklistitemNew.IsChecked = checklistitemOld?.IsChecked ?? false;
            if (checklistitemNew.IsChecked != check)
            {
                checklistitemNew.IsChecked = check;
                modified = true;
            }
            if (checklistitemOld == null || checklistitemOld.DisplayName != desc)
            {
                checklistitemNew.DisplayName = desc;
                modified = true;
            }
            return modified;
        }

        private bool UpdateSubmissionComment(SubmissionComment comment, ChecklistItem checklistitemOld, ChecklistItem checklistitemNew)
        {
            var modified = false;
            var desc = CanvasStringTemplateHelper.GetSubmissionComment(comment);
            var check = true;

            checklistitemNew.IsChecked = checklistitemOld?.IsChecked ?? false;
            if (checklistitemNew.IsChecked != check)
            {
                checklistitemNew.IsChecked = check;
                modified = true;
            }
            if (checklistitemOld == null || checklistitemOld.DisplayName != desc)
            {
                checklistitemNew.DisplayName = desc;
                modified = true;
            }
            return modified;
        }

        public bool UpdateCanvasItem(Course course, ICanvasItem item, TodoTask todoTaskOld, TodoTask todoTaskNew, ICanvasItemConfig config)
        {
            var modified = false;

            if (todoTaskOld == null || todoTaskOld != null && config.UpdateTitle)
            {
                var title = CanvasStringTemplateHelper.GetTitle(course, item);
                if (todoTaskOld == null || todoTaskOld.Title == null || title.Trim() != todoTaskOld.Title.Trim())
                {
                    todoTaskNew.Title = title;
                    modified = true;
                }
            }

            if (todoTaskOld == null && config.CreateContent || todoTaskOld != null && config.UpdateContent)
            {
                var content = CanvasStringTemplateHelper.GetContent(item);
                if (todoTaskOld == null || todoTaskOld.Body.Content == null || content.Trim() != todoTaskOld.Body.Content.Trim())
                {
                    todoTaskNew.Body = new ItemBody() { ContentType = BodyType.Text };
                    todoTaskNew.Body.Content = content;
                    modified = true;
                }
            }

            if (todoTaskOld == null && config.CreateDueDate || todoTaskOld != null && config.UpdateDueDate)
            {
                var duetime = CanvasPreference.GetDueTime(item);
                if (duetime.HasValue)
                {
                    var date = duetime.Value.ToUniversalTime().Date.ToString("yyyy-MM-ddTHH:mm:ss.fffffff", System.Globalization.CultureInfo.InvariantCulture);
                    if (todoTaskOld == null || todoTaskOld.DueDateTime == null || date != todoTaskOld.DueDateTime.DateTime)
                    {
                        todoTaskNew.DueDateTime = DateTimeTimeZone.FromDateTime(duetime.Value);
                        modified = true;
                    }
                }
                else if (todoTaskOld != null && todoTaskOld.DueDateTime != null)
                {
                    todoTaskNew.AdditionalData = new Dictionary<string, object>();
                    todoTaskNew.AdditionalData["dueDateTime"] = null;
                    modified = true;
                }
            }
                
            if (todoTaskOld == null && config.CreateRemind || todoTaskOld != null && config.UpdateRemind)
            {
                var remindtime = CanvasPreference.GetRemindTime(item);
                if (remindtime.HasValue)
                {
                    var date = remindtime.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffff", System.Globalization.CultureInfo.InvariantCulture);
                    if (todoTaskOld == null || todoTaskOld.IsReminderOn == false || todoTaskOld.ReminderDateTime == null || date != todoTaskOld.ReminderDateTime.DateTime)
                    {
                        todoTaskNew.ReminderDateTime = DateTimeTimeZone.FromDateTime(remindtime.Value);
                        todoTaskNew.IsReminderOn = true;
                        modified = true;
                    }
                }
            }

            if (todoTaskOld == null && config.CreateImportance || todoTaskOld != null && config.UpdateImportance)
            {
                var importance = config.SetImportance;
                if (todoTaskOld == null || todoTaskOld.Importance.Value != (importance ? Importance.High : Importance.Normal))
                {
                    todoTaskNew.Importance = (importance ? Importance.High : Importance.Normal);
                    modified = true;
                }
            }

            return modified;
        }

        private bool UploadAttachments(TodoTaskList taskList, TodoTask todoTask, List<Models.CanvasModels.Attachment> files)
        {
            var updated = false;
            try
            {
                var attachments = TodoService.ListAttachments(taskList.Id.ToString(), todoTask.Id.ToString());
                foreach (var file in files)
                {
                    var exist = attachments.Any(x => x.Name == file.DisplayName);
                    if (!exist)
                    {
                        file.Url = file.Url.UrlUnescape().EscToHtml();
                        Uri fulluri;
                        var isabsolute = Uri.TryCreate(file.Url, UriKind.Absolute, out fulluri);
                        if (!isabsolute)
                        {
                            var urires = Uri.TryCreate(new Uri("https://oc.sjtu.edu.cn"), file.Url, out fulluri);
                            if (!urires)
                                throw new Exception($"Uri无效：{file.Url}");
                        }

                        HttpClient client = CanvasService.Client;
                        HttpResponseMessage res = null;
                        try
                        {
                            var datatask = client.GetAsync(fulluri);
                            //datatask.RunSynchronously();
                            datatask.Wait();
                            res = datatask.GetAwaiter().GetResult();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"获取文件时发生错误\n{fulluri.AbsoluteUri}\n{ex.Message}");
                        }

                        if (res.StatusCode != HttpStatusCode.OK)
                            throw new Exception($"获取文件时发生错误\n{fulluri.AbsoluteUri}\n[{(int)res.StatusCode} {res.StatusCode.ToString()}] {res.Content.ReadAsStringAsync().Result}");
                        var data = res.Content.ReadAsByteArrayAsync().Result;

                        if (data.Length > 25 * 1024 * 1024) continue;
                        Stream stream = new MemoryStream(data);

                        //HttpWebRequest req = (HttpWebRequest)WebRequest.Create(fulluri);
                        //req.Method = "GET";
                        //HttpWebResponse resp = null;
                        //resp = (HttpWebResponse)req.GetResponse();
                        //Stream stream = null;
                        //stream = resp.GetResponseStream();
                        //MemoryStream ms = new MemoryStream();
                        //stream.CopyTo(ms);
                        //var data = StreamToBytes(ms);

                        AttachmentInfo info = new AttachmentInfo();
                        info.AttachmentType = AttachmentType.File;
                        info.Size = data.Length;
                        info.Name = file.DisplayName;

                        TodoService.UploadAttachment(taskList.Id.ToString(), todoTask.Id.ToString(), info, stream);
                        updated = true;
                    }
                }
                return updated;
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Progress, $"上传文件失败：{ex.Message}"));
                return false;
            }
        }

        private string GetCourseMessage(Course course)
        {
            return $"处理课程 {(SyncConfig.Default.VerboseMode ? course.Name : CourseCount)} ";
        }

        private string GetItemMessage(ICanvasItem item)
        {
            return $"{item.GetItemName()} {(SyncConfig.Default.VerboseMode ? item.Title : ItemCount)} ";
        }

        public static byte[] StreamToBytes(Stream stream)
        {

            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }
        #endregion
    }
}
