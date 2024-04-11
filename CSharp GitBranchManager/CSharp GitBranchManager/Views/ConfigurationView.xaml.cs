using CSharp_GitBranchManager.ViewModels;
using LibGit2Sharp;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace CSharp_GitBranchManager.Views
{
    public partial class ConfigurationView : System.Windows.Controls.UserControl
    {
        public AppConfigurationViewModel ViewModel { get; set; }

        public ConfigurationView()
        {
            InitializeComponent();
            ViewModel = new AppConfigurationViewModel();
            DataContext = ViewModel;
        }

        private void ReloadRemoteBranches_Click(object sender, RoutedEventArgs e)
        {
            using (var repo = new Repository(ViewModel.Configuration.GitRepositoryPath))
            {
                var branches = repo.Branches.Where(b => b.IsRemote).ToList();

                var value = RemoteMergedComboBox.Text;

                RemoteMergedComboBox.Items.Clear();

                foreach (var branch in branches)
                {
                    RemoteMergedComboBox.Items.Add(branch.FriendlyName.Replace("origin/", ""));
                }

                RemoteMergedComboBox.SelectedIndex = RemoteMergedComboBox.Items.IndexOf(value);
            }
        }

        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Save();
        }

        private void SelectRepositoryPath_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                ViewModel.Configuration.GitRepositoryPath = folderBrowserDialog.SelectedPath;
            }
        }
    }
}
