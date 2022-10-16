using FFXIVVenues.Veni.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Services
{
    internal interface IApiService
    {
        Task<IEnumerable<Venue>> GetAllVenuesAsync();
        Task<IEnumerable<Venue>> GetAllVenuesAsync(ulong forContact);
        Task<IEnumerable<Venue>> GetAllVenuesAsync(string searchTerm);
        Task<IEnumerable<Venue>> GetApprovedVenuesAsync();
        Task<IEnumerable<Venue>> GetUnapprovedVenuesAsync();
        Task<IEnumerable<Venue>> GetOpenVenuesAsync();
        Task<Venue> GetVenueAsync(string id);
        Task<HttpResponseMessage> PutVenueAsync(Venue venue);
        Task<HttpResponseMessage> PutVenueBannerAsync(string id, string url);
        Task<HttpResponseMessage> PutVenueBannerAsync(string id, Stream stream, MediaTypeHeaderValue mediaType);
        Task<HttpResponseMessage> DeleteVenueAsync(string id);
        Task<HttpResponseMessage> OpenVenueAsync(string id);
        Task<HttpResponseMessage> CloseVenueAsync(string id, DateTime until);
        Task<HttpResponseMessage> ApproveAsync(string id, bool approval = true);
    }
}