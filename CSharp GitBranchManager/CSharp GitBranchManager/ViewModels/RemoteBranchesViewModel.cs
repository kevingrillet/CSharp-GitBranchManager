using CSharp_GitBranchManager.Models;
using CSharp_GitBranchManager.Utils;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CSharp_GitBranchManager.ViewModels
{
    public class RemoteBranchesViewModel : ABranchesViewModel
    {
        public RemoteBranchesViewModel(AppConfiguration appConfiguration) : base(appConfiguration)
        {
        }

        protected override void DeleteChecked()
        {
            var result = MessageBox.Show($"Are you sure you want to delete the {_branches.Count(b => b.IsChecked)} checked remote branches?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            using (var repo = new Repository(_configuration.GitRepositoryPath))
            {
                var branchesToRemove = _branches.Where(b => b.IsChecked).ToList();

                foreach (BranchInfo branchInfo in branchesToRemove)
                {
                    var branch = repo.Branches[branchInfo.Name];
                    if (branch != null && branch.IsRemote)
                    {
                        repo.Branches.Remove(branch);
                        _branches.Remove(branchInfo);

                        var remote = repo.Network.Remotes[branch.RemoteName];
                        var pushRefSpec = $":refs/heads/{branch.FriendlyName}";

                        repo.Network.Push(remote, pushRefSpec, new PushOptions());
                    }
                }
            }
        }

        protected override void ExportCommand()
        {
            ExportBranches("RemoteBranches", _branches.Select(branch => branch.Name));
        }

        protected override async void LoadDataAsync()
        {
            using (var repo = new Repository(_configuration.GitRepositoryPath))
            {
                _branches.Clear();

                var mainBranch = repo.Branches[_configuration.RemoteMergedBranch];
                if (mainBranch == null)
                {
                    MessageBox.Show($"{_configuration.RemoteMergedBranch} branch not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var mainCommits = new HashSet<Commit>(mainBranch.Commits);

                var remoteExcludedBranches = _configuration.RemoteExcludedBranchesCSV.CSVSplitAndTrim();

                var branches = repo.Branches
                    .Where(b => b.IsRemote && b != mainBranch
                        && !remoteExcludedBranches.Contains(b.FriendlyName))
                    .ToList();

                DateTime currentDate = DateTime.Now;

                await Task.Run(() =>
                {
                    foreach (var branch in branches)
                    {
                        TimeSpan age = currentDate - branch.Tip.Author.When.LocalDateTime;

                        if (_configuration.RemoteMaxAge && (age.TotalDays / 30) < _configuration.RemoteMaxAgeMonths) continue;
                        if (_configuration.RemoteMerged && !IsBranchMergedIntoMain(mainCommits, branch)) continue;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _branches.Add(new BranchInfo
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

        private static bool IsBranchMergedIntoMain(HashSet<Commit> mainCommits, Branch branch)
        {
            foreach (var commit in branch.Commits)
            {
                if (!mainCommits.Contains(commit)) return false;
            }
            return true;
        }
    }
}
