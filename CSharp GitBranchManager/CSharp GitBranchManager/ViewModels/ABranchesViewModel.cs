using CSharp_GitBranchManager.Models;
using CSharp_GitBranchManager.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;

namespace CSharp_GitBranchManager.ViewModels
{
    public abstract class ABranchesViewModel : ANotifyPropertyChanged
    {
        protected readonly AppConfiguration _configuration;
        protected ObservableCollection<BranchInfo> _branches;
        private readonly DispatcherTimer _filterTimer;
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
        }

        protected static void ExportBranches(string branchType, IEnumerable<string> branchNames)
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt",
                    DefaultExt = "txt",
                    FileName = $"Export {branchType} Names"
                };

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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

        protected abstract void DeleteChecked(object obj);

        protected abstract void ExportCommand(object obj);

        protected abstract void LoadDataAsync();

        protected virtual void LoadGrid(object obj)
        {
            LoadDataAsync();
        }

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
