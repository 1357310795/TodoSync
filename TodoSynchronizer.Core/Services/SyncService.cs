using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TodoSynchronizer.Core.Config;
using TodoSynchronizer.Core.Helpers;
using TodoSynchronizer.Core.Models.CanvasModels;
using Newtonsoft.Json;

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
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Local };
            CourseCount = 0;
            ItemCount = 0;
            UpdateCount = 0;
            FailedCount = 0;

            #region 读取 Canvas 课程列表
            Message = "读取 Canvas 课程列表";
            try
            {
                courses = CanvasService.ListCourses();
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
                if (SyncConfig.Default.ListNameMode == ListNameMode.Category)
                {
                    dicCategory = new Dictionary<string, TodoTaskList>();
                    var todoTaskLists = TodoService.ListLists();

                    void FindList(string cat, string name)
                    {
                        var taskList = todoTaskLists.Find(x => x.DisplayName == name);

                        if (taskList == null)
                            taskList = TodoService.AddTaskList(new TodoTaskList() { DisplayName = name });

                        if (taskList == null)
                            throw new Exception("创建 Todo 列表失败");
                        else
                            Message = $"找到 Todo 列表：{taskList.DisplayName}";
                        dicCategory.Add(cat, taskList);
                    }
                    FindList("quiz", SyncConfig.Default.ListNamesForCategory.QuizListName);
                    FindList("discussion", SyncConfig.Default.ListNamesForCategory.DiscussionListName);
                    FindList("assignment", SyncConfig.Default.ListNamesForCategory.AssignmentListName);
                    FindList("anouncement", SyncConfig.Default.ListNamesForCategory.AnouncementListName);
                }
                else
                {
                    dicCourse = new Dictionary<Course, TodoTaskList>();
                    var todoTaskLists = TodoService.ListLists();

                    foreach(var c in courses)
                    {
                        var name = CanvasStringTemplateHelper.GetListNameForCourse(c);
                        var taskList = todoTaskLists.Find(x => x.DisplayName == name);

                        if (taskList == null)
                            taskList = TodoService.AddTaskList(new TodoTaskList() { DisplayName = name });

                        if (taskList == null)
                            throw new Exception("创建 Todo 列表失败");
                        else
                            Message = $"找到 Todo 列表：{taskList.DisplayName}";
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

            #region Main
            try
            {
                if (SyncConfig.Default.ListNameMode == ListNameMode.Category)
                {
                    foreach (var course in courses)
                    {
                        CourseCount++;
                        ProcessAssignments($"处理课程 {course.Name} ", course, dicCategory["assignment"]);
                        //ProcessAnouncements($"处理课程 {course.Name} ", course, dicCategory["anouncement"]);
                        //ProcessQuizes($"处理课程 {course.Name} ", course, dicCategory["quiz"]);
                        //ProcessDiscussions($"处理课程 {course.Name} ", course, dicCategory["discussion"]);
                    }
                }
                else//Course
                {
                    foreach (var course in courses)
                    {
                        CourseCount++;
                        ProcessAssignments($"处理课程 {course.Name} ", course, dicCourse[course]);
                        //ProcessAnouncements($"处理课程 {course.Name} ", course, dicCourse[course]);
                        //ProcessQuizes($"处理课程 {course.Name} ", course, dicCourse[course]);
                        //ProcessDiscussions($"处理课程 {course.Name} ", course, dicCourse[course]);
                    }
                }
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
            #endregion
            
            OnReportProgress.Invoke(new SyncState(
                SyncStateEnum.Finished,
                $"完成！已处理 {CourseCount} 门课程中的 {ItemCount} 个项目，更新 {UpdateCount} 个项目"
            ));
        }

        #region Assignments
        public void ProcessAssignments(string message_prefix, Course course, TodoTaskList taskList)
        {
            Message = message_prefix;
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
                    var updated = false;
                    ItemCount++;
                    Message = message_prefix + $"作业 {assignment.Name}";
                    TodoTask todoTask = null;
                    if (dicUrl.ContainsKey(assignment.HtmlUrl))
                        todoTask = dicUrl[assignment.HtmlUrl];
                    else
                        todoTask = null;

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
                    if (assignment.HasSubmittedSubmissions)
                    {
                        var links = TodoService.ListCheckItems(taskList.Id.ToString(), todoTask.Id.ToString());
                        
                        var submission = CanvasService.GetAssignmentSubmisson(course.Id.ToString(), assignment.Id.ToString());

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
                    }
                    //---Attachments---//
                    var file_reg = new Regex(@"<a.+?instructure_file_link.+?title=""(.+?)"".+?href=""(.+?)"".+?</a>");
                    var file_matches = file_reg.Matches(assignment.Content);
                    if (file_matches.Count > 0)
                    {
                        var attachments = TodoService.ListAttachments(taskList.Id.ToString(), todoTask.Id.ToString());
                        foreach (Match match in file_matches)
                        {
                            var filename = match.Groups[1].Value;
                            var filepath = match.Groups[2].Value;
                            var exist = attachments.Any(x => x.Name == filename);
                            if (!exist)
                            {
                                WebClient client = new WebClient();
                                var data = client.DownloadData(filepath);
                                if (data.Length > 25 * 1024 * 1024) continue;
                                Stream stream = new MemoryStream(data);

                                AttachmentInfo info = new AttachmentInfo();
                                info.AttachmentType = AttachmentType.File;
                                info.Size = data.Length;
                                info.Name = filename;

                                TodoService.UploadAttachment(taskList.Id.ToString(), todoTask.Id.ToString(), info, stream);
                                updated = true;
                            }
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

        #region Discussions
        private void ProcessDiscussions(string message_prefix, Course course, TodoTaskList taskList)
        {
            Message = message_prefix;
            try
            {
                var discussions = CanvasService.ListDiscussions(course.Id.ToString());
                if (discussions == null)
                    return;
                if (discussions.Count == 0)
                    return;

                foreach (var discussion in discussions)
                {
                    var updated = false;
                    ItemCount++;
                    Message = message_prefix + $"讨论 {discussion.Title}";
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
                    var files = discussion.Attachments;
                    var file_reg = new Regex(@"<a.+?instructure_file_link.+?title=""(.+?)"".+?href=""(.+?)"".+?</a>");
                    var file_matches = file_reg.Matches(discussion.Content);
                    var img_reg = new Regex(@"<img.+?src=""(.+?)"".+?alt=""(.+?)"".+?>");
                    var img_matches = img_reg.Matches(discussion.Content);
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

                    if (files.Count > 0)
                    {
                        var attachments = TodoService.ListAttachments(taskList.Id.ToString(), todoTask.Id.ToString());
                        foreach (var file in files)
                        {
                            var exist = attachments.Any(x => x.Name == file.DisplayName);
                            if (!exist)
                            {
                                WebClient client = new WebClient();
                                var data = client.DownloadData(file.Url);
                                if (data.Length > 25 * 1024 * 1024) continue;
                                Stream stream = new MemoryStream(data);

                                AttachmentInfo info = new AttachmentInfo();
                                info.AttachmentType = AttachmentType.File;
                                info.Size = data.Length;
                                info.Name = file.DisplayName;

                                TodoService.UploadAttachment(taskList.Id.ToString(), todoTask.Id.ToString(), info, stream);
                                updated = true;
                            }
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
            Message = message_prefix;
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
                    var updated = false;
                    ItemCount++;
                    Message = message_prefix + $"测验 {assignment.Name}";
                    TodoTask todoTask = null;
                    if (dicUrl.ContainsKey(assignment.HtmlUrl))
                        todoTask = dicUrl[assignment.HtmlUrl];
                    else
                        todoTask = null;

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
                    if (assignment.HasSubmittedSubmissions)
                    {
                        var links = TodoService.ListCheckItems(taskList.Id.ToString(), todoTask.Id.ToString());
                        var quizsubmissions = CanvasService.ListQuizSubmissons(course.Id.ToString(), assignment.QuizId.ToString());
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
                    //---Attachments---//
                    var file_reg = new Regex(@"<a.+?instructure_file_link.+?title=""(.+?)"".+?href=""(.+?)"".+?</a>");
                    var file_matches = file_reg.Matches(assignment.Content);
                    if (file_matches.Count > 0)
                    {
                        var attachments = TodoService.ListAttachments(taskList.Id.ToString(), todoTask.Id.ToString());
                        foreach (Match match in file_matches)
                        {
                            var filename = match.Groups[1].Value;
                            var filepath = match.Groups[2].Value;
                            var exist = attachments.Any(x => x.Name == filename);
                            if (!exist)
                            {
                                WebClient client = new WebClient();
                                var data = client.DownloadData(filepath);
                                if (data.Length > 25 * 1024 * 1024) continue;
                                Stream stream = new MemoryStream(data);

                                AttachmentInfo info = new AttachmentInfo();
                                info.AttachmentType = AttachmentType.File;
                                info.Size = data.Length;
                                info.Name = filename;

                                TodoService.UploadAttachment(taskList.Id.ToString(), todoTask.Id.ToString(), info, stream);
                                updated = true;
                            }
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
            Message = message_prefix;
            try
            {
                var anouncements = CanvasService.ListAnouncements(course.Id.ToString());
                if (anouncements == null)
                    return;
                if (anouncements.Count == 0)
                    return;

                foreach (var anouncement in anouncements)
                {
                    var updated = false;
                    ItemCount++;
                    Message = message_prefix + $"公告 {anouncement.Title}";
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
                    var files = anouncement.Attachments;
                    var file_reg = new Regex(@"<a.+?instructure_file_link.+?title=""(.+?)"".+?href=""(.+?)"".+?</a>");
                    var file_matches = file_reg.Matches(anouncement.Content);
                    var img_reg = new Regex(@"<img.+?src=""(.+?)"".+?alt=""(.+?)"".+?>");
                    var img_matches = img_reg.Matches(anouncement.Content);
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

                    if (files.Count > 0)
                    {
                        var attachments = TodoService.ListAttachments(taskList.Id.ToString(), todoTask.Id.ToString());
                        foreach (var file in files)
                        {
                            var exist = attachments.Any(x => x.Name == file.DisplayName);
                            if (!exist)
                            {
                                WebClient client = new WebClient();
                                var data = client.DownloadData(file.Url);
                                if (data.Length > 25 * 1024 * 1024) continue;
                                Stream stream = new MemoryStream(data);

                                AttachmentInfo info = new AttachmentInfo();
                                info.AttachmentType = AttachmentType.File;
                                info.Size = data.Length;
                                info.Name = file.DisplayName;

                                TodoService.UploadAttachment(taskList.Id.ToString(), todoTask.Id.ToString(), info, stream);
                                updated = true;
                            }
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

        #region Common
        private bool UpdateSubmissionInfo<T>(Assignment assignment, T submission, ChecklistItem checklistitemOld, ChecklistItem checklistitemNew, Func<Assignment, T, string> func)
        {
            var modified = false;
            var desc = func(assignment, submission);
            var check = !desc.Contains("未");
            checklistitemNew.IsChecked = checklistitemOld.IsChecked;
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
                if (todoTaskOld == null || todoTaskOld.Title == null || title != todoTaskOld.Title)
                {
                    todoTaskNew.Title = title;
                    modified = true;
                }
            }

            if (todoTaskOld == null && config.CreateContent || todoTaskOld != null && config.UpdateContent)
            {
                var content = CanvasStringTemplateHelper.GetContent(item);
                if (todoTaskOld == null || todoTaskOld.Body.Content == null || content != todoTaskOld.Body.Content)
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
        #endregion
    }

    public class SyncState
    {
        public SyncState(SyncStateEnum state, string message)
        {
            State = state;
            Message = message;
        }

        public SyncStateEnum State { get; set; }
        public string Message { get; set; }
    }

    public enum SyncStateEnum
    {
        Finished, Error, Progress
    }
}
