using multi_login.Entities;
using multi_login.Models;

namespace multi_login.Services;

public class UserRepository : IUserRepository
{

    public void AddUser(User user)
    {
        throw new NotImplementedException();
    }

    public void DeleteUser(string id)
    {
        throw new NotImplementedException();
    }

    public Task<User> GetUserByEmailAsync(string email)
    {
        throw new NotImplementedException();
    }

    public Task<User> GetUserByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<User>> GetUsersAsync()
    {
        throw new NotImplementedException();
    }

    public void UpdateUser(User user)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UserIsAuth(UserForAuthWithPasswordDTO userForAuth)
    {
        // Mock implementation
        var user = new User("Teste", "teste@teste.com", "admin", "123456");

        if (userForAuth.Email == user.Email && userForAuth.Password == user.Password)
        {
            return Task.FromResult(true);
        } 
        else
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> UserExistsAsync(string id)
    {
        throw new NotImplementedException();
    }
}
