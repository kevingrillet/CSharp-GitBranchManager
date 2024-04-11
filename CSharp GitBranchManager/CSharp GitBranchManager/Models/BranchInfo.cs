using CSharp_GitBranchManager.Utils;

namespace CSharp_GitBranchManager.Models
{
    public class BranchInfo : ANotifyPropertyChanged
    {
        private bool _isSelected;
        private string _lastCommitBy;
        private string _lastCommitDate;
        private string _name;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string LastCommitBy
        {
            get { return _lastCommitBy; }
            set
            {
                if (_lastCommitBy != value)
                {
                    _lastCommitBy = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string LastCommitDate
        {
            get { return _lastCommitDate; }
            set
            {
                if (_lastCommitDate != value)
                {
                    _lastCommitDate = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
