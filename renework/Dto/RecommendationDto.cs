// Dto/RecommendationDto.cs
namespace renework.Dto
{
    public class RecommendationDto
    {
        public string Type { get; set; } = "";
        public string Location { get; set; } = "";
        public double AreaSqm { get; set; }
        public string Link { get; set; } = "";
        public double Price { get; set; }
    }
}
