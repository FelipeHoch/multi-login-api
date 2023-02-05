using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
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

    private readonly IMapper _mapper;

    public AuthController(IUserRepository userRepository, IConfiguration config, IMapper mapper)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

        _config = config;

        _mapper = mapper;
    }

    [HttpPost(Name = "AuthUserWithPassword")]
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

    [HttpPost("google")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserFriendlyWithTokenDTO>> GoogleClientId(TokenDTO token)
    {
        GoogleJsonWebSignature.Payload tokenPayload;
        User user;

        Console.WriteLine(token.Token);

        try
        {
            tokenPayload = await GoogleJsonWebSignature.ValidateAsync(token.Token);
        } 
        catch
        {
            return Unauthorized();
        }

        if (!TokenIsValid(tokenPayload)) return Unauthorized();

        bool userExists = await _userRepository.UserExistsAsync(tokenPayload.Email);

        if (!userExists)
        {
            user = _mapper.Map<User>(tokenPayload);

            user = await _userRepository.AddUser(user);
        } 
        else
        {
            user = await _userRepository.GetUserByEmailAsync(tokenPayload.Email);
        }

        string appToken = GenerateToken(user);

        var userWithToken = _mapper.Map<UserFriendlyWithTokenDTO>(user);

        userWithToken.Token = appToken;

        return Ok(userWithToken);
    }

    private string CalcHmac(string data)
    {
        var secretKey = Environment.GetEnvironmentVariable("SECRET_PASSWORD");

        byte[] key = Encoding.ASCII.GetBytes(secretKey);
        HMACSHA256 myhmacsha256 = new(key);
        byte[] byteArray = Encoding.ASCII.GetBytes(data);
        MemoryStream stream = new(byteArray);
        string result = myhmacsha256.ComputeHash(stream).Aggregate("", (s, e) => s + string.Format("{0:x2}", e), s => s);
        return result;
    }

    private string GenerateToken(User user)
    {
        var secretKeyEnconded = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY")));
        var signinCredentials = new SigningCredentials(secretKeyEnconded, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim("name", user.Name, ClaimTypes.GivenName),
            new Claim("role", user.Role, ClaimTypes.Role),
            new Claim("email", user.Email, ClaimTypes.Email),
            new Claim("act", user.Id, ClaimTypes.Actor)
        };

        var iat = (int)(DateTime.Now.Subtract(DateTime.UnixEpoch)).TotalSeconds;
        claims.Add(new Claim("iat", iat.ToString(), ClaimValueTypes.Integer));

        var exp = int.Parse(Environment.GetEnvironmentVariable("JWT_LIFETIME"));
        DateTime dt = DateTime.Now.AddSeconds(exp);

        var iss = Environment.GetEnvironmentVariable("JWT_ISSUER");

        var tokenOptions = new JwtSecurityToken(
            claims: claims,
            signingCredentials: signinCredentials,
            issuer: iss,
            expires: dt
        );

        var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

        return token;
    }

    private bool TokenIsValid(GoogleJsonWebSignature.Payload tokenPayload)
    {
        var googleClient = Environment.GetEnvironmentVariable("GOOGLE_CLIENT");

        if (tokenPayload == null) return false;

        return tokenPayload.AudienceAsList.FirstOrDefault(aud => aud == googleClient) != null;
    }

    private static string Base64Encode<T>(T data)
    {
        var dataStr = System.Text.Json.JsonSerializer.Serialize(data);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(dataStr));
    }
}
