using CSharp_GitBranchManager.Entities;
using CSharp_GitBranchManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CSharp_GitBranchManager.Views
{
    /// <summary>
    /// Logique d'interaction pour BranchesView.xaml
    /// </summary>
    public partial class BranchesView : UserControl
    {
        private const int filterDelayMilliseconds = 300;

        private readonly DispatcherTimer filterTimer;
        public BranchType Type { get; set; }
        public BranchesViewModel ViewModel { get; private set; }

        public BranchesView()
        {
            InitializeComponent();

            ViewModel = new BranchesViewModel(Type);

            filterTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(filterDelayMilliseconds)
            };
            filterTimer.Tick += FilterTimer_Tick;

            FilterTextBox.TextChanged += (sender, e) =>
            {
                filterTimer.Stop();
                filterTimer.Start();
            };
        }

        private void BranchesGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                //ToggleSelectedRowsCheckState(LocalBranchesGrid);
            }
        }

        private void DeleteSelectedBranches_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DeleteSelectedCommand.Execute(this);
        }

        private void ExportBranches_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadCommand.Execute(this);
        }

        private void FilterTimer_Tick(object sender, EventArgs e)
        {
            filterTimer.Stop();
        }

        private void LoadBranches_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadCommand.Execute(this);
        }
    }
}
