using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.AI
{
    internal class DavinciProxy
    {
        static double maxTokens { get; } = 150;
        double temperature { get; set; } = 0.7;
        private readonly string apiKey = "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";//CHANGE TO USE IT ON SECRET

        public String askTheAI (string prompt)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var requestPayload = new
                {
                    prompt = prompt,
                    temperature = temperature,
                    max_tokens = maxTokens
                };
                var content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");
                var response = client.PostAsync("https://api.openai.com/v1/engines/text-davinci-003/completions", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;

                    var result = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

                    //If theres an error Luis will handle using this 404 as a error tag
                    if (result == null)
                        return "404";

                    Console.WriteLine(result.choices[0].text);
                    return result.choices[0].text;
                }
                else
                {
                    Console.WriteLine($"Request failed with status code {response.StatusCode}");
                    return "404";
                }
            }
        }
    }
}
