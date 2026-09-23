namespace SimpleWebAppReact.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

/// <summary>
/// A post from a user looking for a roommate
/// </summary>
public class RoommatePost
{
    [BsonId]
    [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userId"), BsonRepresentation(BsonType.String)]
    public string? UserId { get; set; }

    [BsonElement("description"), BsonRepresentation(BsonType.String)]
    public string? Description { get; set; }

    [BsonElement("createdAt"), BsonRepresentation(BsonType.DateTime)]
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
}
