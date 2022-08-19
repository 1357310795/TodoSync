using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Models.CanvasModels
{
    public interface ICanvasItem
    {
        long Id { get; set; }
        string Title { get; set; }
        string Content { get; set; }
        string HtmlUrl { get; set; }
    }
}
