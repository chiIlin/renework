using System;
using System.Globalization;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace renework.MongoDB.Collections
{
    public class Course
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("tags")]
        public List<string> Tags { get; set; } = new();

        [BsonElement("duration")]
        [BsonSerializer(typeof(TimeSpanCustomSerializer))]
        public TimeSpan Duration { get; set; }

        [BsonElement("company")]
        public string Company { get; set; } = string.Empty;

        [BsonElement("link")]
        public string Link { get; set; } = string.Empty;

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [BsonElement("status")]
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Custom serializer for TimeSpan that handles multiple BSON types and serializes as string.
    /// </summary>
    public class TimeSpanCustomSerializer : SerializerBase<TimeSpan>
    {
        public override void Serialize(
            BsonSerializationContext context,
            BsonSerializationArgs args,
            TimeSpan value)
        {
            // Serialize all TimeSpan values as ISO 8601 duration string ("c" format)
            context.Writer.WriteString(value.ToString("c", CultureInfo.InvariantCulture));
        }

        public override TimeSpan Deserialize(
            BsonDeserializationContext context,
            BsonDeserializationArgs args)
        {
            var reader = context.Reader;
            var bsonType = reader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.String:
                    var raw = reader.ReadString();
                    return TimeSpanHelper.Parse(raw);
                case BsonType.Int64:
                    var ticks = reader.ReadInt64();
                    return TimeSpan.FromTicks(ticks);
                case BsonType.Int32:
                    var sec = reader.ReadInt32();
                    return TimeSpan.FromSeconds(sec);
                case BsonType.Double:
                    var dbl = reader.ReadDouble();
                    return TimeSpan.FromSeconds(dbl);
                case BsonType.Document:
                    var doc = BsonDocumentSerializer.Instance.Deserialize(context);
                    return ParseFromDocument(doc);
                default:
                    reader.SkipValue();
                    return TimeSpan.Zero;
            }
        }

        private TimeSpan ParseFromDocument(BsonDocument doc)
        {
            if (doc.TryGetValue("Ticks", out var t) && t.IsInt64)
            {
                return TimeSpan.FromTicks(t.AsInt64);
            }
            var days = doc.GetValue("Days", BsonValue.Create(0)).ToInt32();
            var hours = doc.GetValue("Hours", BsonValue.Create(0)).ToInt32();
            var minutes = doc.GetValue("Minutes", BsonValue.Create(0)).ToInt32();
            var seconds = doc.GetValue("Seconds", BsonValue.Create(0)).ToInt32();
            var milliseconds = doc.GetValue("Milliseconds", BsonValue.Create(0)).ToInt32();
            return new TimeSpan(days, hours, minutes, seconds)
                   .Add(TimeSpan.FromMilliseconds(milliseconds));
        }
    }

    /// <summary>
    /// Helper for parsing human-readable duration strings into TimeSpan.
    /// </summary>
    public static class TimeSpanHelper
    {
        public static TimeSpan Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return TimeSpan.Zero;

            raw = raw.Trim().ToLowerInvariant();
            var parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // e.g. "9 weeks", "3 days", "5 hours", "30 mins"
            if (parts.Length == 2 &&
                int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var num))
            {
                var unit = parts[1];
                if (unit.StartsWith("week")) return TimeSpan.FromDays(num * 7);
                if (unit.StartsWith("day")) return TimeSpan.FromDays(num);
                if (unit.StartsWith("hour")) return TimeSpan.FromHours(num);
                if (unit.StartsWith("min")) return TimeSpan.FromMinutes(num);
                if (unit.StartsWith("sec")) return TimeSpan.FromSeconds(num);
            }

            // fallback to standard TimeSpan parsing
            if (TimeSpan.TryParseExact(raw, "c", CultureInfo.InvariantCulture, out var ts) ||
                TimeSpan.TryParse(raw, CultureInfo.InvariantCulture, out ts))
            {
                return ts;
            }

            return TimeSpan.Zero;
        }
    }
}

