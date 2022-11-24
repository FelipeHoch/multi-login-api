using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using multi_login.Models;
using multi_login.Services;

namespace multi_login.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public AuthController(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    [HttpPost(Name = "AuthUserWithPassword")]
    [HttpHead]
    public async Task<ActionResult> AuthUserWithPassword(UserForAuthWithPasswordDTO userForAuth)
    {
        if (await _userRepository.UserIsAuth(userForAuth)) return Ok();

        return Unauthorized();
    }
}
