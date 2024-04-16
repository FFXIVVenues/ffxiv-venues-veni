using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates;

class NameEntrySessionState : ISessionState
{
    public Task Enter(VeniInteractionContext c)
    {
        c.Session.RegisterMessageHandler(this.OnMessageReceived);
            
        var isDm = c.Interaction.Channel is IDMChannel;
        return c.Interaction.RespondAsync(isDm
            ? VenueControlStrings.AskForNameDirectMessage
            : VenueControlStrings.AskForNameMessage);
    }

    public Task OnMessageReceived(MessageVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        venue.Name = c.Interaction.Content.StripMentions();
        if (c.Session.InEditing())
            return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);
        return c.Session.MoveStateAsync<DescriptionEntrySessionState>(c);
    }
}