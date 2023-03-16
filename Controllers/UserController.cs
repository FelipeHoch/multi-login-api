using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;
using multi_login.Entities;
using multi_login.Models;
using multi_login.Services;
using System.Text;


namespace multi_login.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    private readonly IMapper _mapper;

    public UserController(
        IUserRepository userRepository,
        IMapper mapper
        ) 
    {
        _userRepository = userRepository;

        _mapper = mapper;
    }

    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    [AllowAnonymous]
    [HttpGet("{id}", Name = "UserById")]
    public string Get(int id)
    {
        return "value";
    }

    [AllowAnonymous]
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

            var user = await _userRepository.AddUser(userToCreate);

            var userFriendly = _mapper.Map<UserFriendlyDTO>(user);

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

    [AllowAnonymous]
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

    [AllowAnonymous]
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
}
