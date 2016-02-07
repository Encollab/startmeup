using System;
using System.Linq;
using System.Net;
using NLog;
using TeamworkMsprojectExportTweaker.Properties;

namespace TeamworkMsprojectExportTweaker
{
    public class LoginViewModel : ViewModelBase
    {
        static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public LoginViewModel()
        {
            ApiKey = Settings.Default.ApiKey;
            ApiUrl = Settings.Default.ApiUrl;
            Slow = Settings.Default.Slow;
            LoginText = "Log in";
        }

        private string apikey;

        public string ApiKey
        {
            get { return apikey; }
            set
            {
                apikey = value;
                OnPropertyChanged(() => ApiKey);
                WrongApiKey = false;
            }
        }

        private string apiurl;

        public string ApiUrl
        {
            get { return apiurl; }
            set
            {
                apiurl = value;
                OnPropertyChanged(() => ApiUrl);
                WrongApiUrl = false;
            }
        }

        private bool logged;

        public bool Logged
        {
            get { return logged; }
            set
            {
                logged = value;
                OnPropertyChanged(() => Logged);
                OnPropertyChanged(() => NotLogged);
            }
        }

        public bool NotLogged
        {
            get { return !logged; }
        }

        private bool wrongApiKey;

        public bool WrongApiKey
        {
            get { return wrongApiKey; }
            set
            {
                wrongApiKey = value;
                OnPropertyChanged(() => WrongApiKey);
            }
        }

        private bool wrongApiUrl;

        public bool WrongApiUrl
        {
            get { return wrongApiUrl; }
            set
            {
                wrongApiUrl = value;
                OnPropertyChanged(() => WrongApiUrl);
            }
        }


        private bool slow;

        public bool Slow
        {
            get { return slow; }
            set
            {
                slow = value;
                OnPropertyChanged(() => Slow);
            }
        }

        private int attempt;

        public int Attempt
        {
            get { return attempt; }
            set
            {
                attempt = value;
                OnPropertyChanged(() => Attempt);
            }
        }

        private string loginText;

        public string LoginText
        {
            get { return loginText; }
            set
            {
                loginText = value;
                OnPropertyChanged(() => LoginText);
            }
        }

        public RelayCommand LoginCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    // prefer url
                    var byKey = string.IsNullOrEmpty(ApiUrl);
                    var client = TeamWorkClient.Make(ApiKey, Slow, ApiUrl);

                    logger.Info("logging with apikey '{0}**', url '{1}', slow {2}", ApiKey[0], ApiUrl, Slow);

                    for (int i = 10 - 1; i >= 0; i--)
                    {
                        try
                        {
                            Logged = client.TryLogin();
                            break;
                        }
                        catch (WebException ex)
                        {
                            logger.Error(ex);
                            var webRes = ex.Response as HttpWebResponse;
                            if (webRes != null)
                            {
                                if (webRes.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    WrongApiKey = true;
                                    break;
                                }
                            }
                            Logged = false;
                        }

                        logger.Info("logging: wrong = {0}, attempt = {1}", WrongApiKey, i);
                        LoginText = string.Format("Log in ({0})", i);
                    }

                    Settings.Default.ApiKey = ApiKey;
                    Settings.Default.ApiUrl = ApiUrl;
                    Settings.Default.Slow = Slow;
                    Settings.Default.Save();
                },
                () => !string.IsNullOrEmpty(ApiKey));
            }
        }
    }
}