using CSharp_GitBranchManager.Utils;
using System.Collections.Generic;

namespace CSharp_GitBranchManager.Models
{
    public class Configuration : ANotifyPropertyChanged
    {
        public static readonly string FilePath = "config.json";

        private string _gitRepositoryPath = string.Empty;
        private bool _localMaxAge = false;
        private int _localMaxAgeMonths = 6;
        private bool _localUnused = true;
        private bool _remoteExcludedBranches = true;
        private string _remoteExcludedBranchesCSV = "origin/main; origin/master";
        private bool _remoteMaxAge = true;
        private int _remoteMaxAgeMonths = 6;
        private bool _remoteMerged = true;
        private string _remoteMergedBranch = "master";

        public string GitRepositoryPath
        {
            get => _gitRepositoryPath;
            set => SetField(ref _gitRepositoryPath, value);
        }

        public bool LocalMaxAge
        {
            get => _localMaxAge;
            set => SetField(ref _localMaxAge, value);
        }

        public int LocalMaxAgeMonths
        {
            get => _localMaxAgeMonths;
            set => SetField(ref _localMaxAgeMonths, value);
        }

        public bool LocalUnused
        {
            get => _localUnused;
            set => SetField(ref _localUnused, value);
        }

        public bool RemoteExcludedBranches
        {
            get => _remoteExcludedBranches;
            set => SetField(ref _remoteExcludedBranches, value);
        }

        public string RemoteExcludedBranchesCSV
        {
            get => _remoteExcludedBranchesCSV;
            set => SetField(ref _remoteExcludedBranchesCSV, value);
        }

        public bool RemoteMaxAge
        {
            get => _remoteMaxAge;
            set => SetField(ref _remoteMaxAge, value);
        }

        public int RemoteMaxAgeMonths
        {
            get => _remoteMaxAgeMonths;
            set => SetField(ref _remoteMaxAgeMonths, value);
        }

        public bool RemoteMerged
        {
            get => _remoteMerged;
            set => SetField(ref _remoteMerged, value);
        }

        public string RemoteMergedBranch
        {
            get => _remoteMergedBranch;
            set => SetField(ref _remoteMergedBranch, value);
        }

        public List<string> GetListRemoteExcludedBranches()
        {
            return RemoteExcludedBranchesCSV.CSVSplitAndTrim();
        }
    }
}
