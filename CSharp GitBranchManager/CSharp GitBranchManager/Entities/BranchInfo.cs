using System.ComponentModel;

namespace CSharp_GitBranchManager
{
    public partial class MainWindow
    {
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
    }
}
