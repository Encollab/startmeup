using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NLog;

namespace TeamworkMsprojectExportTweaker
{
    internal class ApiWrapper
    {
        static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        int maxTrys = 3;
        TeamWorkClient client;
        NetworkCredential cred;
        List<TwProject> _projectsList;
        int secsPerTry;

        public ApiWrapper(TeamWorkClient client)
        {
            this.client = client;
            this.secsPerTry = (client.Slow ? 60 : 15);
            this.cred = new NetworkCredential(client.ApiKey, "X");
        }

        public List<TwProject> ProjectsList
        {
            get
            {
                return _projectsList ?? (_projectsList = FetchProjectsList());
            }
        }

        public List<TwProject> FetchProjectsList()
        {
            logger.Debug("in {0}", "FetchProjectsList");
            return FetchWithRetry(FetchProjectsListInner, -1);
        }

        public List<TwItem> FetchProjectTasks(int id)
        {
            logger.Debug("in {0} {1}", "FetchProjectTasks", id);
            return FetchWithRetry(FetchProjectTasksInner, id);
        }

        public List<TwItemList> FetchProjectTaskLists(int id)
        {
            logger.Debug("in {0} {1}", "FetchProjectTaskLists", id);
            return FetchWithRetry(FetchProjectTaskListsInner, id);
        }

        public List<TwItem> FetchTaskListTasks(int id)
        {
            logger.Debug("in {0} {1}", "FetchTaskListTasks", id);
            return FetchWithRetry(FetchTaskListTasksInner, id);
        }

        private List<TwProject> FetchProjectsListInner(WebClient wc, int id = 0)
        {
            var xml =
#if DEBUG
 File.ReadAllText(@"projects.xml");
#else
 wc.DownloadString(GetProjectsUrl());
#endif

            return xml.ParseProjects();
        }

        private List<TwItem> FetchProjectTasksInner(WebClient wc, int id)
        {
            var xml =
#if DEBUG
 File.ReadAllText(@"tasks.xml");
#else
 wc.DownloadString(GetTasksOfProjectUrl(id));
#endif
            return xml.ParseItems();
        }

        private List<TwItemList> FetchProjectTaskListsInner(WebClient wc, int id)
        {
            var xml = wc.DownloadString(GetTaskListsOfProject(id));
            return xml.ParseLists();
        }

        private List<TwItem> FetchTaskListTasksInner(WebClient wc, int id)
        {
            var xml = wc.DownloadString(GetTasksOfTaskListUrl(id));
            return xml.ParseItems();
        }

        private List<T> FetchWithRetry<T>(Func<WebClient, int, List<T>> f, int id)
        {
            for (int i = 1; i <= maxTrys; i++)
            {
                using (var wc = new MyWebClient(MsToWait(i)))
                {
                    wc.Credentials = cred;
                    try
                    {
                        return f(wc, id);
                    }
                    catch (WebException ex)
                    {
                        logger.Error(ex);
                    }
                }
            }
            throw new RetryLimitException(maxTrys.ToString());
        }

        int MsToWait(int attempt)
        {
            return attempt * secsPerTry * 1000;
        }

        private string GetProjectsUrl()
        {
            return string.Format("{0}projects.xml", client.BaseUrl);
        }

        private string GetTasksOfProjectUrl(int projectId)
        {
            return string.Format("{0}projects/{1}/tasks.xml", client.BaseUrl, projectId);
        }

        private string GetTasksOfTaskListUrl(int taskListId)
        {
            return string.Format("{0}tasklists/{1}/tasks.xml", client.BaseUrl, taskListId);
        }

        private string GetTaskListsOfProject(int projectId)
        {
            return string.Format("{0}projects/{1}/tasklists.xml", client.BaseUrl, projectId);
        }

        private class MyWebClient : WebClient
        {
            static readonly ILogger logger = LogManager.GetCurrentClassLogger();
            int timeout;
            public MyWebClient(int ms)
            {
                timeout = ms;
            }
            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                IWebProxy proxy = w.Proxy;
                if (proxy != null)
                {
                    logger.Debug("Proxy: {0}", proxy.GetProxy(w.RequestUri));
                }

                w.Timeout = timeout;
                return w;
            }
        }
    }
}