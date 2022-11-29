using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using multi_login.Entities;
using multi_login.Models;
using multi_login.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace multi_login.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    private readonly IConfiguration _config;

    public AuthController(IUserRepository userRepository, IConfiguration config)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

        _config = config;
    }

    [HttpPost(Name = "AuthUserWithPassword")]
    [HttpOptions]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<string>> AuthUserWithPassword(UserForAuthWithPasswordDTO userForAuth)
    {
        userForAuth.Password = CalcHmac(userForAuth.Password);

        if (!await _userRepository.UserIsAuth(userForAuth)) return Unauthorized();

        var user = await _userRepository.GetUserByEmailAsync(userForAuth.Email);

        var token = GenerateToken(user);
            
        return Ok(token);
    }

    [HttpGet(Name = "GoogleClientIdKey")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<string> GoogleClientId()
    {
        Console.WriteLine("hit");

        return Ok(_config.GetValue<string>("Google:ClientId"));
    }

    private string CalcHmac(string data)
    {
        var secretKey = _config.GetValue<string>("SecretKeys:Password");

        byte[] key = Encoding.ASCII.GetBytes(secretKey);
        HMACSHA256 myhmacsha256 = new(key);
        byte[] byteArray = Encoding.ASCII.GetBytes(data);
        MemoryStream stream = new(byteArray);
        string result = myhmacsha256.ComputeHash(stream).Aggregate("", (s, e) => s + string.Format("{0:x2}", e), s => s);
        return result;
    }

    private string GenerateToken(User user)
    {
        var secretKeyEnconded = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<string>("Jwt:SecretKey")));
        var signinCredentials = new SigningCredentials(secretKeyEnconded, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>();

        claims.Add(new Claim("name", user.Name, ClaimTypes.GivenName));
        claims.Add(new Claim("role", user.Role, ClaimTypes.Role));
        claims.Add(new Claim("email", user.Email, ClaimTypes.Email));
        claims.Add(new Claim("act", user.Id, ClaimTypes.Actor));

        var iat = (int)(DateTime.Now.Subtract(DateTime.UnixEpoch)).TotalSeconds;
        claims.Add(new Claim("iat", iat.ToString(), ClaimValueTypes.Integer));

        var exp = _config.GetValue<int>("Jwt:Lifetime");
        DateTime dt = DateTime.Now.AddSeconds(exp);
        exp = (int)dt.Subtract(DateTime.UnixEpoch).TotalSeconds;
        claims.Add(new Claim("exp", exp.ToString(), ClaimValueTypes.Integer));

        var tokenOptions = new JwtSecurityToken(
            claims: claims,
            signingCredentials: signinCredentials
        );

        var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

        return token;
    }
}
