namespace multi_login.Services;

public static class MongoRepositoryDependencyInjectionExtension
{
    public static IServiceCollection AddMongoRepository(this IServiceCollection serviceCollection, MongoRepositoryOptions mongoOptions)
    {
        serviceCollection.AddSingleton(mongoOptions);

        serviceCollection.AddSingleton<IMongoRepository, MongoRepository>();

        return serviceCollection;
    }
}

public class MongoRepositoryOptions
{
    public string ConnectionString { get; set; }

    public string ClientName { get; set; }
}
