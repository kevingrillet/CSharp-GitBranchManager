using CSharp_GitBranchManager.Model;
using CSharp_GitBranchManager.Utils;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace CSharp_GitBranchManager.ViewModel
{
    /// <summary>
    /// TODO:
    /// - Test
    /// - Filter working?
    /// - Config
    /// - Select
    /// - Toogle
    /// - Sort
    /// - Export
    /// </summary>

    public enum BranchType
    {
        Local = 0,
        Remote = 1
    }

    public class BranchesViewModel : ANotifyPropertyChanged
    {
        public readonly ListCollectionView branchesView;
        private ObservableCollection<BranchInfo> _branches;
        private string _filterText;

        public ObservableCollection<BranchInfo> Branches
        {
            get { return _branches; }
            set
            {
                _branches = value;
                NotifyPropertyChanged();
            }
        }

        public AppConfig Config { get; set; }

        public ICommand DeleteSelectedCommand { get; set; }

        public string FilterText
        {
            get { return _filterText; }
            set
            {
                _filterText = value;
                ApplyFilter();
            }
        }

        public ICommand LoadCommand { get; set; }

        public BranchType Type { get; set; }

        public BranchesViewModel(BranchType brancheType)
        {
            Type = brancheType;
            Branches = new ObservableCollection<BranchInfo>();
            DeleteSelectedCommand = new RelayCommand(DeleteSelected);
            LoadCommand = new RelayCommand(Load);
        }

        private static bool CheckIsBranchMergedIntoMain(HashSet<Commit> mainCommits, Branch branch)
        {
            foreach (var commit in branch.Commits)
            {
                if (!mainCommits.Contains(commit)) return false;
            }
            return true;
        }

        private void ApplyFilter()
        {
            branchesView.Filter = item =>
            {
                if (string.IsNullOrEmpty(FilterText))
                    return true;

                return item is BranchInfo branchInfo && branchInfo.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
            };
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
            var result = MessageBox.Show($"Are you sure you want to delete the {Branches.Count(b => b.IsSelected)} selected ${Type.ToString().ToLower()} branches?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            using (var repo = new Repository(Config.RepositoryPath))
            {
                var branchesToRemove = Branches.Where(b => b.IsSelected).ToList();

                foreach (BranchInfo branchInfo in branchesToRemove)
                {
                    var branch = repo.Branches[branchInfo.Name];
                    if (branch != null && CheckIsRemote(branch))
                    {
                        repo.Branches.Remove(branch);
                        Branches.Remove(branchInfo);
                    }
                }
            }
        }

        private HashSet<Commit> GetMainCommits(Repository repo)
        {
            var mainBranch = repo.Branches[Config.RemoteMergedBranchValue];
            if (mainBranch == null)
            {
                MessageBox.Show($"{Config.RemoteMergedBranchValue} branch not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return [];
            }
            return new HashSet<Commit>(mainBranch.Commits);
        }

        private async void Load(object parameter)
        {
            using (var repo = new Repository(Config.RepositoryPath))
            {
                Branches.Clear();

                var mainCommits = Type == BranchType.Remote ? GetMainCommits(repo) : [];
                var excludedBranches = Type == BranchType.Remote ? Config.GetListRemoteExcludedBranches() : [];

                var branches = repo.Branches
                    .Where(branch =>
                        CheckIsRemote(branch)
                        && !excludedBranches.Contains(branch.FriendlyName))
                    .ToList();

                DateTime currentDate = DateTime.Now;
                var checkMaxAge = Type == BranchType.Local ? Config.LocalMaxAgeCheckBox : Config.RemoteMaxAgeCheckBox;
                var maxAgeMonths = Type == BranchType.Local ? Config.LocalMaxAgeMonths : Config.RemoteMaxAgeMonths;
                var checkUnused = Type == BranchType.Local && Config.LocalUnusedCheckBox;
                var checkMerged = Type == BranchType.Remote && Config.RemoteMergedCheckBox;

                await Task.Run(() =>
                {
                    foreach (var branch in branches)
                    {
                        TimeSpan age = currentDate - branch.Tip.Author.When.LocalDateTime;
                        if (checkMaxAge && ((age.TotalDays / 30) < maxAgeMonths)) continue;
                        if (checkUnused && repo.Branches.Any(b => b.IsRemote && b.FriendlyName.Contains(branch.FriendlyName))) continue;
                        if (checkMerged && !CheckIsBranchMergedIntoMain(mainCommits, branch)) continue;

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            Branches.Add(new BranchInfo
                            {
                                Name = branch.FriendlyName,
                                LastCommitBy = branch.Tip.Author.Name,
                                LastCommitDate = branch.Tip.Author.When.LocalDateTime.ToString()
                            });
                        });
                    }
                });
            }
        }
    }
}
