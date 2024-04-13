using CSharp_GitBranchManager.Entities;
using CSharp_GitBranchManager.Models;
using CSharp_GitBranchManager.ViewModels;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace CSharp_GitBranchManager.Views
{
    /// <summary>
    /// Logique d'interaction pour BranchesView.xaml
    /// </summary>
    public partial class BranchesView : UserControl
    {
        public BranchType Type { get; set; }
        public BranchesViewModel ViewModel { get; private set; }

        public BranchesView()
        {
            InitializeComponent();

            ViewModel = (BranchesViewModel)DataContext;

            SetupSorting(ViewModel.BranchesView, BranchesGrid);
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
    }
}
