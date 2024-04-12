using CSharp_GitBranchManager.Entities;
using CSharp_GitBranchManager.Models;
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
using MessageBox = System.Windows.MessageBox;

namespace CSharp_GitBranchManager.ViewModels
{
    public class BranchesViewModel : ANotifyPropertyChanged
    {
        private ObservableCollection<BranchInfo> _branches;

        #region Commands

        public ICommand ApplyFilterCommand { get => new RelayCommand(ApplyFilter); }
        public ICommand DeleteSelectedCommand { get => new RelayCommand(DeleteSelected); }
        public ICommand LoadCommand { get => new RelayCommand(Load); }

        #endregion Commands

        #region Properties

        public readonly ListCollectionView branchesView;

        public ObservableCollection<BranchInfo> Branches
        {
            get { return _branches; }
            set
            {
                _branches = value;
                NotifyPropertyChanged();
            }
        }

        public AppConfiguration Config { get; set; }
        public BranchType Type { get; set; }

        #endregion Properties

        public BranchesViewModel()
        {
            Branches = new ObservableCollection<BranchInfo>();
            branchesView = (ListCollectionView)CollectionViewSource.GetDefaultView(Branches);
        }

        public BranchesViewModel(BranchType branchType) : this()
        {
            Type = branchType;
        }

        public BranchesViewModel(BranchType branchType, AppConfiguration config) : this(branchType)
        {
            Config = config;
        }

        private static bool CheckIsBranchMergedIntoMain(HashSet<Commit> mainCommits, Branch branch)
        {
            foreach (var commit in branch.Commits)
            {
                if (!mainCommits.Contains(commit)) return false;
            }
            return true;
        }

        private void ApplyFilter(object parameter)
        {
            var filter = parameter as string;
            branchesView.Filter = item =>
            {
                if (string.IsNullOrEmpty(filter))
                    return true;

                return item is BranchInfo branchInfo && branchInfo.Name.Contains(filter, StringComparison.OrdinalIgnoreCase);
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

            using (var repo = new Repository(Config.GitRepositoryPath))
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
            var mainBranch = repo.Branches[Config.RemoteMergedBranch];
            if (mainBranch == null)
            {
                MessageBox.Show($"{Config.RemoteMergedBranch} branch not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return [];
            }
            return new HashSet<Commit>(mainBranch.Commits);
        }

        private async void Load(object parameter)
        {
            using (var repo = new Repository(Config.GitRepositoryPath))
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
                var checkMaxAge = Type == BranchType.Local ? Config.LocalMaxAge : Config.RemoteMaxAge;
                var maxAgeMonths = Type == BranchType.Local ? Config.LocalMaxAgeMonths : Config.RemoteMaxAgeMonths;
                var checkUnused = Type == BranchType.Local && Config.LocalUnused;
                var checkMerged = Type == BranchType.Remote && Config.RemoteMerged;

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
