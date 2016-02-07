using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NLog;

namespace TeamworkMsprojectExportTweaker
{
    public class Fixer
    {
        static readonly XNamespace ns = "http://schemas.microsoft.com/project";
        static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        Project msProject;

        public Fixer(Project badMsProject)
        {
            this.msProject = badMsProject;

            // ms project hangs when open "Change working time" with 0 resource
            var badRes = msProject.Xml.Descendants(ns + "Resource").Where(x => x.Element(ns + "UID").Value == "0").FirstOrDefault();
            badRes.Remove();
        }

        public XDocument Fix(TwProject twProject)
        {
            logger.Info("start fixing '{0}' project", twProject.Name);

            // try match ms proj Task with Tw Lists and Items by name
            // may be many tasks

            var tasksPerList = twProject.Lists.Select(x =>
                new TasksOfItemList()
                {
                    List = x,
                    Tasks = msProject.Tasks
                        .Where(z => z.Assignments.Count == 0)
                        .Where(z => z.Name == x.Name).ToList()
                })
                .ToList();

            foreach (var tsPL in tasksPerList)
            {
                var listname = tsPL.List.Name;
                if (tsPL.Tasks.Count == 1)
                {
                    var listTask = tsPL.Tasks[0];
                    logger.Info("= fixing list {0}", listname);

                    var nextListTaskId = GetNextListTaskId(tasksPerList, tsPL);

                    var tasksPerItem = tsPL.List.Items.Select(x =>
                        new TasksOfItem()
                        {
                            Item = x,

                            // may be same name of items, but in diff lists
                            // ms tasks follows grouping task, id by id
                            // hope there is no samenamed in each list
                            Tasks = msProject.Tasks
                                .Where(z => listTask.ID < z.ID && z.ID < nextListTaskId)
                                .Where(z => z.Name == x.Name).ToList()
                        })
                        .ToList();

                    foreach (var tsPI in tasksPerItem)
                    {
                        var itemName = tsPI.Item.Name;

                        if (tsPI.Tasks.Count == 1)
                        {
                            logger.Info("== fixing item {0} - {1}%", itemName, tsPI.Item.Progress);
                            FixItem(tsPI.Tasks[0], tsPI.Item.Progress);
                        }
                        else if (tsPI.Tasks.Count == 0)
                        {
                            logger.Debug("== no tasks for item {0}", itemName);
                        }
                        else
                        {
                            logger.Warn("== many tasks for item {0} - {1}", itemName, tsPI.Tasks.Count);
                        }
                    }
                }
                else if (tsPL.Tasks.Count == 0)
                {
                    logger.Debug("= no tasks for list {0}", listname);
                }
                else
                {
                    logger.Warn("= many tasks for list {0} - {1}", listname, tsPL.Tasks.Count);
                }
            }

            return new XDocument(msProject.Xml);
        }

        private int GetNextListTaskId(List<TasksOfItemList> tasksPerList, TasksOfItemList current)
        {
            var nextInd = tasksPerList.IndexOf(current) + 1;
            if (nextInd < tasksPerList.Count)
            {
                if (tasksPerList[nextInd].Tasks.Count == 1)
                {
                    logger.Debug("next list task id = {0}", tasksPerList[nextInd].Tasks[0].ID);
                    return tasksPerList[nextInd].Tasks[0].ID;
                }
                else if (tasksPerList[nextInd].Tasks.Count == 0)
                {
                    // no task in xml for that list, skip
                    return GetNextListTaskId(tasksPerList, tasksPerList[nextInd]);
                }
                else
                {
                    logger.Debug("not single task for list {0}", tasksPerList[nextInd].List.Name);
                    return int.MaxValue;
                }
            }
            else
            {
                return int.MaxValue;
            }
        }

