using RestSharp;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace WorkSmart.Application.Services
{
    public class FieldClassifierService
    {
        private readonly string _openAiKey;
        private readonly string _apiUrll;
        public FieldClassifierService(IConfiguration config)
        {
            _openAiKey = config["OpenAI:ApiKey"]!;
            _apiUrll = config["OpenAI:RCMUrl"]!;
        }

        public async Task<string> ClassifyFieldFromCV(string cvText)
        {
            var client = new RestClient(_apiUrll);
            var request = new RestRequest();
            request.Method = Method.Post;

            request.AddHeader("Authorization", $"Bearer {_openAiKey}");
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                model = "gpt-4o",
                messages = new[]
                {
        new { role = "system", content = "You are an AI assistant that classifies CVs into professional fields. Only reply with the field name without explanation." },
        new { role = "user", content = $"Given the following CV details, classify it into a single field (e.g., Frontend Development, Backend Development, Data Science, Finance, Marketing, Construction, Customer Service, Retail, E-commerce, etc.):\n\n{cvText}" }
             },
                temperature = 0.0
            };

            request.AddJsonBody(body);

            var response = await client.ExecuteAsync(request);



            if (!response.IsSuccessful)
                throw new Exception($"Failed to classify CV field: {response.Content}");

            var content = JObject.Parse(response.Content);
            var field = content["choices"]?[0]?["message"]?["content"]?.ToString()?.Trim();

            return field ?? "General"; // fallback nếu API lỗi nhẹ
        }
    }
}
