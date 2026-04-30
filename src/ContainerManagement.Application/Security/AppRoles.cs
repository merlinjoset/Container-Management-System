namespace ContainerManagement.Application.Security;

public static class AppRoles
{
    public const string Admin = "admin";
    public const string User = "user";
    public const string Agent = "agent";

    /// <summary>
    /// All built-in roles, in display order.
    /// </summary>
    public static readonly string[] All = { Admin, Agent, User };
}
