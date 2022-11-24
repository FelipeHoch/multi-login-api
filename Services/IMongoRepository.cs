using MongoDB.Driver;

namespace multi_login.Services;

public interface IMongoRepository
{
    IMongoDatabase Database { get; }
}
