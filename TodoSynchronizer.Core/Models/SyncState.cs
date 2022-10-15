using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Models
{   
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
