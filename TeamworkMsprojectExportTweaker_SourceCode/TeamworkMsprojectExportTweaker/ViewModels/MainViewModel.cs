using System;
using System.Linq;

namespace TeamworkMsprojectExportTweaker
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            LoginVm = new LoginViewModel();
            FixerVm = new FixerViewModel();
        }

        public LoginViewModel LoginVm { get; set; }
        public FixerViewModel FixerVm { get; set; }
    }
}