using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace TeamworkMsprojectExportTweaker
{
    public abstract class MsElement
    {
        public MsElement(XElement xml)
        {
            Xml = xml;
        }

        public XElement Xml { get; set; }
    }

    public class Task : MsElement
    {
        public Task(XElement xml)
            : base(xml)
        {
        }

        public int ID { get; set; }
        public int UID { get; set; }
        public string Name { get; set; }
        public string ManualFinish { get; set; }
        public string ManualStart { get; set; }
        public string Work { get; set; }
        public string Duration { get; set; }
        public List<Assignment> Assignments { get; set; }

        public override string ToString()
        {
            return string.Format("Task {0} ({1}), {2} work={3} duration={4}", ID, UID, Name, Work, Duration);
        }
    }

    public class Assignment : MsElement
    {
        public Assignment(XElement xml)
            : base(xml)
        {
        }

        public int UID { get; set; }
        public int TaskUID { get; set; }
        //public Task Task { get; set; }

        public override string ToString()
        {
            return string.Format("Assignment {0} of task {1}", UID, TaskUID);
        }
    }

    public class Resource : MsElement
    {
        public Resource(XElement xml)
            : base(xml)
        {
        }

        public string ID { get; set; }

        public string Name { get; set; }
    }

    public class Project : MsElement
    {
        public Project(XElement xml)
            : base(xml)
        {
        }

        public string Title { get; set; }
        public List<Task> Tasks { get; set; }
        public List<Assignment> Assignments { get; set; }
        public List<Resource> Resources { get; set; }
    }
}