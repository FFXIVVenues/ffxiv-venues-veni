using FFXIVVenues.Veni.Api.Models;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Api
{
    internal class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Venue>> GetAllVenuesAsync()
        {
            var response = await _httpClient.GetAsync($"/venue");
            return await response.Content.ReadFromJsonAsync<Venue[]>();
        }

        public async Task<IEnumerable<Venue>> GetAllVenuesAsync(ulong forContact)
        {
            var response = await _httpClient.GetAsync($"/venue?contact={forContact}");
            return await response.Content.ReadFromJsonAsync<Venue[]>();
        }

        public async Task<Venue> GetVenueAsync(string id)
        {
            var response = await _httpClient.GetAsync("/venue/" + id);
            return await response.Content.ReadFromJsonAsync<Venue>();
        }

        public Task<HttpResponseMessage> PutVenueAsync(Venue venue) =>
            _httpClient.PutAsJsonAsync("/venue/" + venue.Id, venue);

        public async Task<HttpResponseMessage> PutVenueBannerAsync(string id, string url)
        {
            var stream = await _httpClient.GetStreamAsync(url);
            return await this.PutVenueBannerAsync(id, stream);
        }

        public Task<HttpResponseMessage> PutVenueBannerAsync(string id, Stream stream) =>
            _httpClient.PutAsync("/venue/" + id + "/media", new StreamContent(stream));

        public Task<HttpResponseMessage> DeleteVenueAsync(string id) =>
            _httpClient.DeleteAsync("/venue/" + id);

        public Task<HttpResponseMessage> OpenVenueAsync(string id) =>
            _httpClient.PutAsJsonAsync($"/venue/{id}/open", true);

        public Task<HttpResponseMessage> CloseVenueAsync(string id) =>
            _httpClient.PutAsJsonAsync($"/venue/{id}/open", false);

        public Task<HttpResponseMessage> ApproveAsync(string id, bool approval = true) =>
            _httpClient.PutAsJsonAsync($"/venue/{id}/approved", approval);

    }
}