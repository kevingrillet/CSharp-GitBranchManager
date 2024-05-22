using CSharp_GitBranchManager.Utils;

namespace CSharp_GitBranchManager.Models
{
    public class BranchInfo : ANotifyPropertyChanged
    {
        private bool _isChecked;
        private string _lastCommitBy;
        private string _lastCommitDate;
        private string _name;

        public bool IsChecked
        {
            get => _isChecked;
            set => SetField(ref _isChecked, value);
        }


        public string LastCommitBy
        {
            get => _lastCommitBy;
            set => SetField(ref _lastCommitBy, value);
        }

        public string LastCommitDate
        {
            get => _lastCommitDate;
            set => SetField(ref _lastCommitDate, value);
        }

        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }
    }
}
