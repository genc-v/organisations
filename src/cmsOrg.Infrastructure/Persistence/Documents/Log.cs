using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace cmsOrg.Infrastructure.Persistence.Documents;

public class Log
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public string? OrganisationId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string? ResourceId { get; set; }
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
