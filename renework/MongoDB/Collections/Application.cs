// renework/MongoDB/Collections/Application.cs
using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace renework.MongoDB.Collections
{
    public class Application
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = "";

        [BsonElement("courseId"), BsonRepresentation(BsonType.ObjectId)]
        public string CourseId { get; set; } = "";

        [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = "";

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
            
        [BsonElement("status")]
        public string Status { get; set; } = "Pending";

        [BsonElement("cvFileName")]
        public string CVFileName { get; set; } = "";

        [BsonElement("letterFileName")]
        public string LetterFileName { get; set; } = "";
    }
}
