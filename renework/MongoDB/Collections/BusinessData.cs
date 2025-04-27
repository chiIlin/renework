using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace renework.MongoDB.Collections
{
    [BsonIgnoreExtraElements]
    public class BusinessData
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

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

        [BsonElement("downtimeStart")]
        public DateTime DowntimeStart { get; set; }

        [BsonElement("totalLosses")]
        public double TotalLosses { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public void CalculateTotalLosses(DateTime ended, double meterPrice)
        {
            var now = DateTime.UtcNow;
            int yearDiff  = now.Year  - ended.Year;
            int monthDiff = now.Month - ended.Month;
            int dayDiff   = now.Day   - ended.Day;

            double D = 12 * yearDiff
                     + monthDiff
                     + (dayDiff / 30.0);

            double A = AreaSqm * meterPrice;
            double B = MonthlyRevenue * D;

            TotalLosses = A + B;
        }
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
