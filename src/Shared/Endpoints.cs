namespace Beseler.Shared;

public static class Endpoints
{
    public static class Accounts
    {
        private const string BaseRoute = "/api/accounts";

        public const string Register = $"{BaseRoute}/register";
        public const string Login = $"{BaseRoute}/login";
        public const string Refresh = $"{BaseRoute}/refresh";
        public const string ConfirmEmail = $"{BaseRoute}/confirm-email";
        public const string ResetPassword = $"{BaseRoute}/reset-password";
    }

    public static class Budgeting
    {
        private const string BaseRoute = "/api/budgets";

        public const string Budget = $"{BaseRoute}";
        public const string Transactions = $"{BaseRoute}/transactions";
    }
}
