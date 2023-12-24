using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;
using NChronicle.Core.Interfaces;

namespace FFXIVVenues.Veni.Api
{
    internal class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IChronicle _chronicle;
        private readonly RollingCache<Venue> _venueCache;
        private readonly RollingCache<Venue[]> _venuesCache;

        public ApiService(HttpClient httpClient, IChronicle chronicle)
        {
            this._httpClient = httpClient;
            this._chronicle = chronicle;
            this._venueCache = new(60*1000, 10 * 60 * 1_000);
            this._venuesCache = new(60*1000, 10 * 60 * 1_000);
        }

        public async Task<IEnumerable<Venue>> GetAllVenuesAsync()
        {
            var cached = this._venuesCache.Get("*");
            if (cached.Result == CacheResult.CacheHit)
            {
                this._chronicle.Debug($"ApiService.GetAllVenues (Cache: Hit)");
                return cached.Value;
            }
            this._chronicle.Debug($"ApiService.GetAllVenues (Cache: Miss)");
            
            var response = await _httpClient.GetAsync($"/venue");
            var result = await response.Content.ReadFromJsonAsync<Venue[]>();
            this._venuesCache.Set("*", result);    
            return result;
        }

        public async Task<IEnumerable<Venue>> GetAllVenuesAsync(ulong forContact)
        {
            var cached = this._venuesCache.Get(forContact.ToString());
            if (cached.Result == CacheResult.CacheHit)
            {
                this._chronicle.Debug($"ApiService.GetAllVenues(forContact) `{forContact}` (Cache: Hit)");
                return cached.Value;
            }
            this._chronicle.Debug($"ApiService.GetAllVenues(forContact) `{forContact}` (Cache: Miss)");
            
            var response = await _httpClient.GetAsync($"/venue?manager={forContact}");
            var result = await response.Content.ReadFromJsonAsync<Venue[]>();
            this._venuesCache.Set(forContact.ToString(), result);
            return result;
        }

        public async Task<IEnumerable<Venue>> GetOpenVenuesAsync()
        {
            var cached = this._venuesCache.Get("_open_");
            if (cached.Result == CacheResult.CacheHit)
            {
                this._chronicle.Debug($"ApiService.GetOpenVenues (Cache: Hit)");
                return cached.Value;
            }
            this._chronicle.Debug($"ApiService.GetOpenVenues (Cache: Miss)");
            
            var response = await _httpClient.GetAsync($"/venue?open=true");
            var result = await response.Content.ReadFromJsonAsync<Venue[]>();
            this._venuesCache.Set("_open_", result);
            return result;
        }

        public async Task<IEnumerable<Venue>> GetApprovedVenuesAsync()
        {
            var cached = this._venuesCache.Get("_approved_");
            if (cached.Result == CacheResult.CacheHit)
            {
                this._chronicle.Debug($"ApiService.GetApprovedVenues (Cache: Hit)");
                return cached.Value;
            }
            this._chronicle.Debug($"ApiService.GetApprovedVenues (Cache: Miss)");
            
            var response = await _httpClient.GetAsync($"/venue?approved=true");
            var result = await response.Content.ReadFromJsonAsync<Venue[]>();
            this._venuesCache.Set("_approved_", result);
            return result;
        }
        
        public async Task<IEnumerable<Venue>> GetUnapprovedVenuesAsync()
        {
            var cached = this._venuesCache.Get("_unapproved_");
            if (cached.Result == CacheResult.CacheHit)
            {
                this._chronicle.Debug($"ApiService.GetUnapprovedVenues (Cache: Hit)");
                return cached.Value;
            }
            this._chronicle.Debug($"ApiService.GetUnapprovedVenues (Cache: Miss)");
            
            var response = await _httpClient.GetAsync($"/venue?approved=false");
            var result = await response.Content.ReadFromJsonAsync<Venue[]>();
            this._venuesCache.Set("_unapproved_", result);
            return result;
        }

        public async Task<IEnumerable<Venue>> GetAllVenuesAsync(string searchQuery)
        {
            var cached = this._venuesCache.Get($"_search_{searchQuery}");
            if (cached.Result == CacheResult.CacheHit)
            {
                this._chronicle.Debug($"ApiService.GetAllVenues(searchQuery) `{searchQuery}` (Cache: Hit)");
                return cached.Value;
            }
            this._chronicle.Debug($"ApiService.GetAllVenues(searchQuery) `{searchQuery}` (Cache: Miss)");
            
            var response = await _httpClient.GetAsync($"/venue?search={searchQuery}");
            var result = await response.Content.ReadFromJsonAsync<Venue[]>();
            this._venuesCache.Set($"_search_{searchQuery}", result);
            return result;
        }

