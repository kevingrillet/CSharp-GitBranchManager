using CSharp_GitBranchManager.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CSharp_GitBranchManager.Views
{
    public partial class ConfigurationView : UserControl
    {
        public ConfigurationView()
        {
            InitializeComponent();
            DataContext = ((MainViewModel)Application.Current.MainWindow.DataContext).ConfigurationViewModel;
        }
    }
}
