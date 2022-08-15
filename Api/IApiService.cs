using FFXIVVenues.Veni.Api.Models;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Api
{
    internal interface IApiService
    {
        Task<IEnumerable<Venue>> GetAllVenuesAsync();
        Task<IEnumerable<Venue>> GetAllVenuesAsync(ulong forContact);
        Task<IEnumerable<Venue>> GetAllVenuesAsync(string searchTerm);
        Task<IEnumerable<Venue>> GetOpenVenuesAsync();
        Task<Venue> GetVenueAsync(string id);
        Task<HttpResponseMessage> PutVenueAsync(Venue venue);
        Task<HttpResponseMessage> PutVenueBannerAsync(string id, string url);
        Task<HttpResponseMessage> PutVenueBannerAsync(string id, Stream stream, MediaTypeHeaderValue mediaType);
        Task<HttpResponseMessage> DeleteVenueAsync(string id);
        Task<HttpResponseMessage> OpenVenueAsync(string id);
        Task<HttpResponseMessage> CloseVenueAsync(string id);
        Task<HttpResponseMessage> ApproveAsync(string id, bool approval = true);
    }
}