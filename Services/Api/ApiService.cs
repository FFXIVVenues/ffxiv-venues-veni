using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.Services.Api
{
    internal class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly RollingCache<Venue> _venueCache;
        private readonly RollingCache<Venue[]> _venuesCache;

        public ApiService(HttpClient httpClient)
        {
            this._httpClient = httpClient;
            this._venueCache = new(60*1000, 10 * 60 * 1_000);
            this._venuesCache = new(60*1000, 10 * 60 * 1_000);
        }

        public async Task<IEnumerable<Venue>> GetAllVenuesAsync()
        {
            var cached = this._venuesCache.Get("*");
            if (cached.Result == CacheResult.CacheHit)
                return cached.Value;
            var response = await _httpClient.GetAsync($"/venue");
            var result = await response.Content.ReadFromJsonAsync<Venue[]>();
            this._venuesCache.Set("*", result);
            return result;
        }

        public async Task<IEnumerable<Venue>> GetAllVenuesAsync(ulong forContact)
        {
            var cached = this._venuesCache.Get(forContact.ToString());
            if (cached.Result == CacheResult.CacheHit)
                return cached.Value;
            var response = await _httpClient.GetAsync($"/venue?manager={forContact}");
            var result = await response.Content.ReadFromJsonAsync<Venue[]>();
            this._venuesCache.Set(forContact.ToString(), result);
            return result;
        }

        public async Task<IEnumerable<Venue>> GetOpenVenuesAsync()
        {
            var cached = this._venuesCache.Get("_open_");
            if (cached.Result == CacheResult.CacheHit)
                return cached.Value;
            var response = await _httpClient.GetAsync($"/venue?open=true");
            var result = await response.Content.ReadFromJsonAsync<Venue[]>();
            this._venuesCache.Set("_open_", result);
            return result;
        }

        public async Task<IEnumerable<Venue>> GetApprovedVenuesAsync()
        {
            var cached = this._venuesCache.Get("_approved_");
            if (cached.Result == CacheResult.CacheHit)
                return cached.Value;
            var response = await _httpClient.GetAsync($"/venue?approved=true");
            var result = await response.Content.ReadFromJsonAsync<Venue[]>();
            this._venuesCache.Set("_approved_", result);
            return result;
        }
        
        public async Task<IEnumerable<Venue>> GetUnapprovedVenuesAsync()
        {
            var cached = this._venuesCache.Get("_unapproved_");
            if (cached.Result == CacheResult.CacheHit)
                return cached.Value;
            var response = await _httpClient.GetAsync($"/venue?approved=false");
            var result = await response.Content.ReadFromJsonAsync<Venue[]>();
            this._venuesCache.Set("_unapproved_", result);
            return result;
        }

        public async Task<IEnumerable<Venue>> GetAllVenuesAsync(string searchQuery)
        {
            var cached = this._venuesCache.Get($"_search_{searchQuery}");
            if (cached.Result == CacheResult.CacheHit)
                return cached.Value;
            var response = await _httpClient.GetAsync($"/venue?search={searchQuery}");
            var result = await response.Content.ReadFromJsonAsync<Venue[]>();
            this._venuesCache.Set($"_search_{searchQuery}", result);
            return result;
        }

        public async Task<Venue> GetVenueAsync(string id)
        {
            var cached = this._venueCache.Get(id);
            if (cached.Result == CacheResult.CacheHit)
                return cached.Value;
            var response = await _httpClient.GetAsync("/venue/" + id);
            var result = await response.Content.ReadFromJsonAsync<Venue>();
            this._venueCache.Set(id, result);
            return result;
        }

        public Task<HttpResponseMessage> PutVenueAsync(Venue venue)
        {
            this._venueCache.Remove(venue.Id);
            this._venuesCache.Clear();
            return _httpClient.PutAsJsonAsync("/venue/" + venue.Id, venue);
        }

        public async Task<HttpResponseMessage> PutVenueBannerAsync(string id, string url)
        {
            var response = await _httpClient.GetAsync(url);
            var stream = await response.Content.ReadAsStreamAsync();
            this._venueCache.Remove(id);
            this._venuesCache.Clear();
            return await PutVenueBannerAsync(id, stream, response.Content.Headers.ContentType);
        }

        public Task<HttpResponseMessage> PutVenueBannerAsync(string id, Stream stream, MediaTypeHeaderValue mediaType)
        {
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = mediaType;
            this._venueCache.Remove(id);
            this._venuesCache.Clear();
            return _httpClient.PutAsync("/venue/" + id + "/media", streamContent);
        }

        public Task<HttpResponseMessage> DeleteVenueAsync(string id)
        {
            this._venueCache.Remove(id);
            this._venuesCache.Clear();
            return _httpClient.DeleteAsync("/venue/" + id);
        }

        public Task<HttpResponseMessage> OpenVenueAsync(string id, DateTime until)
        {
            this._venueCache.Remove(id);
            this._venuesCache.Clear();
            return _httpClient.PostAsJsonAsync($"/venue/{id}/open", until);
        }

        public Task<HttpResponseMessage> CloseVenueAsync(string id, DateTime until)
        {
            this._venueCache.Remove(id);
            this._venuesCache.Clear();
            return _httpClient.PostAsJsonAsync($"/venue/{id}/close", until);
        }

        public Task<HttpResponseMessage> ApproveAsync(string id, bool approval = true)
        {
            this._venueCache.Remove(id);
            this._venuesCache.Clear();
            return _httpClient.PutAsJsonAsync($"/venue/{id}/approved", approval);
        }

    }
}