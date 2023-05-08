using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.AI.Davinci
{
    internal class DavinciService : IDavinciService
    {
        private readonly HttpClient httpClient;
        private double maxTokens = 150;
        private double temperature = 0.7;
        private string apiKey;
        
        public DavinciService( DavinciConfiguration configuration)
        {
            apiKey = configuration.ApiKey;
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<String> AskTheAI(string prompt)
        {
           
            var requestPayload = new
                {
                    model = "gpt-4-turbo",
                    messages = new[]
                    {
                    new { role = "system", content = "You are a helpful assistant." },
                    new { role = "user", content = prompt }
                }
                };

            var content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");
            var response = client.PostAsync("https://api.openai.com/v1/chat/completions", content).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

                if (result == null)
                    throw new Exception("Davinci3 APIKey Error"); 

                return result.choices[0].text;
            }
            throw new Exception($"Request failed with status code {response.StatusCode}");
        }
    }
}
