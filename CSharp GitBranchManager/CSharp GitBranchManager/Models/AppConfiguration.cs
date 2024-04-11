using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharp_GitBranchManager.Models
{
    public class AppConfiguration
    {
        private static readonly char[] separator = [',', ';'];
        public static string FilePath { get; } = "config.json";
        public string GitRepositoryPath { get; set; }
        public bool LocalMaxAge { get; set; } = false;
        public int LocalMaxAgeMonths { get; set; } = 6;
        public bool LocalUnused { get; set; } = true;
        public bool RemoteExcludedBranches { get; set; } = true;
        public string RemoteExcludedBranchesCSV { get; set; } = "origin/main; origin/master";
        public bool RemoteMaxAge { get; set; } = true;
        public int RemoteMaxAgeMonths { get; set; } = 6;
        public bool RemoteMerged { get; set; } = true;
        public string RemoteMergedBranch { get; set; } = "master";

        public List<string> GetListRemoteExcludedBranches()
        {
            return RemoteExcludedBranchesCSV
                .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(branch => branch.Trim())
                .ToList();
        }
    }
}
