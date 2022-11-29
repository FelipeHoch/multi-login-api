using MongoDB.Driver;

namespace multi_login.Services;

public class MongoRepository : IMongoRepository
{
    private readonly IMongoClient _mongoClient;

    public IMongoClient Client => _mongoClient;

    // public IMongoDatabase Database { get; }

    public MongoRepository(MongoRepositoryOptions mongoRepositoryOptions)
    {
        var conf = MongoClientSettings.FromConnectionString(mongoRepositoryOptions.ConnectionString);

        conf.ApplicationName = mongoRepositoryOptions.ClientName;

        _mongoClient = new MongoClient(conf);

        // Database = _mongoClient.GetDatabase(mongoRepositoryOptions.Database);
    }
}
