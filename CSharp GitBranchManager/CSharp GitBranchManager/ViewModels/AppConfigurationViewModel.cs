using CSharp_GitBranchManager.Models;
using CSharp_GitBranchManager.Utils;
using LibGit2Sharp;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;

namespace CSharp_GitBranchManager.ViewModels
{
    public interface IAppConfigurationViewModel
    {
        AppConfiguration Configuration { get; set; }
        ObservableCollection<string> RemoteBranches { get; }

        #region Commands

        ICommand ReleadRemoteBranchesCommand { get; }
        ICommand SaveCommand { get; }
        ICommand SelectRepositoryPathCommand { get; }
        ICommand ValidateTextInputCommand { get; }

        #endregion Commands
    }

    public class AppConfigurationViewModel : ANotifyPropertyChanged, IAppConfigurationViewModel
    {
        private readonly JsonSerializerOptions _loadOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        private readonly ObservableCollection<string> _remoteBranches;
        private readonly JsonSerializerOptions _saveOptions = new JsonSerializerOptions { WriteIndented = true };
        private AppConfiguration _configuration;

        #region Commands

        public ICommand ReleadRemoteBranchesCommand { get => new RelayCommand<object>(ReleadRemoteBranches); }
        public ICommand SaveCommand { get => new RelayCommand<object>(Save); }
        public ICommand SelectRepositoryPathCommand { get => new RelayCommand<object>(SelectRepositoryPath); }
        public ICommand ValidateTextInputCommand { get => new RelayCommand<string>(ValidateTextInput); }

        #endregion Commands

        #region Properties

        public AppConfiguration Configuration
        {
            get => _configuration;
            set => SetField(ref _configuration, value);
        }

        public ObservableCollection<string> RemoteBranches
        {
            get { return _remoteBranches; }
        }

        #endregion Properties

        public AppConfigurationViewModel()
        {
            _remoteBranches = new ObservableCollection<string>() { "master" };
            Load();
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(AppConfiguration.FilePath)) return;

                string json = File.ReadAllText(AppConfiguration.FilePath);
                var config = JsonSerializer.Deserialize<AppConfiguration>(json, _loadOptions);
                _remoteBranches.Clear();
                _remoteBranches.Add(config.RemoteMergedBranch);
                Configuration = config;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading config: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReleadRemoteBranches(object parameter)
        {
            using (var repo = new Repository(Configuration.GitRepositoryPath))
            {
                var branches = repo.Branches.Where(b => b.IsRemote).ToList();

                var value = Configuration.RemoteMergedBranch;

                _remoteBranches.Clear();

                foreach (var branch in branches)
                {
                    _remoteBranches.Add(branch.FriendlyName.Replace("origin/", ""));
                }

                Configuration.RemoteMergedBranch = value;
            }
        }

        private void Save(object parameter)
        {
            try
            {
                var jsonString = JsonSerializer.Serialize(Configuration, _saveOptions);
                File.WriteAllText(AppConfiguration.FilePath, jsonString);
                MessageBox.Show("Config saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving config: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectRepositoryPath(object parameter)
        {
            var folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = Configuration.GitRepositoryPath;
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                Configuration.GitRepositoryPath = folderBrowserDialog.SelectedPath;
                OnPropertyChanged(nameof(Configuration));
            }
        }

        private void ValidateTextInput(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                foreach (char c in input)
                {
                    if (!char.IsDigit(c))
                    {
                        MessageBox.Show("Only numeric characters are allowed.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }
        }
    }
}
