using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace renework.MongoDB.Collections
{
    public class AppliedCourse
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonElement("courseId"), BsonRepresentation(BsonType.ObjectId)]
        public string CourseId { get; set; }

        [BsonElement("progress")]
        public double Progress { get; set; }

        [BsonElement("rating")]
        public double Rating { get; set; }

        [BsonElement("duration"), BsonRepresentation(BsonType.String)]
        public TimeSpan Duration { get; set; }

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
