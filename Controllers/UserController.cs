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
[Authorize(Roles = "admin")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    private readonly IMapper _mapper;

    private readonly IJwtService _jwtService;

    public UserController(
        IUserRepository userRepository,
        IMapper mapper,
        IJwtService jwtService
        ) 
    {
        _userRepository = userRepository;

        _mapper = mapper;

        _jwtService = jwtService;
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] User userToCreate, [FromQuery] string? redirect)
    {
        try
        {
            if (userToCreate == null)
            {
                return BadRequest();
            }

            userToCreate.Provider = userToCreate.Provider.ToLower();

            var userAlreadyExists = await _userRepository.GetUserByEmailAsync(userToCreate.Email, userToCreate.Provider);

            if (userAlreadyExists != null) 
            {
                return BadRequest();
            }

            var password = "";

            if (userToCreate.Provider == "credentials")
            {
                password = PasswordGenerator.GeneratePassword(12);

                userToCreate.Password = CalcHmac(password);
            }

            var user = await _userRepository.AddUser(userToCreate);

            var userFriendly = _mapper.Map<UserFriendlyDTO>(user);

            if (userToCreate.Provider == "credentials")
            {
                userFriendly.NewPassword = password;
            }

            if (redirect != null)
            {          
                return RedirectPreserveMethod(redirect + "?data=" + Base64Encode(userFriendly));
            }

            return CreatedAtRoute("UserById", new { id = userFriendly.Id }, userFriendly);
        }
        catch (Exception) 
        {
            throw;
        }
    }

    [HttpPatch("{id}", Name = "UpdateUser")]
    public async Task<IActionResult> Patch(string id, [FromQuery] string? redirect, [FromBody] JsonPatchDocument<User> patchDocument)
    {
        var userToUpdate = await _userRepository.GetUserByIdAsync(id);

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
            return RedirectPreserveMethod(redirect);
        }

        var userFriendly = _mapper.Map<UserFriendlyDTO>(userToUpdate);

        return CreatedAtRoute("GetUserById",
            new { id = userFriendly.Id }, userFriendly);
    }

    [HttpPatch("{id}/change-password")]
    public async Task<IActionResult> Patch(string id)
    {
        var userToUpdate = await _userRepository.GetUserByIdAsync(id);

        if (userToUpdate == null)
        {
            return NotFound();
        }

        if (userToUpdate.Provider == "google") return BadRequest("Usuário não permitido troca de senha");

        var password = PasswordGenerator.GeneratePassword(12);

        userToUpdate.Password = CalcHmac(password);

        _userRepository.UpdateUser(userToUpdate);

        return Ok(password);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id, [FromQuery] string? redirect)
    {
        try
        {
            var userExists = await _userRepository.GetUserByIdAsync(id);

            if (userExists == null)
            {
                return BadRequest();
            }

            _userRepository.DeleteUser(id);

            if (redirect != null)
            {
                return RedirectPreserveMethod(redirect);
            }

            return NoContent();
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static string Base64Encode<T>(T data)
    {
        var dataStr = System.Text.Json.JsonSerializer.Serialize(data);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(dataStr));
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
