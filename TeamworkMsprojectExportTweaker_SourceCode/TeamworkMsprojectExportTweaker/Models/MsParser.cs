using NLog;
using System;
using System.Linq;
using System.Xml.Linq;

namespace TeamworkMsprojectExportTweaker
{
    public static class MsParser
    {
        static readonly XNamespace ns = "http://schemas.microsoft.com/project";
        static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public static Project ParseProject(this string xml)
        {
            var doc = XDocument.Parse(xml);

            var project = new Project(doc.Root);

            project.Title = doc.Descendants(ns + "Title").FirstOrDefault().Value;
            project.Tasks = doc.Descendants(ns + "Task").Select(x =>
                new Task(x)
                {
                    ID = int.Parse(x.Element(ns + "ID").Value),
                    UID = int.Parse(x.Element(ns + "UID").Value),
                    Name = x.Element(ns + "Name").Value,
                    ManualFinish = x.Element(ns + "ManualFinish") == null ? "" : x.Element(ns + "ManualFinish").Value,
                    ManualStart = x.Element(ns + "ManualStart") == null ? "" : x.Element(ns + "ManualStart").Value,
                    Work = x.Element(ns + "Work") == null ? "" : x.Element(ns + "Work").Value,
                    Duration = x.Element(ns + "Duration") == null ? "" : x.Element(ns + "Duration").Value,
                }).ToList();
            project.Assignments = doc.Descendants(ns + "Assignment").Select(x =>
                new Assignment(x)
                {
                    UID = int.Parse(x.Element(ns + "UID").Value),
                    TaskUID = int.Parse(x.Element(ns + "TaskUID").Value),
                }
                ).ToList();

            foreach (var t in project.Tasks)
            {
                t.Assignments = project.Assignments.Where(x => x.TaskUID == t.UID).ToList();
            }

            return project;
        }
    }
}