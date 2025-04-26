using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace renework.MongoDB.Collections
{
    public class Application
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonElement("courseId"), BsonRepresentation(BsonType.ObjectId)]
        public string CourseId { get; set; }

        [BsonElement("status")]
        public string Status { get; set; }

        [BsonElement("letter")]
        public string Letter { get; set; }

        // if you really want to embed the PDF, use byte[] + GridFS in production;
        // or store a URL/path here instead of raw bytes.
        [BsonElement("cv")]
        public byte[] Cv { get; set; }

        [BsonElement("submittedAt")]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
