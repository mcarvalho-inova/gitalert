namespace Bonobo.Git.Server.Models
{
    public class LogOnModel
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public int DatabaseResetCode { get; set; }

        public string ReturnUrl { get; set; }
    }

    public class ForgotPasswordModel
    {
        public string Username { get; set; }
    }

    public class ResetPasswordModel
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public string Digest { get; set; }
    }
}