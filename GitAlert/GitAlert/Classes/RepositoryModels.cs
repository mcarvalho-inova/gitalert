using GitAlert.Model;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Bonobo.Git.Server.Models
{
    public class RepositoryModel : INameProperty
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public string Description { get; set; }
        public bool AnonymousAccess { get; set; }
        public UserModel[] Users { get; set; }
        public UserModel[] Administrators { get; set; }
        public TeamModel[] Teams { get; set; }
        public bool AuditPushUser { get; set; }
        public byte[] Logo { get; set; }
        public bool RemoveLogo { get; set; }
        public string LinksRegex { get; set; }
        public string LinksUrl { get; set; }
        public bool LinksUseGlobal { get; set; }

        public RepositoryModel()
        {
            LinksUseGlobal = true;
            LinksUrl = "";
            LinksRegex = "";
        }

        public bool NameIsValid
        {
            get
            {
                // Check for an exact match, not just a substring hit - based on RegularExpressionAttribute
                Match match = Regex.Match(Name, NameValidityRegex);
                return match.Success && match.Index == 0 && match.Length == Name.Length;
            }
        }

        public string DisplayName
        {
            get
            {
                return Name;
            }
        }

        public void EnsureCollectionsAreValid()
        {
            if (Administrators == null)
            {
                Administrators = new UserModel[0];
            }

            if (Users == null)
            {
                Users = new UserModel[0];
            }

            if (Teams == null)
            {
                Teams = new TeamModel[0];
            }
        }

        public const string NameValidityRegex = @"([\w\.-])*([\w])$";
    }

    public class RepositoryDetailModel
    {
        public RepositoryDetailModel()
        {
        }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Group { get; set; }

        public string Description { get; set; }

        public UserModel[] Users { get; set; }
        public Guid[] PostedSelectedUsers { get; set; }
        public UserModel[] AllUsers { get; set; }

        public TeamModel[] Teams { get; set; }
        public Guid[] PostedSelectedTeams { get; set; }
        public TeamModel[] AllTeams { get; set; }

        public UserModel[] Administrators { get; set; }
        public Guid[] PostedSelectedAdministrators { get; set; }
        public UserModel[] AllAdministrators { get; set; }

        public bool IsCurrentUserAdministrator { get; set; }

        public bool AllowAnonymous { get; set; }

        public RepositoryDetailStatus Status { get; set; }

        public RepositoryLogoDetailModel Logo { get; set; }
        public string GitUrl { get; set; }
        public string PersonalGitUrl { get; set; }

        public string LinksRegex { get; set; }

        public string LinksUrl { get; set; }

        public bool LinksUseGlobal { get; set; }
    }

    public enum RepositoryDetailStatus
    {
        Unknown = 0,
        Valid,
        Missing
    }

    public class RepositoryTreeDetailModel
    {
        public string Name { get; set; }

        public string CommitMessage { get; set; }

        public DateTime? CommitDate { get; set; }
        public string CommitDateString { get { return CommitDate.HasValue ? CommitDate.Value.ToString() : CommitDate.ToString(); } }

        public string Author { get; set; }
        public bool IsTree { get; set; }
        public bool IsLink { get; set; }
        public string TreeName { get; set; }
        public bool IsImage { get; set; }
        public bool IsText { get; set; }
        public bool IsMarkdown { get; set; }
        public string Path { get; set; }
        public byte[] Data { get; set; }
        public string Text { get; set; }
        public string TextBrush { get; set; }
        public Encoding Encoding { get; set; }
        public RepositoryLogoDetailModel Logo { get; set; }
    }

    public class RepositoryTreeModel
    {
        public string Name { get; set; }
        public string Branch { get; set; }
        public string Path { get; set; }
        public string Readme { get; set; }
        public RepositoryLogoDetailModel Logo { get; set; }
        public IEnumerable<RepositoryTreeDetailModel> Files { get; set; }
    }

    public class RepositoryCommitsModel
    {
        public string Name { get; set; }
        public RepositoryLogoDetailModel Logo { get; set; }
        public IEnumerable<RepositoryCommitModel> Commits { get; set; }
    }

    public class RepositoryCommitChangeModel
    {
        public string ChangeId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public ChangeKind Status { get; set; }
        public int LinesAdded { get; set; }
        public int LinesDeleted { get; set; }
        public int LinesChanged { get { return LinesAdded + LinesDeleted; } }
        public string Patch { get; set; }
    }

    public class RepositoryCommitNoteModel
    {
        public RepositoryCommitNoteModel(string message, string @namespace)
        {
            this.Message = message;
            this.Namespace = @namespace;
        }

        public string Message { get; set; }

        public string Namespace { get; set; }
    }

    public class RepositoryCommitModel
    {
        public RepositoryCommitModel()
        {
            Links = new List<string>();
        }

        public string Name { get; set; }
        public RepositoryLogoDetailModel Logo { get; set; }

        public string ID { get; set; }

        public string TreeID { get; set; }

        public string[] Parents { get; set; }

        public string Author { get; set; }

        public string AuthorEmail { get; set; }

        public string AuthorAvatar { get; set; }

        public DateTime Date { get; set; }

        private string _message;

        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
            }
        }

        public string MessageShort { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public IEnumerable<RepositoryCommitChangeModel> Changes { get; set; }

        public IEnumerable<RepositoryCommitNoteModel> Notes { get; set; }

        public IEnumerable<string> Links { get; set; }
    }

    public class RepositoryBlameModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public RepositoryLogoDetailModel Logo { get; set; }
        public long FileSize { get; set; }
        public long LineCount { get; set; }
        public IEnumerable<RepositoryBlameHunkModel> Hunks { get; set; }
    }

    public class RepositoryBlameHunkModel
    {
        public RepositoryCommitModel Commit { get; set; }
        public string[] Lines { get; set; }
    }

    public class RepositoryLogoDetailModel
    {
        private byte[] _data;

        public RepositoryLogoDetailModel()
        {
        }

        public RepositoryLogoDetailModel(byte[] data)
        {
            this._data = data;
        }
    }
}