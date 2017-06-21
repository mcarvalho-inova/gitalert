using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitAlert.Model;

namespace GitAlert.Classes
{
    class Classes
    {

    }

    [Serializable]
    public class BranchModel
    {
        public string BranchName { get; set; }
        public List<CommitModel> Commits { get; set; }
        public string repositoryName { get; set; }
        public string BrachStatus { get; set; }
    }

    public class CommitModel
    {
        public string Id { get; set; }
        public string Author { get; set; }
        public string AuthorEmail { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
        public string BranchName { get; set; }
        public List<ItemsCommitModel> ItemsModificados { get; set; }
        public string RepoName { get; set; }
    }

    public class RepositorioModel
    {
        public List<BranchModel> Branchs { get; set; }
        public string RepoDescription { get; set; }
        public string RepoId { get; set; }
        public string RepoName { get; set; }
        public string RepoStatus { get; set; }
    }

    public class ItemsCommitModel
    {
        public string ChangeId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Status { get; set; }
    }


}


