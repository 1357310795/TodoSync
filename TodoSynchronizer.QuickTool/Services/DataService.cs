using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.QuickTool.Services
{
    public static class DataService
    {
        private static Dictionary<string, Object> _data = new Dictionary<string, object>();

        public static T GetData<T>(string key)
        {
            return (T)_data[key];
        }

        public static void SetData(string key, object data)
        {
            _data.Add(key, data);
        }
    }
}
