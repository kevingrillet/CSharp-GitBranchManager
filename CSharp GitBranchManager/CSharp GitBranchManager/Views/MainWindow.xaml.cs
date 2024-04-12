using CSharp_GitBranchManager.ViewModels;
using System.Windows;

namespace CSharp_GitBranchManager.Views
{
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            DataContext = ViewModel;
        }
    }
}
