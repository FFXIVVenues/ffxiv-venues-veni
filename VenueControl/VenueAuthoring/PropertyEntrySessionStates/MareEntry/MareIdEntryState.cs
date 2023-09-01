using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.MareEntry;

class MareIdEntryState : ISessionState
{

    public Task Enter(VeniInteractionContext c)
    {
        c.Session.RegisterMessageHandler(this.OnMessageReceived);
        var isDm = c.Interaction.Channel is IDMChannel;
        return c.Interaction.RespondAsync(isDm ? 
                VenueControlStrings.AskForMareIdDirectMessage :
                VenueControlStrings.AskForMareIdMessage,
            new ComponentBuilder()
                .WithBackButton(c)
                .Build());
    }

    public Task OnMessageReceived(MessageVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        
        venue.MareCode = c.Interaction.Content.StripMentions();
        return c.Session.MoveStateAsync<MarePasswordEntryState>(c);
    }


}