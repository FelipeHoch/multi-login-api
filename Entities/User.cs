using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace multi_login.Entities;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("_id")]
    public string Id { get; set; }

    [BsonRequired]
    [BsonElement("name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [BsonRequired]
    [BsonElement("email")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [BsonRequired]
    [BsonElement("role")]
    [MaxLength(50)]
    public string Role { get; set; } = string.Empty;

    [BsonRequired]
    [BsonElement("provider")]
    [MaxLength(50)]
    public string Provider { get; set; } = string.Empty;

    [BsonIgnoreIfNull]
    [BsonElement("password")]
    [MaxLength(300)]
    public string? Password { get; set; }
}

