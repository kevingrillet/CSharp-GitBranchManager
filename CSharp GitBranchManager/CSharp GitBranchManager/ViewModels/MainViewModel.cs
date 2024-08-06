using CSharp_GitBranchManager.Models;
using CSharp_GitBranchManager.Utils;

namespace CSharp_GitBranchManager.ViewModels
{
    public class MainViewModel : ANotifyPropertyChanged
    {
        public AppConfigurationViewModel ConfigurationViewModel { get; private set; }
        public BranchesViewModel LocalBranchesViewModel { get; private set; }
        public BranchesViewModel RemoteBranchesViewModel { get; private set; }

        public MainViewModel()
        {
            var appConfiguration = new AppConfiguration();
            ConfigurationViewModel = new AppConfigurationViewModel(appConfiguration);
            LocalBranchesViewModel = new BranchesViewModel(appConfiguration);
            RemoteBranchesViewModel = new BranchesViewModel(appConfiguration);
        }
    }
}
