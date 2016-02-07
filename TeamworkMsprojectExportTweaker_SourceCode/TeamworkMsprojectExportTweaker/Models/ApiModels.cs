using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamworkMsprojectExportTweaker
{
    public abstract class TwBase
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", Id, Name);
        }
    }
    public class TwProject : TwBase
    {
        //public List<Item> AllItems { get; set; }
        public List<TwItemList> Lists { get; set; }
    }
    public class TwItemList : TwBase
    {
        public List<TwItem> Items { get; set; }
    }
    public class TwItem : TwBase
    {
        public int? ParentId { get; set; }
        public int Progress { get; set; }
        public int Order { get; set; }
        public List<TwItem> Childs { get; set; }
    }
}
