using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace renework.MongoDB.Collections
{
    public class CourseReview
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonElement("courseId"), BsonRepresentation(BsonType.ObjectId)]
        public string CourseId { get; set; }

        [BsonElement("comment")]
        public string Comment { get; set; }

        [BsonElement("rating")]
        public int Rating { get; set; }

        [BsonElement("company")]
        public string Company { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
