using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using multi_login.Entities;
using multi_login.Models;
using multi_login.Services;
using multi_login.Utils;
using System.Security.Cryptography;
using System.Text;


namespace multi_login.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class ProfileController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    private readonly IMapper _mapper;

    private readonly IJwtService _jwtService;

    public ProfileController(
        IUserRepository userRepository,
        IMapper mapper,
        IJwtService jwtService
        ) 
    {
        _userRepository = userRepository;

        _mapper = mapper;

        _jwtService = jwtService;
    }

    [HttpGet(Name = "GetProfile")]
    public async Task<ActionResult<UserFriendlyDTO>> Get()
    {
        var user = await _userRepository.GetUserByIdAsync(_jwtService.GetUserId());

        if (user == null)
        {
            return NotFound();
        }

        var userFriendly = _mapper.Map<UserFriendlyDTO>(user);

        return Ok(userFriendly);
    }


    [HttpPatch(Name = "UpdateProfile")]
    public async Task<IActionResult> Patch(string? redirect, [FromBody] JsonPatchDocument<User> patchDocument)
    {
        var userToUpdate = await _userRepository.GetUserByIdAsync(_jwtService.GetUserId());

        if (userToUpdate == null)
        {
            return NotFound();
        }

        patchDocument.ApplyTo(userToUpdate, ModelState);

        if (!ModelState.IsValid)
        {
            var values = ModelState.Values;

            foreach (var value in values)
            {
                if (value.Errors.Where(error => !error.ErrorMessage.Contains("was not found")).Count() > 0)
                    return BadRequest(ModelState);
            }
        }

        _userRepository.UpdateUser(userToUpdate);

        if (redirect != null) 
        {
            redirect += $"/{_jwtService.GetUserId()}";

            return RedirectPreserveMethod(redirect);
        }

        var userFriendly = _mapper.Map<UserFriendlyDTO>(userToUpdate);

        return CreatedAtRoute("GetUserById",
            new { id = userFriendly.Id }, userFriendly);
    }

    [HttpPatch("change-password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDTO)
    {
        try
        {
            if (changePasswordDTO == null) return BadRequest();

            var userId = _jwtService.GetUserId();

            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null) return BadRequest();

            if (user.Provider == "google") return BadRequest("Usuário não permitido troca de senha");
 
            if (user.Password != CalcHmac(changePasswordDTO.OldPassword)) return BadRequest();

            user.Password = CalcHmac(changePasswordDTO.NewPassword);

            _userRepository.UpdateUser(user);

            return NoContent();
        }
        catch (Exception)
        {
            throw;
        }
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
}
