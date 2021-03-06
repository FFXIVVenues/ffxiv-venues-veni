using FFXIVVenues.Veni.Api.Models;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
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
            var response = await _httpClient.GetAsync($"/venue?manager={forContact}");
            return await response.Content.ReadFromJsonAsync<Venue[]>();
        }

        public async Task<IEnumerable<Venue>> GetAllVenuesAsync(string searchQuery)
        {
            var response = await _httpClient.GetAsync($"/venue?search={searchQuery}");
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
            var response = await this._httpClient.GetAsync(url);
            var stream = await response.Content.ReadAsStreamAsync();
            return await this.PutVenueBannerAsync(id, stream, response.Content.Headers.ContentType);
        }

        public Task<HttpResponseMessage> PutVenueBannerAsync(string id, Stream stream, MediaTypeHeaderValue mediaType)
        {
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = mediaType;
            return this._httpClient.PutAsync("/venue/" + id + "/media", streamContent);
        }

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