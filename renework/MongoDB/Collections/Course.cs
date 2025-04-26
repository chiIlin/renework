using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace renework.MongoDB.Collections
{
    public class Course
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("tags")]
        public List<string> Tags { get; set; } = new();

        [BsonElement("duration")]
        [BsonRepresentation(BsonType.String)]
        public TimeSpan Duration { get; set; }

        [BsonElement("company")]
        public string Company { get; set; }

        [BsonElement("link")]
        public string Link { get; set; }

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [BsonElement("status")]
        public string Status { get; set; }
    }
}
