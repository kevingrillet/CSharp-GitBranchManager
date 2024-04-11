using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharp_GitBranchManager.Model
{
    /// <summary>
    /// TODO:
    /// - Observable
    /// - MVVM
    /// </summary>
    public class AppConfiguration
    {
        private static readonly char[] separator = [',', ';'];
        public static string FilePath { get; } = "config.json";
        public bool LocalMaxAgeCheckBox { get; set; } = false;
        public int LocalMaxAgeMonths { get; set; } = 6;
        public bool LocalUnusedCheckBox { get; set; } = true;
        public string RemoteExcludedBranches { get; set; } = "origin/main; origin/master";
        public bool RemoteExcludedBranchesCheckBox { get; set; } = true;
        public bool RemoteMaxAgeCheckBox { get; set; } = true;
        public int RemoteMaxAgeMonths { get; set; } = 6;
        public string RemoteMergedBranchValue { get; set; }
        public bool RemoteMergedCheckBox { get; set; } = true;
        public string RepositoryPath { get; set; }

        public List<string> GetListRemoteExcludedBranches()
        {
            return RemoteExcludedBranches
                .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(branch => branch.Trim())
                .ToList();
        }
    }
}
