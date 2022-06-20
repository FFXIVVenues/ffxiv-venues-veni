using FFXIVVenues.Veni.Api.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Api
{
    internal interface IApiService
    {
        Task<IEnumerable<Venue>> GetAllVenuesAsync();
        Task<IEnumerable<Venue>> GetAllVenuesAsync(ulong forContact);
        Task<Venue> GetVenueAsync(string id);
        Task<HttpResponseMessage> PutVenueAsync(Venue venue);
        Task<HttpResponseMessage> DeleteVenueAsync(string id);
        Task<HttpResponseMessage> OpenVenue(string id);
        Task<HttpResponseMessage> CloseVenue(string id);
    }
}