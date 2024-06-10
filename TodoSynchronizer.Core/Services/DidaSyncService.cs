using Microsoft.Graph.TermStore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using TodoSynchronizer.Core.Models.DidaModels;

namespace TodoSynchronizer.Core.Services
{
    public class DidaSyncService
    {
        public Dictionary<string, DidaTask> dicUrl = null;
        public Dictionary<string, DidaTaskList> dicCategory = null;
        public Dictionary<Course, DidaTaskList> dicCourse = null;
        public List<DidaTask> canvasTasks = null;
        public List<Course> courses = null;
        public DidaBatchCheckDto batchCheckDto = null;
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
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Utc, NullValueHandling = NullValueHandling.Ignore, DateFormatString = "yyyy-MM-ddTHH:mm:ss.fff+0000" };
            CourseCount = 0;
            ItemCount = 0;
            UpdateCount = 0;
            FailedCount = 0;
            #endregion

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

            #region 检查滴答清单
            Message = "检查滴答清单";
            try
            {
                dicCategory = new Dictionary<string, DidaTaskList>();
                dicCourse = new Dictionary<Course, DidaTaskList>();
                batchCheckDto = DidaService.BatchCheck();
                if (batchCheckDto.ProjectProfiles == null || batchCheckDto.ProjectProfiles.Count == 0)
                    throw new Exception("清单 ProjectProfiles 为空");

                void FindList(string cat, string name)
                {
                    var taskList = batchCheckDto.ProjectProfiles.Find(x => x.Name.CleanEmoji() == name);

                    if (taskList == null)
                        taskList = DidaService.AddTaskList(name);

                    if (taskList == null)
                        throw new Exception("创建滴答清单失败");
                    else
                        Message = $"找到滴答清单：{taskList.Name}";
                    dicCategory.Add(cat, taskList);
                }

                if (SyncConfig.Default.NotificationConfig.Enabled)
                    FindList("notification", CanvasStringTemplateHelper.GetNotificationListName());

                //滴答清单只支持 Category
                if (true || SyncConfig.Default.ListNameMode == ListNameMode.Category)
                {
                    if (SyncConfig.Default.QuizConfig.Enabled)
                        FindList("quiz", SyncConfig.Default.ListNamesForCategory.QuizListName);
                    if (SyncConfig.Default.DiscussionConfig.Enabled)
                        FindList("discussion", SyncConfig.Default.ListNamesForCategory.DiscussionListName);
                    if (SyncConfig.Default.AssignmentConfig.Enabled)
                        FindList("assignment", SyncConfig.Default.ListNamesForCategory.AssignmentListName);
                    if (SyncConfig.Default.AnnouncementConfig.Enabled)
                        FindList("announcement", SyncConfig.Default.ListNamesForCategory.AnnouncementListName);
                }
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
            #endregion

            #region 读取滴答清单项目
            Message = "读取滴答清单项目";
            try
            {
                IEnumerable<DidaTask> ListDidaTasksInternal()
                {
                    var lists = new HashSet<string>();
                    foreach (var item in dicCategory)
                        lists.Add(item.Value.Id);

                    var tmplist = batchCheckDto.SyncTaskBean.Update;
                    foreach (var task in tmplist)
                        if (lists.Contains(task.ProjectId))
                            yield return task;

                    foreach (var item in dicCategory)
                    {
                        tmplist = DidaService.GetCompleted(item.Value.Id);
                        foreach (var task in tmplist)
                            yield return task;
                    }
                }
                canvasTasks = ListDidaTasksInternal().ToList();
                if (canvasTasks == null)
                    throw new Exception("滴答清单项目为空");
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
            #endregion

            #region 处理 Comment
            Message = "建立字典";
            try
            {
                dicUrl = new Dictionary<string, DidaTask>();
                foreach (var didaTask in canvasTasks)
                {
                    var comments = DidaService.ListTaskComments(didaTask.ProjectId, didaTask.Id);
                    if (comments != null && comments.Count > 0)
                    {
                        var url = comments.First().Title;
                        Uri uri;
                        if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                            dicUrl.Add(url, didaTask);
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
                if (true || SyncConfig.Default.ListNameMode == ListNameMode.Category)
                {
                    foreach (var course in courses)
                    {
                        CourseCount++;
                        if (SyncConfig.Default.AssignmentConfig.Enabled)
                            ProcessAssignments(GetCourseMessage(course), course, dicCategory["assignment"]);
                        if (SyncConfig.Default.AnnouncementConfig.Enabled)
                            ProcessAnnouncements(GetCourseMessage(course), course, dicCategory["announcement"]);
                        if (SyncConfig.Default.QuizConfig.Enabled)
                            ProcessQuizes(GetCourseMessage(course), course, dicCategory["quiz"]);
                        if (SyncConfig.Default.DiscussionConfig.Enabled)
                            ProcessDiscussions(GetCourseMessage(course), course, dicCategory["discussion"]);
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
        public void ProcessAssignments(string message_prefix, Course course, DidaTaskList taskList)
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
                    DidaTask didaTask = null;
                    if (dicUrl.ContainsKey(assignment.HtmlUrl))
                        didaTask = dicUrl[assignment.HtmlUrl];
                    else
                        didaTask = new DidaTask()
                        {
                            Id = Common.GetRandomString(24, true, false, false, false, "abcdef"),
                            ProjectId = taskList.Id,
                        };
                    var isnew = didaTask.Creator is null;

                    //---Self & LinkedResource---//
                    var res1 = UpdateCanvasItem(course, assignment, didaTask, SyncConfig.Default.AssignmentConfig);
                    if (res1)
                    {
                        if (didaTask.Creator is null)
                        {
                            DidaService.AddTask(didaTask);
                            DidaService.AddTaskComment(taskList.Id.ToString(), didaTask.Id.ToString(), assignment.HtmlUrl);
                            dicUrl.Add(assignment.HtmlUrl, didaTask);
                        }
                        else
                        {
                            DidaService.UpdateTask(didaTask);
                        }
                        updated = true;
                    }

                    //---Submissions -> CheckItems---//
                    if (SyncConfig.Default.AssignmentConfig.CreateScoreAndCommit && isnew
                        || SyncConfig.Default.AssignmentConfig.UpdateScoreAndCommit && !isnew)
                        if (assignment.HasSubmittedSubmissions)
                        {
                            bool checkitemupdated = false;
                            if (didaTask.Items == null)
                                didaTask.Items = new List<DidaCheckItem>();
                            var links = didaTask.Items;

                            var submission = CanvasService.GetAssignmentSubmisson(course.Id.ToString(), assignment.Id.ToString());

                            DidaCheckItem checkitem1 = null;
                            if (links.Count >= 1)
                                checkitem1 = links[0];
                            else
                            {
                                links.Add(new DidaCheckItem() { Id= Common.GetRandomString(24, true, false, false, false, "abcdef")});
                                checkitem1 = links[0];
                            }
                                
                            var res2 = UpdateSubmissionInfo(assignment, submission, checkitem1, CanvasStringTemplateHelper.GetSubmissionDesc);
                            checkitemupdated |= res2;
                            
                            DidaCheckItem checkitem2 = null;
                            if (links.Count >= 2)
                                checkitem2 = links[1];
                            else
                            {
                                links.Add(new DidaCheckItem() { Id = Common.GetRandomString(24, true, false, false, false, "abcdef") });
                                checkitem2 = links[1];
                            }

                            var res3 = UpdateSubmissionInfo(assignment, submission, checkitem2, CanvasStringTemplateHelper.GetGradeDesc);
                            checkitemupdated |= res3;

                            //---Comments---//
                            if (SyncConfig.Default.AssignmentConfig.CreateComments && isnew
                                || SyncConfig.Default.AssignmentConfig.UpdateComments && !isnew)
                            {
                                if (submission.SubmissionComments?.Count > 0)
                                {
                                    int i = 2;
                                    foreach (var comment in submission.SubmissionComments)
                                    {
                                        DidaCheckItem checkitem3 = null;
                                        if (links.Count >= i + 1)
                                            checkitem3 = links[i];
                                        else
                                        {
                                            links.Add(new DidaCheckItem() { Id = Common.GetRandomString(24, true, false, false, false, "abcdef") });
                                            checkitem3 = links[i];
                                        }
                                        i++;

                                        var res4 = UpdateSubmissionComment(comment, checkitem3);
                                        checkitemupdated |= res4;
                                    }
                                }
                            }

                            if (checkitemupdated)
                            {
                                DidaService.UpdateTask(didaTask);
                                updated = true;
                            }
                        }

                    //---Attachments---//
                    //if (SyncConfig.Default.AssignmentConfig.CreateAttachments)
                    //{
                    //    var files = new List<Models.CanvasModels.Attachment>();

                    //    if (assignment.Content != null)
                    //    {
                    //        CheckAttachments(assignment.Content, files);
                    //    }

                    //    if (files.Count > 0)
                    //    {
                    //        updated |= UploadAttachments(taskList, didaTask, files);
                    //    }
                    //}
                        
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
        private void ProcessDiscussions(string message_prefix, Course course, DidaTaskList taskList)
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
                    DidaTask didaTask = null;
                    if (dicUrl.ContainsKey(discussion.HtmlUrl))
                        didaTask = dicUrl[discussion.HtmlUrl];
                    else
                        didaTask = new DidaTask()
                        {
                            Id = Common.GetRandomString(24, true, false, false, false, "abcdef"),
                            ProjectId = taskList.Id,
                        };

                    //---Self & LinkedResource---//
                    var res1 = UpdateCanvasItem(course, discussion, didaTask, SyncConfig.Default.DiscussionConfig);
                    if (res1)
                    {
                        if (didaTask.Creator is null)
                        {
                            DidaService.AddTask(didaTask);
                            DidaService.AddTaskComment(taskList.Id.ToString(), didaTask.Id.ToString(), discussion.HtmlUrl);
                            dicUrl.Add(discussion.HtmlUrl, didaTask);
                        }
                        else
                        {
                            DidaService.UpdateTask(didaTask);
                        }
                        updated = true;
                    }

                    //---Attachments---//
                    //if (SyncConfig.Default.DiscussionConfig.CreateAttachments)
                    //{
                    //    var files = discussion.Attachments;
                        
                    //    if (discussion.Content != null)
                    //    {
                    //        CheckAttachments(discussion.Content, files);
                    //    }

                    //    if (files.Count > 0)
                    //    {
                    //        updated |= UploadAttachments(taskList, didaTask, files);
                    //    }
                    //}
                        
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
        private void ProcessQuizes(string message_prefix, Course course, DidaTaskList taskList)
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
                    DidaTask didaTask = null;
                    if (dicUrl.ContainsKey(assignment.HtmlUrl))
                        didaTask = dicUrl[assignment.HtmlUrl];
                    else
                        didaTask = new DidaTask()
                        {
                            Id = Common.GetRandomString(24, true, false, false, false, "abcdef"),
                            ProjectId = taskList.Id,
                        };
                    var isnew = didaTask == null;

                    //---Self & LinkedResource---//
                    var res1 = UpdateCanvasItem(course, assignment, didaTask, SyncConfig.Default.QuizConfig);
                    if (res1)
                    {
                        if (didaTask.Creator is null)
                        {
                            DidaService.AddTask(didaTask);
                            DidaService.AddTaskComment(taskList.Id.ToString(), didaTask.Id.ToString(), assignment.HtmlUrl);
                            dicUrl.Add(assignment.HtmlUrl, didaTask);
                        }
                        else
                        {
                            DidaService.UpdateTask(didaTask);
                        }
                        updated = true;
                    }

                    //---Submissions -> CheckItems---//
                    if (SyncConfig.Default.QuizConfig.CreateScoreAndCommit && isnew
                        || SyncConfig.Default.QuizConfig.UpdateScoreAndCommit && !isnew)
                        if (assignment.HasSubmittedSubmissions)
                        {
                            if (didaTask.Items == null)
                                didaTask.Items = new List<DidaCheckItem>();
                            var links = didaTask.Items;
                            var quizsubmissions = CanvasService.ListQuizSubmissons(course.Id.ToString(), assignment.QuizId.ToString());

                            if (quizsubmissions != null)
                            {
                                for (int i = 0; i < quizsubmissions.Count; i++)
                                {
                                    DidaCheckItem checkitem0 = null;
                                    if (links.Count >= i + 1)
                                        checkitem0 = links[i];
                                    else
                                    {
                                        links.Add(new DidaCheckItem() { Id = Common.GetRandomString(24, true, false, false, false, "abcdef") });
                                        checkitem0 = links[i];
                                    }

                                    var res4 = UpdateSubmissionInfo(assignment, quizsubmissions[i], checkitem0, CanvasStringTemplateHelper.GetSubmissionDesc);
                                    if (res4)
                                    {
                                        DidaService.UpdateTask(didaTask);
                                        updated = true;
                                    }
                                }
                            }
                        
                        }
                    //---Attachments---//
                    //if (SyncConfig.Default.QuizConfig.CreateAttachments)
                    //{
                    //    var files = new List<Models.CanvasModels.Attachment>();

                    //    if (assignment.Content != null)
                    //    {
                    //        CheckAttachments(assignment.Content, files);
                    //    }
                        
                    //    if (files.Count > 0)
                    //    {
                    //        updated |= UploadAttachments(taskList, didaTask, files);
                    //    }
                    //}
                    
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

        #region Announcements
        private void ProcessAnnouncements(string message_prefix, Course course, DidaTaskList taskList)
        {
            Message = message_prefix + "公告";
            try
            {
                var announcements = CanvasService.ListAnnouncements(course.Id.ToString());
                if (announcements == null)
                    return;
                if (announcements.Count == 0)
                    return;

                foreach (var announcement in announcements)
                {
                    if (SyncConfig.Default.IgnoreTooOldItems)
                        if (announcement?.PostedAt?.ToUniversalTime() < DateTime.Now.AddDays(-14).ToUniversalTime())
                            continue;
                    var updated = false;
                    ItemCount++;
                    Message = message_prefix + GetItemMessage(announcement);
                    DidaTask didaTask = null;
                    if (dicUrl.ContainsKey(announcement.HtmlUrl))
                        didaTask = dicUrl[announcement.HtmlUrl];
                    else
                        didaTask = new DidaTask()
                        {
                            Id = Common.GetRandomString(24, true, false, false, false, "abcdef"),
                            ProjectId = taskList.Id,
                        };

                    //---Self & LinkedResource---//
                    var res1 = UpdateCanvasItem(course, announcement, didaTask, SyncConfig.Default.AnnouncementConfig);
                    if (res1)
                    {
                        if (didaTask.Creator is null)
                        {
                            DidaService.AddTask(didaTask);
                            DidaService.AddTaskComment(taskList.Id.ToString(), didaTask.Id.ToString(), announcement.HtmlUrl);
                            dicUrl.Add(announcement.HtmlUrl, didaTask);
                        }
                        else
                        {
                            DidaService.UpdateTask(didaTask);
                        }
                        updated = true;
                    }

                    //---Attachments---//
                    //if (SyncConfig.Default.AnnouncementConfig.CreateAttachments)
                    //{
                    //    var files = announcement.Attachments;
                    //    var file_reg = new Regex(@"<a.+?instructure_file_link.+?title=""(.+?)"".+?href=""(.+?)"".+?</a>");

                    //    if (announcement.Content != null)
                    //    {
                    //        CheckAttachments(announcement.Content, files);
                    //    }
                        
                    //    if (files.Count > 0)
                    //    {
                    //        updated |= UploadAttachments(taskList, didaTask, files);
                    //    }
                    //}
                        
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

                IEnumerable<DidaTask> ListDidaTasksInternal(string listid)
                {
                    var tmplist2 = batchCheckDto.SyncTaskBean.Update;
                    foreach (var task in tmplist2)
                        if (task.ProjectId == listid)
                            yield return task;

                    tmplist2 = DidaService.GetCompleted(listid);
                    foreach (var task in tmplist2)
                        yield return task;
                }

                var tmplist = ListDidaTasksInternal(notilist.Id).ToList();
                var dic = new Dictionary<string, DidaTask>();

                foreach (var didaTask in tmplist)
                {
                    if (didaTask.CommentCount != null && didaTask.CommentCount > 0)
                    {
                        var comments = DidaService.ListTaskComments(didaTask.ProjectId, didaTask.Id);
                        var url = comments.First().Title;
                        dic.Add(url, didaTask);
                    }
                }

                foreach (var notification in notifications)
                {
                    var updated = false;
                    ItemCount++;
                    Message = "处理全局通知 " + notification.Subject;
                    DidaTask didaTask = null;
                    didaTask = tmplist.FirstOrDefault(x => x.Title == notification.Subject);

                    if (dic.ContainsKey(notification.Id.ToString()))
                        didaTask = dic[notification.Id.ToString()];
                    else
                        didaTask = new DidaTask()
                        {
                            Id = Common.GetRandomString(24, true, false, false, false, "abcdef"),
                            ProjectId = notilist.Id,
                        };

                    //---Self---//
                    var res1 = UpdateCanvasItem(null, notification, didaTask, SyncConfig.Default.NotificationConfig);

                    if (res1)
                    {
                        if (didaTask.Creator is null)
                        {
                            DidaService.AddTask(didaTask);
                            DidaService.AddTaskComment(notilist.Id.ToString(), didaTask.Id.ToString(), notification.Id.ToString());
                            dicUrl.Add(notification.Id.ToString(), didaTask);
                        }
                        else
                        {
                            DidaService.UpdateTask(didaTask);
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
        private bool UpdateSubmissionInfo<T>(Assignment assignment, T submission, DidaCheckItem checklistitem, Func<Assignment, T, string> func)
        {
            var modified = false;
            var desc = func(assignment, submission);
            var check = !desc.Contains("未") && !desc.Contains("正在");

            if (checklistitem.Status != (check ? 1 : 0))
            {
                checklistitem.Status = (check ? 1 : 0);
                modified = true;
            }
            if (checklistitem == null || checklistitem.Title != desc)
            {
                checklistitem.Title = desc;
                modified = true;
            }
            return modified;
        }

        private bool UpdateSubmissionComment(SubmissionComment comment, DidaCheckItem checklistitem)
        {
            var modified = false;
            var desc = CanvasStringTemplateHelper.GetSubmissionComment(comment);
            var check = true;

            if (checklistitem.Status != (check ? 1 : 0))
            {
                checklistitem.Status = (check ? 1 : 0);
                modified = true;
            }
            if (checklistitem == null || checklistitem.Title != desc)
            {
                checklistitem.Title = desc;
                modified = true;
            }
            return modified;
        }

        public bool UpdateCanvasItem(Course course, ICanvasItem item, DidaTask didaTask, ICanvasItemConfig config)
        {
            var modified = false;

            if (didaTask == null || didaTask != null && config.UpdateTitle)
            {
                var title = CanvasStringTemplateHelper.GetTitle(course, item);
                if (didaTask == null || didaTask.Title == null || title.Trim() != didaTask.Title.Trim())
                {
                    didaTask.Title = title;
                    modified = true;
                }
            }

            if (didaTask == null && config.CreateContent || didaTask != null && config.UpdateContent)
            {
                var content = CanvasStringTemplateHelper.GetContent(item);
                if (didaTask == null || didaTask.Desc == null || content.Trim() != didaTask.Desc.Trim())
                {
                    didaTask.Desc = content;
                    modified = true;
                }
            }

            if (didaTask == null && config.CreateDueDate || didaTask != null && config.UpdateDueDate)
            {
                var duetime = CanvasPreference.GetDueTime(item);
                if (duetime.HasValue)
                {
                    var date = duetime.Value.ToUniversalTime();
                    if (didaTask == null || didaTask.DueDate == null || date != didaTask.DueDate)
                    {
                        didaTask.DueDate = didaTask.StartDate = didaTask.RepeatFirstDate = date;
                        didaTask.TimeZone = "Asia/Shanghai";
                        modified = true;
                    }
                }
                else if (didaTask != null && didaTask.DueDate != null)
                {
                    didaTask.DueDate = didaTask.StartDate = null;
                    modified = true;
                }
            }

            if (didaTask == null && config.CreateRemind || didaTask != null && config.UpdateRemind)
            {
                var remindtime = CanvasPreference.GetRemindBefore(config);
                if (remindtime.HasValue && didaTask.RepeatFirstDate != null)
                {
                    var trigger = CanvasStringTemplateHelper.GetTrigger(remindtime.Value);
                    if (didaTask == null || didaTask.Reminders == null || didaTask.Reminders.Count == 0 || didaTask.Reminders[0].Trigger != trigger)
                    {
                        didaTask.Reminders = new List<Reminder>();
                        didaTask.Reminders.Add(new Reminder() { Id = Common.GetRandomString(24, true, false, false, false, "abcdef"), Trigger = trigger });

                        modified = true;
                    }
                }
            }

            if (didaTask == null && config.CreateImportance || didaTask != null && config.UpdateImportance)
            {
                var importance = config.SetImportance;
                if (didaTask == null || didaTask.Priority != (importance ? 5 : 0))
                {
                    didaTask.Priority = (importance ? 5 : 0);
                    modified = true;
                }
            }

            return modified;
        }


        private string GetCourseMessage(Course course)
        {
            return $"处理课程 {(SyncConfig.Default.VerboseMode ? course.Name : CourseCount)} ";
        }

        private string GetItemMessage(ICanvasItem item)
        {
            return $"{item.GetItemName()} {(SyncConfig.Default.VerboseMode ? item.Title : ItemCount)} ";
        }


        #endregion

        #region Attachments
        //private static void CheckAttachments(string content, List<Models.CanvasModels.Attachment> files)
        //{
        //    var file_reg = new Regex(@"<a.+?instructure_file_link.+?title=""(.+?)"".+?href=""(.+?)"".+?</a>");
        //    var file_matches = file_reg.Matches(content);
        //    var img_reg = new Regex(@"<img.+?src=""(.+?)"".+?alt=""(.+?)"".+?>");
        //    var img_matches = img_reg.Matches(content);
        //    foreach (Match match in file_matches)
        //    {
        //        var filename = match.Groups[1].Value;
        //        var filepath = match.Groups[2].Value;
        //        files.Add(new Core.Models.CanvasModels.Attachment() { DisplayName = filename, Url = filepath, Locked = false });
        //    }
        //    foreach (Match match in img_matches)
        //    {
        //        var filename = match.Groups[2].Value;
        //        var filepath = match.Groups[1].Value;
        //        files.Add(new Core.Models.CanvasModels.Attachment() { DisplayName = filename, Url = filepath, Locked = false });
        //    }
        //}

        //private bool UploadAttachments(DidaTaskList taskList, DidaTask didaTask, List<Models.CanvasModels.Attachment> files)
        //{
        //    var updated = false;
        //    try
        //    {
        //        var attachments = DidaService.ListAttachments(taskList.Id.ToString(), didaTask.Id.ToString());
        //        foreach (var file in files)
        //        {
        //            var exist = attachments.Any(x => x.Name == file.DisplayName);
        //            if (!exist)
        //            {
        //                file.Url = file.Url.UrlUnescape().EscToHtml();
        //                Uri fulluri;
        //                var isabsolute = Uri.TryCreate(file.Url, UriKind.Absolute, out fulluri);
        //                if (!isabsolute)
        //                {
        //                    var urires = Uri.TryCreate(new Uri("https://oc.sjtu.edu.cn"), file.Url, out fulluri);
        //                    if (!urires)
        //                        throw new Exception($"Uri无效：{file.Url}");
        //                }

        //                HttpClient client = CanvasService.Client;
        //                HttpResponseMessage res = null;
        //                try
        //                {
        //                    var datatask = client.GetAsync(fulluri);
        //                    //datatask.RunSynchronously();
        //                    datatask.Wait();
        //                    res = datatask.GetAwaiter().GetResult();
        //                }
        //                catch (Exception ex)
        //                {
        //                    throw new Exception($"获取文件时发生错误\n{fulluri.AbsoluteUri}\n{ex.Message}");
        //                }

        //                if (res.StatusCode != HttpStatusCode.OK)
        //                    throw new Exception($"获取文件时发生错误\n{fulluri.AbsoluteUri}\n[{(int)res.StatusCode} {res.StatusCode.ToString()}] {res.Content.ReadAsStringAsync().Result}");
        //                var data = res.Content.ReadAsByteArrayAsync().Result;

        //                if (data.Length > 25 * 1024 * 1024) continue;
        //                Stream stream = new MemoryStream(data);

        //                //HttpWebRequest req = (HttpWebRequest)WebRequest.Create(fulluri);
        //                //req.Method = "GET";
        //                //HttpWebResponse resp = null;
        //                //resp = (HttpWebResponse)req.GetResponse();
        //                //Stream stream = null;
        //                //stream = resp.GetResponseStream();
        //                //MemoryStream ms = new MemoryStream();
        //                //stream.CopyTo(ms);
        //                //var data = StreamToBytes(ms);

        //                AttachmentInfo info = new AttachmentInfo();
        //                info.AttachmentType = AttachmentType.File;
        //                info.Size = data.Length;
        //                info.Name = file.DisplayName;

        //                DidaService.UploadAttachment(taskList.Id.ToString(), didaTask.Id.ToString(), info, stream);
        //                updated = true;
        //            }
        //        }
        //        return updated;
        //    }
        //    catch (Exception ex)
        //    {
        //        OnReportProgress.Invoke(new SyncState(SyncStateEnum.Progress, $"上传文件失败：{ex.Message}"));
        //        return false;
        //    }
        //}

        //public static byte[] StreamToBytes(Stream stream)
        //{

        //    byte[] bytes = new byte[stream.Length];
        //    stream.Read(bytes, 0, bytes.Length);
        //    stream.Seek(0, SeekOrigin.Begin);
        //    return bytes;
        //}
        #endregion
    }
}
