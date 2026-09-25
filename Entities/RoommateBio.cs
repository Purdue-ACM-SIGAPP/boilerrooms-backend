namespace SimpleWebAppReact.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

/// <summary>
/// A user's roommate-matching bio: their interests, housing preferences, contact info,
/// and a free-form description, so other users can find them (issue #87)
/// </summary>
public class RoommateBio
{
    [BsonId]
    [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userId"), BsonRepresentation(BsonType.String)]
    public string? UserId { get; set; }

    [BsonElement("bio"), BsonRepresentation(BsonType.String)]
    public string? Bio { get; set; }

    [BsonElement("interests")]
    public List<string> Interests { get; set; } = new();

    /// <summary>
    /// ids of buildings (see Entities/Building.cs) this user is interested in living in
    /// </summary>
    [BsonElement("preferredBuildingIds")]
    public List<string> PreferredBuildingIds { get; set; } = new();

    [BsonElement("contactInfo"), BsonRepresentation(BsonType.String)]
    public string? ContactInfo { get; set; }

    [BsonElement("createdAt"), BsonRepresentation(BsonType.DateTime)]
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
}
