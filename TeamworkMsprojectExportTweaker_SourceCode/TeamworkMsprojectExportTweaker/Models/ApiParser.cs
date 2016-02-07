using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace TeamworkMsprojectExportTweaker
{
    public static class ApiParser
    {
        static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public static List<TwProject> ParseProjects(this string xml)
        {
            var doc = XDocument.Parse(xml);
            var projects = doc.Descendants("project").Select(x => new TwProject()
            {
                Id = int.Parse(x.Element("id").Value),
                Name = x.Element("name").Value
            }).ToList();
            logger.Info("got {0} projects", projects.Count);
            return projects;
        }

        public static List<TwItemList> ParseLists(this string xml)
        {
            var doc = XDocument.Parse(xml);
            var lists = doc.Descendants("tasklist").Select(x => new TwItemList()
            {
                Id = int.Parse(x.Element("id").Value),
                Name = x.Element("name").Value,
            }).ToList();
            logger.Info("got {0} items", lists.Count);
            return lists;
        }

        public static List<TwItem> ParseItems(this string xml)
        {
            var doc = XDocument.Parse(xml);
            var todoItems = doc.Descendants("todo-item").Select(x => new TwItem()
            {
                Id = int.Parse(x.Element("id").Value),
                Name = x.Element("content").Value,
                ParentId = x.Element("parenttaskid").Value.ToNullableInt32(),
                Progress = int.Parse(x.Element("progress").Value),
                Order = int.Parse(x.Element("order").Value),
            }).ToList();
            logger.Info("got {0} items", todoItems.Count);
            return todoItems;
        }

        public static int? ToNullableInt32(this string s)
        {
            int i;
            if (Int32.TryParse(s, out i)) return i;
            return null;
        }
    }
}