        public async Task<Venue> GetVenueAsync(string id, bool recordView = false)
        {
            var cached = this._venueCache.Get(id);
            if (cached.Result == CacheResult.CacheHit)
            {
                this._chronicle.Debug($"ApiService.GetVenue `{id}` (Cache: Hit)");
                return cached.Value;
            }
            this._chronicle.Debug($"ApiService.GetVenue `{id}` (Cache: Miss)");
            
            var response = await _httpClient.GetAsync("/venue/" + id + "?recordView=" + recordView);
            var result = await response.Content.ReadFromJsonAsync<Venue>();
            this._venueCache.Set(id, result);
            return result;
        }

        public async Task<HttpResponseMessage> PutVenueAsync(Venue venue)
        {
            this._chronicle.Debug($"ApiService.PutVenue `{venue.Id}`");
            this._venueCache.Remove(venue.Id);
            this._venuesCache.Clear();
            var response = await _httpClient.PutAsJsonAsync("/venue/" + venue.Id, venue);
            if (!response.IsSuccessStatusCode)
                this._chronicle.Debug($"ApiService.PutVenue `{venue.Id}` Failed request: {response.StatusCode}\n{await response.Content.ReadAsStringAsync()}");
            return response;
        }

        public async Task<HttpResponseMessage> PutVenueBannerAsync(string id, string url)
        {
            var response = await _httpClient.GetAsync(url);
            var stream = await response.Content.ReadAsStreamAsync();
            this._venueCache.Remove(id);
            this._venuesCache.Clear();
            return await PutVenueBannerAsync(id, stream, response.Content.Headers.ContentType);
        }

        public async Task<HttpResponseMessage> PutVenueBannerAsync(string id, Stream stream, MediaTypeHeaderValue mediaType)
        {
            this._chronicle.Debug($"ApiService.PutVenueBanner `{id}`");
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = mediaType;
            this._venueCache.Remove(id);
            this._venuesCache.Clear();
            var response = await _httpClient.PutAsync("/venue/" + id + "/media", streamContent);
            if (!response.IsSuccessStatusCode)
                this._chronicle.Debug($"ApiService.PutVenueBanner `{id}` Failed request: {response.StatusCode}\n{await response.Content.ReadAsStringAsync()}");
            return response;
        }

        public async Task<HttpResponseMessage> DeleteVenueAsync(string id)
        {
            this._chronicle.Debug($"ApiService.DeleteVenue `{id}`");
            this._venueCache.Remove(id);
            this._venuesCache.Clear();
            var response = await _httpClient.DeleteAsync("/venue/" + id);
            if (!response.IsSuccessStatusCode)
                this._chronicle.Debug($"ApiService.DeleteVenue `{id}` Failed request: {response.StatusCode}\n{await response.Content.ReadAsStringAsync()}");
            return response;
        }

        public async Task<HttpResponseMessage> OpenVenueAsync(string id, DateTimeOffset until)
        {
            this._chronicle.Debug($"ApiService.OpenVenue `{id}`");
            this._venueCache.Remove(id);
            this._venuesCache.Clear();
            var response = await _httpClient.PostAsJsonAsync($"/venue/{id}/open", until);
            if (!response.IsSuccessStatusCode)
                this._chronicle.Debug($"ApiService.OpenVenue `{id}` Failed request: {response.StatusCode}\n{await response.Content.ReadAsStringAsync()}");
            return response;
        }

        public async Task<HttpResponseMessage> CloseVenueAsync(string id, DateTimeOffset until)
        {
            this._chronicle.Debug($"ApiService.CloseVenue `{id}`");
            this._venueCache.Remove(id);
            this._venuesCache.Clear();
            var response = await _httpClient.PostAsJsonAsync($"/venue/{id}/close", until);
            if (!response.IsSuccessStatusCode)
                this._chronicle.Debug($"ApiService.CloseVenue `{id}` Failed request: {response.StatusCode}\n{await response.Content.ReadAsStringAsync()}");
            return response;
        }

        public async Task<HttpResponseMessage> ApproveAsync(string id, bool approval = true)
        {
            this._chronicle.Debug($"ApiService.Approve `{id}`");
            this._venueCache.Remove(id);
            this._venuesCache.Clear();
            var response = await _httpClient.PutAsJsonAsync($"/venue/{id}/approved", approval);
            if (!response.IsSuccessStatusCode)
                this._chronicle.Debug($"ApiService.Approve `{id}` Failed request: {response.StatusCode}\n{await response.Content.ReadAsStringAsync()}");
            return response;
        }

    }
}