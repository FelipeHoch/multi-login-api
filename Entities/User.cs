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
    public string Name { get; set; }

    [BsonRequired]
    [BsonElement("email")]
    [MaxLength(150)]
    public string Email { get; set; }

    [BsonRequired]
    [BsonElement("role")]
    [MaxLength(50)]
    public string Role { get; set; }

    [BsonIgnoreIfNull]
    [BsonElement("password")]
    [MaxLength(300)]
    public string Password { get; set; }

    public User(string name, string email, string role, string password)
    {
        Name = name;
        Email = email;
        Role = role;
        Password = password;
    }
}

