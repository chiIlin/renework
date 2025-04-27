// Pages/Recommendations.cshtml.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using renework.Dto;

namespace renework.Pages
{
    public class RecommendationsModel : PageModel
    {
        private readonly IHttpClientFactory _http;

        public RecommendationsModel(IHttpClientFactory http)
        {
            _http = http;
        }

        public List<RecommendationDto> Recommendations { get; set; } = new();

        public async Task OnGetAsync()
        {
            var client = _http.CreateClient();
            // TODO: set BaseAddress or use named client in Program.cs
            var result = await client.GetFromJsonAsync<List<RecommendationDto>>("/api/recommendations");
            if (result != null)
                Recommendations = result;
        }
    }
}
