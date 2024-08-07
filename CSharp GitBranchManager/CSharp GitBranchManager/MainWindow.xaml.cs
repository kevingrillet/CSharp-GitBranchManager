using CSharp_GitBranchManager.Models;
using CSharp_GitBranchManager.ViewModels;
using CSharp_GitBranchManager.Views;
using System.Windows;

namespace CSharp_GitBranchManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var configuration = new Configuration();

            LocalBranchesView.DataContext = new LocalBranchesViewModel(configuration);
            RemoteBranchesView.DataContext = new RemoteBranchesViewModel(configuration);
            ConfigurationView.DataContext = new ConfigurationViewModel(configuration);
        }
    }
}
