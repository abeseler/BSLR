namespace Beseler.Shared;

public static class Endpoints
{
    public static class Accounts
    {
        private const string BaseRoute = "/api/accounts";

        public const string Register = $"{BaseRoute}/register";
        public const string Login = $"{BaseRoute}/login";
        public const string Logout = $"{BaseRoute}/logout";
        public const string Refresh = $"{BaseRoute}/refresh";
        public const string ConfirmEmail = $"{BaseRoute}/confirm-email";
        public const string Info = $"{BaseRoute}/info";
    }
}
