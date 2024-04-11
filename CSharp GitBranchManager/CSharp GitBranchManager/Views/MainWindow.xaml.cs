using CSharp_GitBranchManager.Models;
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
using System.Windows.Threading;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace CSharp_GitBranchManager.Views
{
    /// <summary>
    /// TODO:
    /// - Component for tab 1 / 2
    /// - MVVM
    /// - Scroll
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string ConfigFilePath = "config.json";
        private const int filterDelayMilliseconds = 300;
        private static readonly char[] separator = [',', ';'];
        private readonly DispatcherTimer filterTimerLocal;
        private readonly DispatcherTimer filterTimerRemote;
        private readonly ListCollectionView localBranchesView;
        private readonly ListCollectionView remoteBranchesView;
        private bool skipUpdateStatusBar = false;
        public ObservableCollection<BranchInfo> LocalBranches { get; set; }
        public ObservableCollection<BranchInfo> RemoteBranches { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            LoadConfig();
            RemoteMaxAgeTextBox.PreviewTextInput += MaxRemoteAgeTextBox_PreviewTextInput;
            MainTabControl.SelectionChanged += (sender, e) => UpdateStatusBar();

            LocalBranches = new ObservableCollection<BranchInfo>();
            LocalBranchesGrid.ItemsSource = LocalBranches;
            LocalBranches.CollectionChanged += (sender, e) => UpdateStatusBar();
            localBranchesView = (ListCollectionView)CollectionViewSource.GetDefaultView(LocalBranches);
            SetupSorting(localBranchesView, LocalBranchesGrid);

            filterTimerLocal = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(filterDelayMilliseconds)
            };
            filterTimerLocal.Tick += FilterTimerLocal_Tick;

            LocalFilterTextBox.TextChanged += (sender, e) =>
            {
                filterTimerLocal.Stop();
                filterTimerLocal.Start();
            };

            RemoteBranches = new ObservableCollection<BranchInfo>();
            RemoteBranchesGrid.ItemsSource = RemoteBranches;
            RemoteBranches.CollectionChanged += (sender, e) => UpdateStatusBar();
            remoteBranchesView = (ListCollectionView)CollectionViewSource.GetDefaultView(RemoteBranches);
            SetupSorting(remoteBranchesView, RemoteBranchesGrid);

            filterTimerRemote = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(filterDelayMilliseconds)
            };
            filterTimerRemote.Tick += FilterTimerRemote_Tick;

            RemoteFilterTextBox.TextChanged += (sender, e) =>
            {
                filterTimerRemote.Stop();
                filterTimerRemote.Start();
            };
        }

        private static void ExportBranches(string branchType, IEnumerable<string> branchNames)
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

        private void ApplyLocalBranchesFilter()
        {
            localBranchesView.Filter = item =>
            {
                if (string.IsNullOrEmpty(LocalFilterTextBox.Text))
                    return true;

                return item is BranchInfo branchInfo && branchInfo.Name.Contains(LocalFilterTextBox.Text, StringComparison.OrdinalIgnoreCase);
            };
        }

        private void ApplyRemoteBranchesFilter()
        {
            remoteBranchesView.Filter = item =>
            {
                if (string.IsNullOrEmpty(RemoteFilterTextBox.Text))
                    return true;

                return item is BranchInfo branchInfo && branchInfo.Name.Contains(RemoteFilterTextBox.Text, StringComparison.OrdinalIgnoreCase);
            };
        }

        private void DeleteSelectedLocalBranches_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to delete the selected local branches?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            using (var repo = new Repository(GitRepoPathTextBox.Text))
            {
                var branchesToRemove = LocalBranches.Where(b => b.IsSelected).ToList();

                foreach (BranchInfo branchInfo in branchesToRemove)
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

        private void DeleteSelectedRemoteBranches_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to delete the selected remote branches?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            using (var repo = new Repository(GitRepoPathTextBox.Text))
            {
                var branchesToRemove = RemoteBranches.Where(b => b.IsSelected).ToList();

                foreach (BranchInfo branchInfo in branchesToRemove)
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

        private void ExportLocalBranches_Click(object sender, RoutedEventArgs e)
        {
            ExportBranches("LocalBranches", LocalBranches.Select(branch => branch.Name));
        }

        private void ExportRemoteBranches_Click(object sender, RoutedEventArgs e)
        {
            ExportBranches("RemoteBranches", RemoteBranches.Select(branch => branch.Name));
        }

        private void FilterTimerLocal_Tick(object sender, EventArgs e)
        {
            filterTimerLocal.Stop();
            ApplyLocalBranchesFilter();
            UpdateStatusBar();
        }

        private void FilterTimerRemote_Tick(object sender, EventArgs e)
        {
            filterTimerRemote.Stop();
            ApplyRemoteBranchesFilter();
            UpdateStatusBar();
        }

        private void LoadConfig()
        {
            try
            {
                if (!File.Exists(ConfigFilePath)) return;

                string json = File.ReadAllText(ConfigFilePath);
                AppConfiguration config = JsonSerializer.Deserialize<AppConfiguration>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (config != null && !string.IsNullOrWhiteSpace(config.GitRepositoryPath))
                {
                    // General
                    GitRepoPathTextBox.Text = config.GitRepositoryPath;
                    // Local
                    LocalUnusedCheckBox.IsChecked = config.LocalUnused;
                    LocalMaxAgeCheckBox.IsChecked = config.LocalMaxAge;
                    LocalMaxAgeTextBox.Text = config.LocalMaxAgeMonths.ToString();
                    // Remote
                    RemoteMergedCheckBox.IsChecked = config.RemoteMerged;
                    if (!string.IsNullOrWhiteSpace(config.RemoteMergedBranch))
                    {
                        RemoteMergedComboBox.Items.Clear();
                        RemoteMergedComboBox.Items.Add(config.RemoteMergedBranch);
                        RemoteMergedComboBox.SelectedIndex = 0;
                    }
                    RemoteMaxAgeCheckBox.IsChecked = config.RemoteMaxAge;
                    RemoteMaxAgeTextBox.Text = config.RemoteMaxAgeMonths.ToString();
                    RemoteExcludedBranchesCheckBox.IsChecked = config.RemoteExcludedBranches;
                    RemoteExcludedBranchesTextBox.Text = config.RemoteExcludedBranchesCSV;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading config: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                var currentBranchIndex = 0;
                var progressReporter = new Progress<int>(value =>
                {
                    UpdateStatusBar(value, branches.Count);
                });

                DateTime currentDate = DateTime.Now;
                _ = int.TryParse(LocalMaxAgeTextBox.Text, out int maxAgeMonths);
                var checkMaxAge = LocalMaxAgeCheckBox.IsChecked ?? false;
                var checkUnused = LocalUnusedCheckBox.IsChecked ?? false;

                await Task.Run(() =>
                {
                    foreach (var branch in branches)
                    {
                        ((IProgress<int>)progressReporter).Report(++currentBranchIndex);
                        TimeSpan age = currentDate - branch.Tip.Author.When.LocalDateTime;

                        if (checkMaxAge && age.TotalDays / 30 < maxAgeMonths) continue;

                        if (checkUnused && repo.Branches.Any(b => b.IsRemote && b.FriendlyName.Contains(branch.FriendlyName))) continue;

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

                var mainBranch = repo.Branches[RemoteMergedComboBox.Text];
                if (mainBranch == null)
                {
                    MessageBox.Show($"{RemoteMergedComboBox.Text} branch not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var mainCommits = new HashSet<Commit>(mainBranch.Commits);

                var remoteExcludedBranches = RemoteExcludedBranchesTextBox.Text
                    .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                    .Select(branch => branch.Trim())
                    .ToList();

                var branches = repo.Branches
                    .Where(b => b.IsRemote && b != mainBranch
                        && !remoteExcludedBranches.Contains(b.FriendlyName))
                    .ToList();

                var currentBranchIndex = 0;
                var progressReporter = new Progress<int>(value =>
                {
                    UpdateStatusBar(value, branches.Count);
                });

                DateTime currentDate = DateTime.Now;
                _ = int.TryParse(RemoteMaxAgeTextBox.Text, out int maxAgeMonths);
                var checkMaxAge = RemoteMaxAgeCheckBox.IsChecked ?? false;
                var checkMerged = RemoteMergedCheckBox.IsChecked ?? false;

                await Task.Run(() =>
                {
                    foreach (var branch in branches)
                    {
                        ((IProgress<int>)progressReporter).Report(++currentBranchIndex);
                        TimeSpan age = currentDate - branch.Tip.Author.When.LocalDateTime;

                        if (checkMaxAge && age.TotalDays / 30 < maxAgeMonths) continue;
                        if (checkMerged && !IsBranchMergedIntoMain(mainCommits, branch)) continue;

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

        private void ReloadRemoteBranches_Click(object sender, RoutedEventArgs e)
        {
            using (var repo = new Repository(GitRepoPathTextBox.Text))
            {
                var branches = repo.Branches
                    .Where(b => b.IsRemote)
                    .ToList();

                var value = RemoteMergedComboBox.Text;

                RemoteMergedComboBox.Items.Clear();

                foreach (var branch in branches)
                {
                    RemoteMergedComboBox.Items.Add(branch.FriendlyName.Replace("origin/", ""));
                }

                RemoteMergedComboBox.SelectedIndex = RemoteMergedComboBox.Items.IndexOf(value);
            }
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
            _ = int.TryParse(LocalMaxAgeTextBox.Text, out var LocalMaxAgeMonths);
            _ = int.TryParse(RemoteMaxAgeTextBox.Text, out var RemoteMaxAgeMonths);
            AppConfiguration config = new()
            {
                // General
                GitRepositoryPath = repoPath,
                // Local
                LocalUnused = LocalUnusedCheckBox.IsChecked ?? false,
                LocalMaxAge = LocalMaxAgeCheckBox.IsChecked ?? false,
                LocalMaxAgeMonths = LocalMaxAgeMonths,
                // Remote
                RemoteMerged = RemoteMergedCheckBox.IsChecked ?? false,
                RemoteMergedBranch = RemoteMergedComboBox.Text,
                RemoteMaxAge = RemoteMaxAgeCheckBox.IsChecked ?? false,
                RemoteMaxAgeMonths = RemoteMaxAgeMonths,
                RemoteExcludedBranches = RemoteExcludedBranchesCheckBox.IsChecked ?? false,
                RemoteExcludedBranchesCSV = RemoteExcludedBranchesTextBox.Text
            };

            try
            {
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigFilePath, json);
                MessageBox.Show("Config saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving config: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
