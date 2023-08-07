using System.Collections.Generic;

namespace Series.KeyValueDatabase.Models
{
    public class SortedSetRangeDetailResult<T>
    {
        public string[] Keys { get; set; }

        public List<T> Items { get; set; }

        public SortedSetRangeDetailResult()
        {
            Items = new List<T>();
        }
    }
}
