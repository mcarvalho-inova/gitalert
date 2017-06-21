
using System;
using System.Collections.Generic;

namespace GitAlert.Model
{
    public class RoleModel : INameProperty
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid[] Members { get; set; }

        public string DisplayName
        {
            get
            {
                return Name;
            }
        }
    }

    public class UserModel : INameProperty
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }

        public string DisplayName
        {
            get
            {
                var compositeName = String.Format("{0} {1}", GivenName, Surname).Trim();
                if (String.IsNullOrEmpty(compositeName))
                {
                    // Return the username if we don't have a GivenName or Surname
                    return Username;
                }
                else
                {
                    return compositeName;
                }
            }
        }

        string INameProperty.Name
        {
            get { return Username; }
        }

        /// <summary>
        /// This is the name we'd sort users by
        /// </summary>
        public string SortName
        {
            get
            {
                var compositeName = Surname + GivenName;
                if (String.IsNullOrEmpty(compositeName))
                {
                    return Username;
                }
                return compositeName;
            }
        }
    }

    public class UserEditModel
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string Email { get; set; }

        public string OldPassword { get; set; }

        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }

        public string[] Roles { get; set; }

        public string[] SelectedRoles { get; set; }
        public string[] PostedSelectedRoles { get; set; }
    }

    public class UserDetailModel
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string Email { get; set; }

        public string[] Roles { get; set; }
        public bool IsReadOnly { get; set; }
    }

    public class UserDetailModelList : List<UserDetailModel>
    {
        public bool IsReadOnly { get; set; }
    }

    public class UserCreateModel
    {
        public string Username { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }
}