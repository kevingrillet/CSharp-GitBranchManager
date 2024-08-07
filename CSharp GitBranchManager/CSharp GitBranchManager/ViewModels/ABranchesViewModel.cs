using CSharp_GitBranchManager.Models;
using CSharp_GitBranchManager.Utils;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

namespace CSharp_GitBranchManager.ViewModels
{
    public abstract class ABranchesViewModel : ANotifyPropertyChanged
    {
        private readonly AppConfiguration _configuration;
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

        protected ABranchesViewModel(AppConfiguration appConfiguration)
        {
            _configuration = appConfiguration;
            _filterTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _filterTimer.Tick += FilterTimer_Tick;

            LoadData();
        }

        protected abstract void DeleteChecked(object obj);

        protected abstract void ExportCommand(object obj);

        protected abstract void LoadData();

        protected abstract void LoadGrid(object obj);

        private void FilterTimer_Tick(object sender, EventArgs e)
        {
            _filterTimer.Stop();
            UpdateFilteredBranches();
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
