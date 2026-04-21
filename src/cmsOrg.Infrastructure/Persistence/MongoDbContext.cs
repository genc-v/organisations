using cmsOrg.Infrastructure.Persistence.Documents;
using MongoDB.Driver;

namespace cmsOrg.Infrastructure.Persistence;

public class MongoDbContext
{
    public IMongoDatabase Database { get; }
    public IMongoCollection<Log> Logs { get; }

    public MongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        Database = client.GetDatabase(databaseName);
        Logs = Database.GetCollection<Log>("logs");
    }
}
