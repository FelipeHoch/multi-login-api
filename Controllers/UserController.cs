﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using multi_login.Entities;
using multi_login.Models;
using multi_login.Services;
using multi_login.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
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

    private readonly IHttpContextAccessor _httpContext;

    public UserController(
        IUserRepository userRepository,
        IMapper mapper,
        IJwtService jwtService,
        IHttpContextAccessor httpContext
        ) 
    {
        _userRepository = userRepository;

        _mapper = mapper;

        _jwtService = jwtService;

        _httpContext = httpContext;
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] JObject userFromClient, [FromQuery] string? redirect)
    {
        try
        {
            var userToCreate = userFromClient.ToObject<User>();

            if (userToCreate == null)
            {
                return BadRequest();
            }

            userToCreate.Provider = userToCreate.Provider.ToLower();

            userToCreate.Email = userToCreate.Email.ToLower();

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
                var authHeader = _httpContext.HttpContext.Request.Headers["Authorization"];

                _httpContext.HttpContext.Response.Headers.Add("Authorization", authHeader);

                var http = new HttpClient();

                http.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);
                
                var userSerialized = JsonConvert.SerializeObject(userFromClient, new JsonSerializerSettings
                {
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                });

                var content = new StringContent(userSerialized, Encoding.UTF8, "application/json");

                var response = await http.PostAsync(redirect + "?data=" + Base64Encode(userFriendly), content);

                if (response.IsSuccessStatusCode)
                {
                    return Ok(userFromClient);
                }
                
                return BadRequest();
            }

            return Ok(userFriendly);
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
            return NotFound("Usuário não encontrado");
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

        userToUpdate.Email = userToUpdate.Email.ToLower();

        if (await _userRepository.IsDuplicatedEmail(userToUpdate.Id, userToUpdate.Email, userToUpdate.Provider)) return BadRequest("Email já cadastrado");

        _userRepository.UpdateUser(userToUpdate);

        if (redirect != null) 
        {
            var authHeader = _httpContext.HttpContext.Request.Headers["Authorization"];

            _httpContext.HttpContext.Response.Headers.Add("Authorization", authHeader);

            var http = new HttpClient();

            http.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);

            var patchSerialized = JsonConvert.SerializeObject(patchDocument, new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            });

            var content = new StringContent(patchSerialized, Encoding.UTF8, "application/json");

            var response = await http.PatchAsync(redirect, content);

            var stringResult = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<JObject>(stringResult);

            if (response.IsSuccessStatusCode)
            {
                return Ok(result);
            }

            return BadRequest();
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

        if (userToUpdate.Provider == "google") return BadRequest("Usuário não permitido para troca de senha");

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
                var authHeader = _httpContext.HttpContext.Request.Headers["Authorization"];

                _httpContext.HttpContext.Response.Headers.Add("Authorization", authHeader);

                var http = new HttpClient();

                http.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);

                var response = await http.DeleteAsync(redirect);

                if (response.IsSuccessStatusCode)
                {
                    return Ok();
                }

                return BadRequest();
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
