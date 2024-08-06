using CSharp_GitBranchManager.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CSharp_GitBranchManager.Views
{
    public partial class BranchesView : UserControl
    {
        public BranchesView()
        {
            InitializeComponent();
            DataContext = ((MainViewModel)Application.Current.MainWindow.DataContext).LocalBranchesViewModel;
        }
    }
}
