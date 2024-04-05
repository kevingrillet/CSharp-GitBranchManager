using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace CSharp_GitBranchManager
{
    public partial class MainWindow : Window
    {
        private const string ConfigFilePath = "config.json";
        private readonly ListCollectionView localBranchesView;
        private readonly ListCollectionView remoteBranchesView;
        private bool skipUpdateStatusBar = false;
        public ObservableCollection<BranchInfo> LocalBranches { get; set; }
        public ObservableCollection<BranchInfo> RemoteBranches { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            LoadConfig();
            MaxRemoteAgeTextBox.PreviewTextInput += MaxRemoteAgeTextBox_PreviewTextInput;

            LocalBranches = [];
            LocalBranchesGrid.ItemsSource = LocalBranches;
            LocalBranches.CollectionChanged += (sender, e) => UpdateStatusBar();
            localBranchesView = (ListCollectionView)CollectionViewSource.GetDefaultView(LocalBranches);
            SetupSorting(localBranchesView, LocalBranchesGrid);

            LocalFilterTextBox.TextChanged += (sender, e) =>
            {
                localBranchesView.Filter = item =>
                {
                    if (string.IsNullOrEmpty(LocalFilterTextBox.Text))
                        return true;

                    return item is BranchInfo branchInfo && branchInfo.Name.Contains(LocalFilterTextBox.Text, StringComparison.OrdinalIgnoreCase);
                };
                UpdateStatusBar();
            };

            RemoteBranches = [];
            RemoteBranchesGrid.ItemsSource = RemoteBranches;
            RemoteBranches.CollectionChanged += (sender, e) => UpdateStatusBar();
            remoteBranchesView = (ListCollectionView)CollectionViewSource.GetDefaultView(RemoteBranches);
            SetupSorting(remoteBranchesView, RemoteBranchesGrid);

            RemoteFilterTextBox.TextChanged += (sender, e) =>
            {
                remoteBranchesView.Filter = item =>
                {
                    if (string.IsNullOrEmpty(RemoteFilterTextBox.Text))
                        return true;

                    return item is BranchInfo branchInfo && branchInfo.Name.Contains(RemoteFilterTextBox.Text, StringComparison.OrdinalIgnoreCase);
                };
                UpdateStatusBar();
            };

            LocalBranches.CollectionChanged += (sender, e) => UpdateStatusBar();
            RemoteBranches.CollectionChanged += (sender, e) => UpdateStatusBar();
            MainTabControl.SelectionChanged += (sender, e) => UpdateStatusBar();
        }

        private static bool IsBranchMergedIntoMain(HashSet<Commit> mainCommits, Branch branch)
        {
            foreach (var commit in branch.Commits)
            {
                if (!mainCommits.Contains(commit)) return false;
            }
            return true;
        }

        private static void SetupSorting(ListCollectionView view, DataGrid grid)
        {
            if (view != null && grid != null)
            {
                grid.Sorting += (sender, e) =>
                {
                    string propertyName = e.Column.SortMemberPath;
                    ListSortDirection sortDirection = (e.Column.SortDirection != ListSortDirection.Ascending)
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

        private void DeleteSelectedLocalBranches_Click(object sender, RoutedEventArgs e)
        {
            using (var repo = new Repository(GitRepoPathTextBox.Text))
            {
                var branchesToRemove = LocalBranches.Where(b => b.IsSelected).ToList();

                foreach (BranchInfo branchInfo in branchesToRemove)
                {
                    if (branchInfo.IsSelected)
                    {
                        var branch = repo.Branches[branchInfo.Name];
                        if (branch != null && !branch.IsRemote)
                        {
                            repo.Branches.Remove(branch);
                            LocalBranches.Remove(branchInfo);
                        }
                    }
                }
            }
        }

        private void DeleteSelectedRemoteBranches_Click(object sender, RoutedEventArgs e)
        {
            using (var repo = new Repository(GitRepoPathTextBox.Text))
            {
                var branchesToRemove = RemoteBranches.Where(b => b.IsSelected).ToList();

                foreach (BranchInfo branchInfo in branchesToRemove)
                {
                    if (branchInfo.IsSelected)
                    {
                        var branch = repo.Branches[branchInfo.Name];
                        if (branch != null && branch.IsRemote)
                        {
                            repo.Branches.Remove(branch);
                            RemoteBranches.Remove(branchInfo);
                        }
                    }
                }
            }
        }

        private void LoadConfig()
        {
            try
            {
                if (!File.Exists(ConfigFilePath)) return;

                string json = File.ReadAllText(ConfigFilePath);
                Config config = JsonSerializer.Deserialize<Config>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (config != null && !string.IsNullOrWhiteSpace(config.RepositoryPath))
                {
                    GitRepoPathTextBox.Text = config.RepositoryPath;
                    MaxRemoteAgeTextBox.Text = config.MaxRemoteBranchAgeMonths.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading repository path: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadLocalBranches_Click(object sender, RoutedEventArgs e)
        {
            using (var repo = new Repository(GitRepoPathTextBox.Text))
            {
                LocalBranches.Clear();
                skipUpdateStatusBar = true;

                var branches = repo.Branches
                    .Where(b => !b.IsRemote)
                    .ToList();
                var totalBranches = branches.Count;
                var currentBranchIndex = 0;
                var progressReporter = new Progress<int>(value =>
                {
                    UpdateStatusBar(value, totalBranches);
                });

                await Task.Run(() =>
                {
                    foreach (var branch in branches)
                    {
                        ((IProgress<int>)progressReporter).Report(++currentBranchIndex);
                        if (repo.Branches.Any(b => b.IsRemote && b.FriendlyName.Contains(branch.FriendlyName))) continue;

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            LocalBranches.Add(new BranchInfo
                            {
                                Name = branch.FriendlyName,
                                LastCommitBy = branch.Tip.Author.Name,
                                LastCommitDate = branch.Tip.Author.When.LocalDateTime.ToString()
                            });
                        });
                    }
                });
                skipUpdateStatusBar = false;
                UpdateStatusBar();
            }
        }

        private async void LoadRemoteBranches_Click(object sender, RoutedEventArgs e)
        {
            using (var repo = new Repository(GitRepoPathTextBox.Text))
            {
                RemoteBranches.Clear();
                skipUpdateStatusBar = true;

                var mainBranch = repo.Branches["main"] ?? repo.Branches["master"];
                if (mainBranch == null)
                {
                    System.Windows.MessageBox.Show("Main branch not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var mainCommits = new HashSet<Commit>(mainBranch.Commits);

                var branches = repo.Branches
                    .Where(b => b.IsRemote && b != mainBranch)
                    .ToList();
                var totalBranches = branches.Count;
                var currentBranchIndex = 0;
                var progressReporter = new Progress<int>(value =>
                {
                    UpdateStatusBar(value, totalBranches);
                });
                DateTime currentDate = DateTime.Now;
                _ = int.TryParse(MaxRemoteAgeTextBox.Text, out int maxAgeMonths);

                await Task.Run(() =>
                {
                    foreach (var branch in branches)
                    {
                        ((IProgress<int>)progressReporter).Report(++currentBranchIndex);
                        TimeSpan age = currentDate - branch.Tip.Author.When.LocalDateTime;
                        if ((age.TotalDays / 30) < maxAgeMonths) continue;
                        if (!IsBranchMergedIntoMain(mainCommits, branch)) continue;

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            RemoteBranches.Add(new BranchInfo
                            {
                                Name = branch.FriendlyName,
                                LastCommitBy = branch.Tip.Author.Name,
                                LastCommitDate = branch.Tip.Author.When.LocalDateTime.ToString()
                            });
                        });
                    }
                });
                skipUpdateStatusBar = false;
                UpdateStatusBar();
            }
        }

        private void LocalBranchesGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                ToggleSelectedRowsCheckState(LocalBranchesGrid);
            }
        }

        private void MaxRemoteAgeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void RemoteBranchesGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                ToggleSelectedRowsCheckState(RemoteBranchesGrid);
            }
        }

        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            string repoPath = GitRepoPathTextBox.Text.Trim();
            _ = int.TryParse(MaxRemoteAgeTextBox.Text, out var MaxRemoteBranchAgeMonths);
            Config config = new() { RepositoryPath = repoPath, MaxRemoteBranchAgeMonths = MaxRemoteBranchAgeMonths };

            try
            {
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigFilePath, json);
                System.Windows.MessageBox.Show("Repository path saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving repository path: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectRepoPath_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                GitRepoPathTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void UpdateStatusBar(int value, int total)
        {
            var tabs = new List<TabItem> { TabLocalBranches, TabRemoteBranches };
            Dispatcher.Invoke(() =>
            {
                if (tabs.Contains(MainTabControl.SelectedItem))
                {
                    StatusItem.Text = $"Progress: {value}/{total}";
                    MainProgressBar.Value = (double)value / total * 100;
                }
            });
        }

        private void UpdateStatusBar()
        {
            if (skipUpdateStatusBar) return;
            if (MainTabControl.SelectedItem == TabLocalBranches)
            {
                StatusItem.Text = $"Selected: {LocalBranches.Count(b => b.IsSelected)}; Filtered: {localBranchesView.Count}; Total: {LocalBranches.Count}";
                MainProgressBar.Value = 0;
            }
            else if (MainTabControl.SelectedItem == TabRemoteBranches)
            {
                StatusItem.Text = $"Selected: {RemoteBranches.Count(b => b.IsSelected)}; Filtered: {remoteBranchesView.Count}; Total: {RemoteBranches.Count}";
                MainProgressBar.Value = 0;
            }
            else
            {
                StatusItem.Text = string.Empty;
                MainProgressBar.Value = 0;
            }
        }

        public class BranchInfo : INotifyPropertyChanged
        {
            private bool isSelected;

            public event PropertyChangedEventHandler PropertyChanged;

            public bool IsSelected
            {
                get { return isSelected; }
                set
                {
                    if (isSelected != value)
                    {
                        isSelected = value;
                        NotifyPropertyChanged(nameof(IsSelected));
                    }
                }
            }

            public string LastCommitBy { get; set; }
            public string LastCommitDate { get; set; }
            public string Name { get; set; }

            private void NotifyPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class Config
        {
            public int MaxRemoteBranchAgeMonths { get; set; } = 6;
            public string RepositoryPath { get; set; }
        }
    }
}
