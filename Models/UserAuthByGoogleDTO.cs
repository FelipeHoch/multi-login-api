namespace multi_login.Models;

public class UserAuthByGoogleDTO
{
    public string Id { get; } = string.Empty;
    public string IdToken { get; } = string.Empty;
    public string FirstName { get; } = string.Empty;
    public string LastName { get; } = string.Empty;
    public string Name { get; } = string.Empty;
    public string Email { get; } = string.Empty;
    public string Provider { get; } = string.Empty;

}
