using System.Windows.Input;

namespace CSharp_GitBranchManager.Views
{
    public partial class ConfigurationView : System.Windows.Controls.UserControl
    {
        public ConfigurationView()
        {
            InitializeComponent();
            LocalMaxAgeTextBox.PreviewTextInput += MaxAgeTextBox_PreviewTextInput;
            RemoteMaxAgeTextBox.PreviewTextInput += MaxAgeTextBox_PreviewTextInput;
        }

        private void MaxAgeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }
    }
}
