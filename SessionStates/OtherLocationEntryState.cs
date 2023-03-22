using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.Session;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.SessionStates;

internal class OtherLocationEntrySessionState : ISessionState
{
    
    public Task Enter(VeniInteractionContext c)
    {
        c.Session.RegisterMessageHandler(this.MessageHandler);
        return c.Interaction.RespondAsync("Ooo, interesting! In as few characters as possible, where is your venue **located**? 🥰", new ComponentBuilder()
            .WithBackButton(c).Build());
    }

    private Task MessageHandler(MessageVeniInteractionContext c)
    {
        var venue = c.Session.GetItem<Venue>("venue");
        venue.Location = new Location { Override = c.Interaction.Content.StripMentions() };
            
        if (c.Session.GetItem<bool>("modifying"))
            return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);

        return c.Session.MoveStateAsync<SfwEntrySessionState>(c);
    }
    
}