        private static void FixItem(Task itemTask, int percent)
        {
            // start/finish == export date
            var manualStart = itemTask.ManualStart;
            var manualFinish = itemTask.ManualFinish;

            var workTime = TimeParser.ToTime(itemTask.Work);
            var durTime = TimeParser.ToTime(itemTask.Duration);
            if (workTime == TimeSpan.Zero)
            {
                logger.Debug("no work for task {0}", itemTask);
            }
            if (durTime == TimeSpan.Zero)
            {
                logger.Debug("no duration for task {0}", itemTask);
            }

            var actualDurationString = PercentsOfTime(durTime, percent);
            var remainDurationString = PercentsOfTime(durTime, 100 - percent);
            //logger.Trace("{0}% of {1} = {2}", percent, work, actualDurationString);

            // percents ignored and calculated by ms project : pc = ad/d and pwc = aw/w

            //itemTask.Xml.Add(new XElement(ns + "PercentWorkComplete", percent));
            //itemTask.Xml.Add(new XElement(ns + "PercentComplete", percent));

            itemTask.Xml.Add(new XElement(ns + "ActualDuration", actualDurationString));
            itemTask.Xml.Add(new XElement(ns + "RemainingDuration", remainDurationString));

            itemTask.Xml.Add(new XElement(ns + "ActualStart", manualStart));
            itemTask.Xml.Element(ns + "Start").Value = manualStart;
            itemTask.Xml.Element(ns + "Finish").Value = manualFinish;

            var assCount = itemTask.Assignments.Count;
            var workersCount = assCount;
            if (assCount == 0)
            {
                logger.Debug("no assigments for {0}", itemTask.Name);
                // if no assignments, imagine that there is single worker
                workersCount = 1;
            }

            // everybody start and progress together, with same work
            // so total work in task = sum of work for every worker
            var totalWorkStr = PercentsOfTime(workTime, 100 * workersCount);
            var totalActWorkStr = PercentsOfTime(workTime, percent * workersCount);
            var totalRemWorkStr = PercentsOfTime(workTime, (100 - percent) * workersCount);

            // total work in xml incremented if workers > 1
            itemTask.Xml.Element(ns + "Work").Value = totalWorkStr;

            if (assCount == 0)
            {
                // else work is summed from assigments
                itemTask.Xml.Add(new XElement(ns + "ActualWork", totalActWorkStr));
                itemTask.Xml.Add(new XElement(ns + "RemainingWork", totalRemWorkStr));
            }

            var perAssMulti = 1;
            var workPerAssString = PercentsOfTime(workTime, 100 * perAssMulti);
            var actWorkPerAssString = PercentsOfTime(workTime, percent * perAssMulti);
            var remWorkPerAssString = PercentsOfTime(workTime, (100 - percent) * perAssMulti);
            if (assCount > 1)
            {
                logger.Debug("{0} assignments, each has {1} work", assCount, workPerAssString);
            }
            foreach (var ass in itemTask.Assignments)
            {
                ass.Xml.Add(new XElement(ns + "ActualStart", manualStart));
                ass.Xml.Add(new XElement(ns + "PercentWorkComplete", percent));

                ass.Xml.Element(ns + "ActualWork").Value = actWorkPerAssString;
                ass.Xml.Element(ns + "RemainingWork").Value = remWorkPerAssString;
                ass.Xml.Element(ns + "Work").Value = workPerAssString;
                ass.Xml.Element(ns + "RegularWork").Value = workPerAssString;
            }
        }

        private static string PercentsOfTime(TimeSpan time, int percent)
        {
            var t = new TimeSpan(time.Ticks * percent / 100);
            return TimeParser.ToString(t);
        }

        private class ItemsOfTask
        {
            public Task Task { get; set; }
            public IList<TwItem> Items { get; set; }
        }

        private class ItemListsOfTask
        {
            public Task Task { get; set; }
            public IList<TwItemList> Lists { get; set; }
        }

        private class TasksOfItemList
        {
            public TwItemList List { get; set; }

            /// <summary>
            /// candidates to match List
            /// </summary>
            public IList<Task> Tasks { get; set; }

            public override string ToString()
            {
                return string.Format("{0} : {1}", List, Tasks.Count == 1 ? Tasks[0].ToString() : Tasks.Count.ToString());
            }
        }

        private class TasksOfItem
        {
            public TwItem Item { get; set; }

            /// <summary>
            /// candidates to match Item
            /// </summary>
            public IList<Task> Tasks { get; set; }

            public override string ToString()
            {
                return string.Format("{0} : {1}", Item, Tasks.Count == 1 ? Tasks[0].ToString() : Tasks.Count.ToString());
            }
        }
    }
}