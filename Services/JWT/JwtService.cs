
using System.Security.Claims;

public interface IJwtService
{
    string GetUserName();
    string GetRole();
    string GetEmail();

    string GetUserId();
}

public class JwtService : IJwtService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUserName()
    {
        return _httpContextAccessor.HttpContext.User.Identity.Name;
    }

    public string GetRole()
    {
        return _httpContextAccessor.HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
    }

    public string GetEmail()
    {
        return _httpContextAccessor.HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
    }

    public string GetUserId()
    {
        return _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    }
}
