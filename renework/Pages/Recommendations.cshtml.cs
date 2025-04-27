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

        // will hold the JSON from Django
        public List<RecommendationDto> Recommendations { get; set; } = new();

        // accept bizId as a query‐string or route parameter
        // Program.cs (BaseAddress should already be http://analytics:8000/api/)

        public async Task OnGetAsync(string bizId)
        {
            if (string.IsNullOrWhiteSpace(bizId)) return;

            var client = _http.CreateClient("analytics");

            // note the "api/" prefix is already in BaseAddress
            var result = await client.GetFromJsonAsync<List<RecommendationDto>>(
                $"api/business/{bizId}/google-search/");

            if (result != null)
                Recommendations = result;
        }

    }
}
