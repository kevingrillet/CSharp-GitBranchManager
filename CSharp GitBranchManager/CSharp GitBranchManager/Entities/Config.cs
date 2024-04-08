namespace CSharp_GitBranchManager
{
    public partial class MainWindow
    {
        public class Config
        {
            public bool LocalMaxAgeCheckBox { get; set; } = false;
            public int LocalMaxAgeMonths { get; set; } = 6;
            public bool LocalUnusedCheckBox { get; set; } = true;
            public string RemoteExcludedBranches { get; set; } = "main; master";
            public bool RemoteExcludedBranchesCheckBox { get; set; } = true;
            public bool RemoteMaxAgeCheckBox { get; set; } = true;
            public int RemoteMaxAgeMonths { get; set; } = 6;
            public string RemoteMergedBranchValue { get; set; }
            public bool RemoteMergedCheckBox { get; set; } = true;
            public string RepositoryPath { get; set; }
        }
    }
}
