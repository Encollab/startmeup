using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace TeamworkMsprojectExportTweaker
{
    public class FixerViewModel : ViewModelBase
    {
        static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public FixerViewModel()
        {
            TeamWorkClient.Created += (s, e) =>
            {
                api = new ApiWrapper(TeamWorkClient.Current);
            };
#if DEBUG

            // TeamWorkClient.Make(Settings.Default.ApiKey);
            // DoCommand.Execute(null);
#endif
        }

        private string filePath;

        public string FilePath
        {
            get { return filePath; }
            set
            {
                filePath = value;
                OnPropertyChanged(() => FilePath);
            }
        }

        private string result;
        private ApiWrapper api;

        public string Result
        {
            get { return result; }
            set
            {
                result = value;
                OnPropertyChanged(() => Result);
            }
        }

        private bool working;

        public bool IsWorking
        {
            get { return working; }
            set
            {
                working = value;
                OnPropertyChanged(() => IsWorking);
            }
        }

        public RelayCommand DoCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
#if DEBUG
                        FilePath = @"First Project_not_working.xml";
#endif
                    IsWorking = true;
                    logger.Info("====\nbegin fixing {0}", FilePath);
                    Result = "Loading data...";

                    string title;
                    Fixer fixer;
                    LoadBadFile(out title, out fixer);
                    if (fixer == null)
                    {
                        IsWorking = false;
                        return;
                    }
                    DoFixing(title, fixer);
                },
                () => api != null && !string.IsNullOrEmpty(FilePath) && !IsWorking);
            }
        }

        private void LoadBadFile(out string title, out Fixer fixer)
        {
            fixer = null;
            title = null;
            try
            {
                var badXml = File.ReadAllText(FilePath);
                var badMsProject = badXml.ParseProject();
                fixer = new Fixer(badMsProject);
                title = badMsProject.Title;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Result = string.Format("Error: {0}\nSee log file for details.", ex.Message);
            }
        }

        private void DoFixing(string title, Fixer fixer)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += (s, e) =>
            {
                if (e.Result is Exception)
                {
                    var ex = e.Result as Exception;
                    logger.Error(ex);
                    if (ex is RetryLimitException)
                    {
                        Result = "Retry Limit: " + ex.Message.ToString();
                    }
                    else
                    {
                        Result = string.Format("Error: {0}\nSee log file for details.", ex.Message);
                    }
                    IsWorking = false;
                    return;
                }
                var project = e.Result as TwProject;
                if (project == null)
                {
                    logger.Warn("project '{0}' not found", title);
                    Result = string.Format("project '{0}' not found on server", title);
                }
                else
                {
                    var result = fixer.Fix(project);
                    var filename = FileManager.GetResultFilename(FilePath);
                    result.Save(filename);
                    Result = "Saved to " + filename;
                }
                IsWorking = false;
            };
            bw.RunWorkerAsync(title);
        }
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var title = e.Argument as string;
            try
            {
                e.Result = GetTwProject(title);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private TwProject GetTwProject(string title)
        {
            // load projects
            var project = api.ProjectsList.FirstOrDefault(x => x.Name == title);
            if (project == null)
            {
                return null;
            }

            // load project task lists
            // load lists' tasks
            project.Lists = api.FetchProjectTaskLists(project.Id);
            Parallel.ForEach(project.Lists, (list) =>
            {
                list.Items = api.FetchTaskListTasks(list.Id);
                foreach (var item in list.Items)
                {
                    item.Childs = list.Items.Where(x => x.ParentId == item.Id).ToList();
                }
            });
            return project;
        }
    }
}