using CSharp_GitBranchManager.Models;
using CSharp_GitBranchManager.Utils;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

namespace CSharp_GitBranchManager.ViewModels
{
    public class BranchesViewModel : ANotifyPropertyChanged
    {
        private readonly DispatcherTimer _filterTimer;
        private ObservableCollection<BranchInfo> _branches;
        private ObservableCollection<BranchInfo> _filteredBranches;
        private string _filterText;
        private int _progressValue;
        private string _statusText;

        #region Commands

        public ICommand DeleteCheckedCommand => new RelayCommand<object>(DeleteChecked);
        public ICommand ExportCommandCommand => new RelayCommand<object>(ExportCommand);
        public ICommand LoadGridCommand => new RelayCommand<object>(LoadGrid);

        #endregion Commands

        #region Properties

        public ObservableCollection<BranchInfo> Branches
        {
            get => _branches;
            set => SetField(ref _branches, value);
        }

        public ObservableCollection<BranchInfo> FilteredBranches
        {
            get => _filteredBranches;
            set => SetField(ref _filteredBranches, value);
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetField(ref _filterText, value))
                {
                    _filterTimer?.Stop();
                    _filterTimer?.Start();
                }
            }
        }

        public int ProgressValue
        {
            get => _progressValue;
            set => SetField(ref _progressValue, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetField(ref _statusText, value);
        }

        #endregion Properties

        public BranchesViewModel()
        {
            _filterTimer = new DispatcherTimer();
            _filterTimer.Interval = TimeSpan.FromMilliseconds(500);
            _filterTimer.Tick += FilterTimer_Tick;

            LoadData();
        }

        private void DeleteChecked(object obj)
        {
            throw new NotImplementedException();
        }

        private void ExportCommand(object obj)
        {
            throw new NotImplementedException();
        }

        private void FilterTimer_Tick(object sender, EventArgs e)
        {
            _filterTimer.Stop();
            UpdateFilteredBranches();
        }

        private void LoadData()
        {
            Branches = new ObservableCollection<BranchInfo>
            {
                new BranchInfo { Name = "Branch 1", IsChecked = false },
                new BranchInfo { Name = "Branch 2", IsChecked = true },
                new BranchInfo { Name = "Branch 3", IsChecked = false }
            };

            FilteredBranches = new ObservableCollection<BranchInfo>(Branches);
        }

        private void LoadGrid(object obj)
        {
            throw new NotImplementedException();
        }

        private void UpdateFilteredBranches()
        {
            if (string.IsNullOrWhiteSpace(FilterText))
            {
                FilteredBranches = new ObservableCollection<BranchInfo>(Branches);
            }
            else
            {
                FilteredBranches = new ObservableCollection<BranchInfo>(Branches.Where(b => b.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));
            }
        }
    }
}
