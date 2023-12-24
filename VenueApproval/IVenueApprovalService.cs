using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueRendering;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.People
{
    public interface IVenueApprovalService
    {

        Task<BroadcastReceipt> SendForApproval(Venue venue, string bannerUrl, VenueRenderFlags renderFlags);

        Task<bool> ApproveVenueAsync(Venue venue);

        Task<bool> HandleComponentInteractionAsync(SocketMessageComponent context);

    }
}