using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace renework.MongoDB.Collections
{
    public class User
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("hashedPassword")]
        public string HashedPassword { get; set; }

        [BsonElement("role")]
        public string Role { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("skills")]
        public List<string> Skills { get; set; } = new();

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("appliedCourses")]
        public List<string> AppliedCourses { get; set; } = new();

        // Optional: track when the user was created
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("status")]
        public string Status { get; set; }
    }
}
