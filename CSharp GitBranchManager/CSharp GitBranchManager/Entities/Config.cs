namespace CSharp_GitBranchManager
{
    public partial class MainWindow
    {
        public class Config
        {
            public int MaxRemoteBranchAgeMonths { get; set; } = 6;
            public string RepositoryPath { get; set; }
        }
    }
}
