using System;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using NLog;

namespace TeamworkMsprojectExportTweaker
{
    public class TeamWorkClient
    {
        static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        static TeamWorkClient instance;

        public static TeamWorkClient Make(string apiKey, bool slow, string baseUrl = null)
        {
            instance = new TeamWorkClient();
            if (!string.IsNullOrEmpty(baseUrl))
                instance.BaseUrl = new UriBuilder(baseUrl).Uri.ToString();
            instance.ApiKey = apiKey;
            instance.Slow = slow;
            OnCreated(EventArgs.Empty);
            return instance;
        }


        public static TeamWorkClient Current { get { return instance; } }

        private TeamWorkClient()
        {
        }

        public static event EventHandler Created;

        private static void OnCreated(EventArgs e)
        {
            var h = Created;
            if (h != null)
                h.Invoke(typeof(TeamWorkClient), e);
        }

        public string BaseUrl { get; private set; }

        public string ApiKey { get; private set; }

        public bool Slow { get; private set; }

        public bool TryLogin()
        {
            if (BaseUrl == null)
            {
                using (var wc = new WebClient())
                {
                    wc.Credentials = new NetworkCredential(ApiKey, "X");
                    string xml;

                    xml = wc.DownloadString("http://authenticate.teamworkpm.net/authenticate.xml");

                    var doc = XDocument.Parse(xml);
                    var url = doc.Descendants("url").FirstOrDefault();
                    if (url != null)
                        BaseUrl = url.Value;
                    return BaseUrl != null;
                }
            }
            return true;
        }
    }
}