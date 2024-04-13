using CSharp_GitBranchManager.Entities;
using CSharp_GitBranchManager.Models;
using CSharp_GitBranchManager.Utils;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace CSharp_GitBranchManager.ViewModels
{
    public interface IBranchesViewModel
    {
        ObservableCollection<BranchInfo> Branches { get; set; }
        ListCollectionView BranchesView { get; }
        AppConfiguration Config { get; set; }
        string FilterText { get; set; }
        BranchType Type { get; set; }

        #region Commands

        ICommand DeleteSelectedCommand { get; }
        ICommand ExportCommand { get; }
        ICommand LoadCommand { get; }

        #endregion Commands
    }

    public class BranchesViewModel : ANotifyPropertyChanged, IBranchesViewModel
    {
        private const int filterDelayMilliseconds = 300;
        private readonly ListCollectionView _branchesView;
        private readonly DispatcherTimer filterTimer;
        private ObservableCollection<BranchInfo> _branches;
        private AppConfiguration _config;
        private string _filterText;
        private BranchType _type;

        #region Commands

        public ICommand DeleteSelectedCommand { get => new RelayCommand<string>(DeleteSelected); }
        public ICommand ExportCommand { get => new RelayCommand<string>(Export); }
        public ICommand LoadCommand { get => new RelayCommand<object>(async (param) => await LoadAsync()); }

        #endregion Commands

        #region Properties

        public ObservableCollection<BranchInfo> Branches
        {
            get => _branches;
            set => SetField(ref _branches, value);
        }

        public ListCollectionView BranchesView
        {
            get => _branchesView;
        }

        public AppConfiguration Config
        {
            get => _config;
            set => SetField(ref _config, value);
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetField(ref _filterText, value))
                {
                    filterTimer.Stop();
                    filterTimer.Start();
                }
            }
        }

        public BranchType Type
        {
            get => _type;
            set => SetField(ref _type, value);
        }

        #endregion Properties

        public BranchesViewModel()
        {
            _branches = new ObservableCollection<BranchInfo>();
            _branchesView = (ListCollectionView)CollectionViewSource.GetDefaultView(_branches);

            filterTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(filterDelayMilliseconds)
            };
            filterTimer.Tick += FilterTimer_Tick;
        }

        public BranchesViewModel(BranchType branchType, AppConfiguration config) : this()
        {
            _type = branchType;
            _config = config;
        }

        private static void ExportBranches(string branchType, IEnumerable<string> branchNames)
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt",
                    DefaultExt = "txt",
                    FileName = $"Export {branchType} branches names"
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

        private void ApplyFilter()
        {
            BranchesView.Filter = item =>
            {
                if (string.IsNullOrEmpty(FilterText))
                    return true;

                return item is BranchInfo branchInfo && branchInfo.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
            };
        }

        private bool CheckIsBranchMergedIntoMain(HashSet<Commit> mainCommits, Branch branch)
        {
            foreach (var commit in branch.Commits)
            {
                if (!mainCommits.Contains(commit)) return false;
            }
            return true;
        }

        private bool CheckIsRemote(Branch branch)
        {
            return Type switch
            {
                BranchType.Local => !branch.IsRemote,
                BranchType.Remote => branch.IsRemote,
                _ => throw new NotImplementedException(),
            };
        }

        private void DeleteSelected(object parameter)
        {
            var result = MessageBox.Show($"Are you sure you want to delete the {Branches.Count(b => b.IsSelected)} selected {Type.ToString().ToLower()} branches?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            using (var repo = new Repository(Config.GitRepositoryPath))
            {
                var branchesToRemove = Branches.Where(b => b.IsSelected).ToList();

                foreach (BranchInfo branchInfo in branchesToRemove)
                {
                    var branch = repo.Branches[branchInfo.Name];
                    if (branch != null && CheckIsRemote(branch))
                    {
                        repo.Branches.Remove(branch);
                        Application.Current.Dispatcher.Invoke(() => Branches.Remove(branchInfo));
                    }
                }
            }
        }

        private void Export(object parameter)
        {
            ExportBranches(Type.ToString(), Branches.Select(branch => branch.Name));
        }

        private void FilterTimer_Tick(object sender, EventArgs e)
        {
            filterTimer.Stop();
            ApplyFilter();
        }

        private HashSet<Commit> GetMainCommits(Repository repo)
        {
            var mainBranch = repo.Branches[Config.RemoteMergedBranch];
            if (mainBranch == null)
                throw new InvalidOperationException($"{Config.RemoteMergedBranch} branch not found.");

            return new HashSet<Commit>(mainBranch.Commits);
        }

        private async Task LoadAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var repo = new Repository(Config.GitRepositoryPath))
                    {
                        _branches.Clear();

                        var mainCommits = Type == BranchType.Remote ? GetMainCommits(repo) : new HashSet<Commit>();
                        var excludedBranches = Type == BranchType.Remote ? Config.GetListRemoteExcludedBranches() : new List<string>();

                        var branches = repo.Branches
                            .Where(branch =>
                                CheckIsRemote(branch)
                                && !excludedBranches.Contains(branch.FriendlyName))
                            .ToList();

                        DateTime currentDate = DateTime.Now;
                        var checkMaxAge = Type == BranchType.Local ? Config.LocalMaxAge : Config.RemoteMaxAge;
                        var maxAgeMonths = Type == BranchType.Local ? Config.LocalMaxAgeMonths : Config.RemoteMaxAgeMonths;
                        var checkUnused = Type == BranchType.Local && Config.LocalUnused;
                        var checkMerged = Type == BranchType.Remote && Config.RemoteMerged;

                        foreach (var branch in branches)
                        {
                            TimeSpan age = currentDate - branch.Tip.Author.When.LocalDateTime;
                            if (checkMaxAge && ((age.TotalDays / 30) < maxAgeMonths)) continue;
                            if (checkUnused && repo.Branches.Any(b => b.IsRemote && b.FriendlyName.Contains(branch.FriendlyName))) continue;
                            if (checkMerged && !CheckIsBranchMergedIntoMain(mainCommits, branch)) continue;

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Branches.Add(new BranchInfo
                                {
                                    Name = branch.FriendlyName,
                                    LastCommitBy = branch.Tip.Author.Name,
                                    LastCommitDate = branch.Tip.Author.When.LocalDateTime.ToString()
                                });
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading branches: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }
    }
}
