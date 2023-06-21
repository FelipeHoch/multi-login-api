using multi_login.Entities;
using multi_login.Models;

namespace multi_login.Services;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetUsersAsync();
    Task<User> GetUserByIdAsync(string id);
    Task<User> GetUserByEmailAsync(string email, string loginMethod);
    Task<User> AddUser(User user);
    void UpdateUser(User user);
    void DeleteUser(string id);
    Task<bool> UserIsAuth(UserForAuthWithPasswordDTO userForAuth);
    Task<bool> UserExistsAsync(string id, string loginMethod);
    Task<bool> IsPasswordCorrect(string id, string password);
}
