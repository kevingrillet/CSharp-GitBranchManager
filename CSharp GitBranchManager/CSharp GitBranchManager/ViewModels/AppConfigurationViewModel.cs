using CSharp_GitBranchManager.Models;
using CSharp_GitBranchManager.Utils;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace CSharp_GitBranchManager.ViewModels
{
    public class AppConfigurationViewModel : ANotifyPropertyChanged
    {
        private readonly JsonSerializerOptions _loadOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        private readonly JsonSerializerOptions _saveOptions = new JsonSerializerOptions { WriteIndented = true };
        private AppConfiguration _configuration;

        public AppConfiguration Configuration
        {
            get { return _configuration; }
            set
            {
                _configuration = value;
                NotifyPropertyChanged();
            }
        }

        public AppConfigurationViewModel()
        {
            Load();
        }

        public void Save()
        {
            try
            {
                var jsonString = JsonSerializer.Serialize(Configuration, _saveOptions);
                File.WriteAllText(AppConfiguration.FilePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration: {ex.Message}");
            }
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(AppConfiguration.FilePath)) return;

                string json = File.ReadAllText(AppConfiguration.FilePath);
                Configuration = JsonSerializer.Deserialize<AppConfiguration>(json, _loadOptions);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading config: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
