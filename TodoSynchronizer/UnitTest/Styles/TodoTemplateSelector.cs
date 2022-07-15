using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TodoSynchronizer.UnitTest
{
    public class TodoTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TodoListTemplate { get; set; }
        public DataTemplate TodoItemTemplate { get; set; }
        public DataTemplate TodoLinkedResourceTemplate { get; set; }
        public DataTemplate TodoCheckItemTemplate { get; set; }
        public override DataTemplate SelectTemplate(object obj, DependencyObject container)
        {
            if (obj == null) return null;
            if (obj is TodoTaskList) return TodoListTemplate;
            if (obj is TodoTask) return TodoItemTemplate;
            if (obj is ChecklistItem) return TodoCheckItemTemplate;
            if (obj is LinkedResource) return TodoLinkedResourceTemplate;
            return null;
        }
    }
}
