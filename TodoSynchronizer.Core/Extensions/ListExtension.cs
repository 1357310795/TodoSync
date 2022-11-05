using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Extensions
{
    public static class ListExtension
    {
        public static List<t> Shuffle<t>(this List<t> TList)
        {
            List<t> NewList = new List<t>();
            Random Rand = new Random();
            foreach (var item in TList)
            {
                NewList.Insert(Rand.Next(NewList.Count()), item);
            }
            return (NewList);
        }   
    }
}
