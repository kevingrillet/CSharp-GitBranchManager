using CSharp_GitBranchManager.Models;
using LibGit2Sharp;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CSharp_GitBranchManager.ViewModels
{
    public class LocalBranchesViewModel : ABranchesViewModel
    {
        public LocalBranchesViewModel(AppConfiguration appConfiguration) : base(appConfiguration)
        {
        }

        protected override void DeleteChecked()
        {
            var result = MessageBox.Show($"Are you sure you want to delete the {_branches.Count(b => b.IsChecked)} checked local branches?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            using (var repo = new Repository(_configuration.GitRepositoryPath))
            {
                var branchesToRemove = _branches.Where(b => b.IsChecked).ToList();

                foreach (BranchInfo branchInfo in branchesToRemove)
                {
                    var branch = repo.Branches[branchInfo.Name];
                    if (branch != null && !branch.IsRemote)
                    {
                        repo.Branches.Remove(branch);
                        _branches.Remove(branchInfo);
                    }
                }
            }
        }

        protected override void ExportCommand()
        {
            ExportBranches("LocalBranches", _branches.Select(branch => branch.Name));
        }

        protected override async void LoadDataAsync()
        {
            using (var repo = new Repository(_configuration.GitRepositoryPath))
            {
                _branches.Clear();

                var branches = repo.Branches
                    .Where(b => !b.IsRemote)
                    .ToList();

                DateTime currentDate = DateTime.Now;

                await Task.Run(() =>
                {
                    foreach (var branch in branches)
                    {
                        TimeSpan age = currentDate - branch.Tip.Author.When.LocalDateTime;

                        if (_configuration.LocalMaxAge && ((age.TotalDays / 30) < _configuration.LocalMaxAgeMonths)) continue;

                        if (_configuration.LocalUnused && repo.Branches.Any(b => b.IsRemote && b.FriendlyName.Contains(branch.FriendlyName))) continue;

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
    }
}
