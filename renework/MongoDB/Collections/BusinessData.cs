// MongoDB/Collections/BusinessData.cs
using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace renework.MongoDB.Collections
{
    public class BusinessData
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("location")]
        public BusinessLocation Location { get; set; } = new();

        [BsonElement("area_sqm")]
        public double AreaSqm { get; set; }

        [BsonElement("monthlyRevenue")]
        public double MonthlyRevenue { get; set; }

        [BsonElement("budget")]
        public double Budget { get; set; }

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("downtimeMonths")]
        public int DowntimeMonths { get; set; }

        // Under development; stubbed as zero for now
        [BsonElement("totalLosses")]
        public double TotalLosses { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class BusinessLocation
    {
        [BsonElement("city")]
        public string City { get; set; } = string.Empty;

        [BsonElement("region")]
        public string Region { get; set; } = string.Empty;

        [BsonElement("address")]
        public string Address { get; set; } = string.Empty;
    }
}
