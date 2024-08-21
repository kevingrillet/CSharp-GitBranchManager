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
    public interface IConfigurationViewModel
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

    public class ConfigurationViewModel : ANotifyPropertyChanged, IConfigurationViewModel
    {
        private readonly JsonSerializerOptions _loadOptions = new() { PropertyNameCaseInsensitive = true };
        private readonly ObservableCollection<string> _remoteBranches;
        private readonly JsonSerializerOptions _saveOptions = new() { WriteIndented = true };
        private AppConfiguration _configuration;

        #region Commands

        public ICommand ReleadRemoteBranchesCommand => new RelayCommand(ReleadRemoteBranches);
        public ICommand SaveCommand => new RelayCommand(Save);
        public ICommand SelectRepositoryPathCommand => new RelayCommand(SelectRepositoryPath);
        public ICommand ValidateTextInputCommand => new RelayCommand<string>(ValidateTextInput);

        #endregion Commands

        #region Properties

        public Models.AppConfiguration Configuration
        {
            get => _configuration;
            set => SetField(ref _configuration, value);
        }

        public ObservableCollection<string> RemoteBranches
        {
            get { return _remoteBranches; }
        }

        #endregion Properties

        public ConfigurationViewModel(Models.AppConfiguration appConfiguration)
        {
            _configuration = appConfiguration;
            _remoteBranches = ["master"];
            Load();
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(Models.AppConfiguration.FilePath)) return;

                string json = File.ReadAllText(Models.AppConfiguration.FilePath);
                var config = JsonSerializer.Deserialize<Models.AppConfiguration>(json, _loadOptions);
                _remoteBranches.Clear();
                _remoteBranches.Add(config.RemoteMergedBranch);
                Configuration = config;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading config: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReleadRemoteBranches()
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

        private void Save()
        {
            try
            {
                var jsonString = JsonSerializer.Serialize(Configuration, _saveOptions);
                File.WriteAllText(Models.AppConfiguration.FilePath, jsonString);
                MessageBox.Show("Config saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving config: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectRepositoryPath()
        {
            var folderBrowserDialog = new FolderBrowserDialog
            {
                SelectedPath = Configuration.GitRepositoryPath ?? string.Empty
            };

            DialogResult result = folderBrowserDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                Configuration.GitRepositoryPath = folderBrowserDialog.SelectedPath;
                OnPropertyChanged(nameof(Configuration));
            }
        }

        private void ValidateTextInput(string input)
        {
            if (!string.IsNullOrEmpty(input) && input.Any(c => !char.IsDigit(c)))
            {
                MessageBox.Show("Only numeric characters are allowed.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
