using CSharp_GitBranchManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharp_GitBranchManager.Models
{
    public class AppConfiguration : ANotifyPropertyChanged
    {
        private static readonly char[] separator = [',', ';'];
        private string _gitRepositoryPath;
        private bool _localMaxAge = false;
        private int _localMaxAgeMonths = 6;
        private bool _localUnused = true;
        private bool _remoteExcludedBranches = true;
        private string _remoteExcludedBranchesCSV = "origin/main; origin/master";
        private bool _remoteMaxAge = true;
        private int _remoteMaxAgeMonths = 6;
        private bool _remoteMerged = true;
        private string _remoteMergedBranch = "master";
        public static string FilePath { get; } = "config.json";

        public string GitRepositoryPath
        {
            get { return _gitRepositoryPath; }
            set
            {
                if (_gitRepositoryPath != value)
                {
                    _gitRepositoryPath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool LocalMaxAge
        {
            get { return _localMaxAge; }
            set
            {
                if (_localMaxAge != value)
                {
                    _localMaxAge = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int LocalMaxAgeMonths
        {
            get { return _localMaxAgeMonths; }
            set
            {
                if (_localMaxAgeMonths != value)
                {
                    _localMaxAgeMonths = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool LocalUnused
        {
            get { return _localUnused; }
            set
            {
                if (_localUnused != value)
                {
                    _localUnused = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool RemoteExcludedBranches
        {
            get { return _remoteExcludedBranches; }
            set
            {
                if (_remoteExcludedBranches != value)
                {
                    _remoteExcludedBranches = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string RemoteExcludedBranchesCSV
        {
            get { return _remoteExcludedBranchesCSV; }
            set
            {
                if (_remoteExcludedBranchesCSV != value)
                {
                    _remoteExcludedBranchesCSV = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool RemoteMaxAge
        {
            get { return _remoteMaxAge; }
            set
            {
                if (_remoteMaxAge != value)
                {
                    _remoteMaxAge = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int RemoteMaxAgeMonths
        {
            get { return _remoteMaxAgeMonths; }
            set
            {
                if (_remoteMaxAgeMonths != value)
                {
                    _remoteMaxAgeMonths = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool RemoteMerged
        {
            get { return _remoteMerged; }
            set
            {
                if (_remoteMerged != value)
                {
                    _remoteMerged = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string RemoteMergedBranch
        {
            get { return _remoteMergedBranch; }
            set
            {
                if (_remoteMergedBranch != value)
                {
                    _remoteMergedBranch = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public List<string> GetListRemoteExcludedBranches()
        {
            return RemoteExcludedBranchesCSV
                .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(branch => branch.Trim())
                .ToList();
        }
    }
}
