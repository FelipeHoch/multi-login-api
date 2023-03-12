namespace multi_login.Models;

public class UserFriendlyWithIdpIdDTO
{
    public string Id { get; set; } = string.Empty;
    public string IdentityProviderId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

}

