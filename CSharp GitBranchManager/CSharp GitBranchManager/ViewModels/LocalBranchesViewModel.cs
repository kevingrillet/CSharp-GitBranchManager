using CSharp_GitBranchManager.Models;
using System;
using System.Collections.ObjectModel;

namespace CSharp_GitBranchManager.ViewModels
{
    public class LocalBranchesViewModel : ABranchesViewModel
    {
        public LocalBranchesViewModel(Configuration appConfiguration) : base(appConfiguration)
        {
        }

        protected override void DeleteChecked(object obj)
        {
            throw new NotImplementedException();
        }

        protected override void ExportCommand(object obj)
        {
            throw new NotImplementedException();
        }

        protected override void LoadData()
        {
            Branches =
            [
                new() { Name = "Branch 1", IsChecked = false },
                new() { Name = "Branch 2", IsChecked = true },
                new() { Name = "Branch 3", IsChecked = false }
            ];

            FilteredBranches = new ObservableCollection<BranchInfo>(Branches);
        }

        protected override void LoadGrid(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
