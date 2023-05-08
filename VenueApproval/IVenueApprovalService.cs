using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.People
{
    public interface IVenueApprovalService
    {

        Task<List<BroadcastMessage>> SendForApproval(Venue venue, string bannerUrl);

        Task<bool> ApproveVenueAsync(Venue venue);

        Task<bool> HandleComponentInteractionAsync(SocketMessageComponent context);

    }
}