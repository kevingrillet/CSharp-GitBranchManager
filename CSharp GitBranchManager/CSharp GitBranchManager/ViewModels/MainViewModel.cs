using CSharp_GitBranchManager.Entities;
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
            ConfigurationViewModel = new AppConfigurationViewModel();
            LocalBranchesViewModel = new BranchesViewModel(BranchType.Local/*, ConfigurationViewModel.Configuration*/);
            RemoteBranchesViewModel = new BranchesViewModel(BranchType.Remote/*, ConfigurationViewModel.Configuration*/);
        }
    }
}
