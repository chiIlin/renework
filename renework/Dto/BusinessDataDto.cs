// Dto/BusinessDataDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace renework.Dto
{
    public class BusinessDataDto
    {
        [Required] public string City { get; set; } = "";
        [Required] public string Region { get; set; } = "";
        [Required] public string Address { get; set; } = "";

        [Range(0, double.MaxValue)]
        public double AreaSqm { get; set; }

        [Range(0, double.MaxValue)]
        public double MonthlyRevenue { get; set; }

        [Range(0, double.MaxValue)]
        public double Budget { get; set; }

        public string Description { get; set; } = "";

        [Range(0, int.MaxValue)]
        public int DowntimeMonths { get; set; }

        // TotalLosses computed server-side; no input
    }
}
