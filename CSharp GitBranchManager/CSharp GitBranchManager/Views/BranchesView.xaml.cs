using CSharp_GitBranchManager.Entities;
using CSharp_GitBranchManager.Models;
using CSharp_GitBranchManager.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace CSharp_GitBranchManager.Views
{
    /// <summary>
    /// Logique d'interaction pour BranchesView.xaml
    /// </summary>
    public partial class BranchesView : System.Windows.Controls.UserControl
    {
        private const int filterDelayMilliseconds = 300;

        private readonly DispatcherTimer filterTimer;
        public BranchType Type { get; set; }
        public BranchesViewModel ViewModel { get; private set; }

        public BranchesView()
        {
            InitializeComponent();

            ViewModel = (BranchesViewModel)DataContext;

            BranchesGrid.ItemsSource = ViewModel.Branches;
            SetupSorting(ViewModel.branchesView, BranchesGrid);

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

        private static void ExportBranches(string branchType, IEnumerable<string> branchNames)
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt",
                    DefaultExt = "txt",
                    FileName = $"Export {branchType} Names"
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = dialog.FileName;
                    File.WriteAllLines(filePath, branchNames);
                    MessageBox.Show("Branch names exported successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting branch names: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void SetupSorting(ListCollectionView view, DataGrid grid)
        {
            if (view != null && grid != null)
            {
                grid.Sorting += (sender, e) =>
                {
                    string propertyName = e.Column.SortMemberPath;
                    ListSortDirection sortDirection = e.Column.SortDirection != ListSortDirection.Ascending
                        ? ListSortDirection.Ascending
                        : ListSortDirection.Descending;

                    view.SortDescriptions.Clear();
                    view.SortDescriptions.Add(new SortDescription(propertyName, sortDirection));
                    e.Column.SortDirection = sortDirection;
                    e.Handled = true;
                };
            }
        }

        private static void ToggleSelectedRowsCheckState(DataGrid dataGrid)
        {
            if (dataGrid.SelectedItems.Count == 0) return;

            bool allSelected = true;
            bool anySelected = false;

            foreach (var selectedItem in dataGrid.SelectedItems)
            {
                if (selectedItem is BranchInfo branchInfo)
                {
                    if (!branchInfo.IsSelected)
                    {
                        allSelected = false;
                    }
                    anySelected = true;
                }
            }

            foreach (var selectedItem in dataGrid.SelectedItems)
            {
                if (selectedItem is BranchInfo branchInfo)
                {
                    branchInfo.IsSelected = !(allSelected && anySelected);
                }
            }
        }

        private void BranchesGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                ToggleSelectedRowsCheckState(BranchesGrid);
            }
        }

        private void DeleteSelectedBranches_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DeleteSelectedCommand.Execute(this);
        }

        private void ExportBranches_Click(object sender, RoutedEventArgs e)
        {
            ExportBranches("LocalBranches", ViewModel.Branches.Select(branch => branch.Name));
        }

        private void FilterTimer_Tick(object sender, EventArgs e)
        {
            filterTimer.Stop();
            //ApplyLocalBranchesFilter();
        }

        private void LoadBranches_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadCommand.Execute(this);
        }
    }
}
