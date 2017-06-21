
using System;
using System.Collections.Generic;
using GitAlert.Model;

namespace Bonobo.Git.Server.Models
{
    public class TeamModel : INameProperty
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public UserModel[] Members { get; set; }

        public string DisplayName
        {
            get
            {
                return Name;
            }
        }
    }

    public class TeamEditModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public UserModel[] AllUsers { get; set; }

        public UserModel[] SelectedUsers { get; set; }

        public Guid[] PostedSelectedUsers { get; set; }
    }

    public class TeamDetailModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public UserModel[] Members { get; set; }

        public RepositoryModel[] Repositories { get; set; }
        public bool IsReadOnly { get; set; }
    }

    public class TeamDetailModelList : List<TeamDetailModel>
    {
        public bool IsReadOnly { get; set; }
    }
}