using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Bonobo.Git.Server.Models
{
    public class GlobalSettingsModel
    {
        
        public bool AllowAnonymousPush { get; set; }

        
        public bool AllowAnonymousRegistration { get; set; }

        
        public bool AllowUserRepositoryCreation { get; set; }

        
        public bool AllowPushToCreate { get; set; }

        
        public string RepositoryPath { get; set; }

        
        public string DefaultLanguage { get; set; }

        
        public string SiteTitle { get; set; }

        
        public string SiteLogoUrl { get; set; }

        
        public string SiteFooterMessage { get; set; }

        
        public bool IsCommitAuthorAvatarVisible { get; set; }

        
        public string LinksUrl { get; set; }

        public string LinksRegex { get; set; }
    }
}