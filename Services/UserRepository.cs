using MongoDB.Driver;
using multi_login.Entities;
using multi_login.Models;

namespace multi_login.Services;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _usersCollection;

    public IMongoRepository _mongoRepository;

    public UserRepository(IMongoRepository mongoRepository)
    {
        _mongoRepository = mongoRepository;

        _usersCollection = _mongoRepository.Client.GetDatabase("multilogin").GetCollection<User>("users");
    }

    public async Task<User> AddUser(User user)
    {
        await _usersCollection.InsertOneAsync(user);

        return user;
    }

    public void DeleteUser(string id)
    {
        throw new NotImplementedException();
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        return await _usersCollection.Aggregate()
                                     .Match(user => user.Email == email)
                                     .FirstOrDefaultAsync();
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

    public async Task<bool> UserIsAuth(UserForAuthWithPasswordDTO userForAuth)
    {
        var user = await _usersCollection.Aggregate()
                                         .Match(user => user.Email == userForAuth.Email && user.Password == userForAuth.Password)
                                         .FirstOrDefaultAsync();

        return user != null;
    }

    public async Task<bool> UserExistsAsync(string email)
    {
       var user = await _usersCollection.Aggregate()
                                        .Match(user => user.Email == email)
                                        .FirstOrDefaultAsync();

        return user != null;
    }
}
