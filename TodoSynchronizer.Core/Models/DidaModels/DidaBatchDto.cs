using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Models.DidaModels
{
    public class DidaBatchDto
    {
        public DidaBatchDto()
        {
            add = new List<DidaTask>();
            update = new List<DidaTask>();
            delete = new List<DidaTask>();
            addAttachments = new List<DidaTask>();
            updateAttachments = new List<DidaTask>();
            deleteAttachments = new List<DidaTask>();
        }

        public List<DidaTask> add { get; set; }
        public List<DidaTask> update { get; set; }
        public List<DidaTask> delete { get; set; }
        public List<DidaTask> addAttachments { get; set; }
        public List<DidaTask> updateAttachments { get; set; }
        public List<DidaTask> deleteAttachments { get; set; }
    }
}
