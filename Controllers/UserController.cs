using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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


    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }

    private static string Base64Encode<T>(T data)
    {
        var dataStr = System.Text.Json.JsonSerializer.Serialize(data);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(dataStr));
    }
}
