using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace renework.MongoDB.Collections
{
    // Ignore any extra fields in stored documents
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        // Primary field is "username", but accept legacy "name" values
        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("firstName")]
        public string FirstName { get; set; }

        [BsonElement("surname")]
        public string Surname { get; set; }

        [BsonElement("companyName"), BsonIgnoreIfNull]
        public string? CompanyName { get; set; }

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

        [BsonElement("businessId"), BsonRepresentation(BsonType.ObjectId)]
        public string? BusinessId { get; set; } 

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
    