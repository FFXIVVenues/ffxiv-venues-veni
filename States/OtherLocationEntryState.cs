using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;
using Venue = FFXIVVenues.Veni.Models.Venue;

namespace FFXIVVenues.Veni.States;

internal class OtherLocationEntryState : IState
{
    
    public Task Enter(InteractionContext c)
    {
        c.Session.RegisterMessageHandler(this.MessageHandler);
        return c.Interaction.RespondAsync("Ooo, interesting! In as few characters as possible, where is your venue **located**? 🥰", new ComponentBuilder()
            .WithBackButton(c).Build());
    }

    private Task MessageHandler(MessageInteractionContext c)
    {
        var venue = c.Session.GetItem<Venue>("venue");
        venue.Location = new Location { Override = c.Interaction.Content.StripMentions() };
            
        if (c.Session.GetItem<bool>("modifying"))
            return c.Session.MoveStateAsync<ConfirmVenueState>(c);

        return c.Session.MoveStateAsync<SfwEntryState>(c);
    }
    
}