using multi_login.Entities;

namespace multi_login.Services;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetUsersAsync();
    Task<User> GetUserByIdAsync(string id);
    Task<User> GetUserByEmailAsync(string email);
    void AddUser(User user);
    void UpdateUser(User user);
    void DeleteUser(string id);
    Task<bool> UserExistsAsync(string id);
}
