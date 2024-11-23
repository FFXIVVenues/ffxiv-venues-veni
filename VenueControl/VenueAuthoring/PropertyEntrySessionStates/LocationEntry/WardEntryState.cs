using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.LocationEntry;

class WardEntrySessionState(VenueAuthoringContext authoringContext) : ISessionState<VenueAuthoringContext>
{
    public Task EnterState(VeniInteractionContext interactionContext)
    {
        interactionContext.RegisterMessageHandler(this.OnMessageReceived);
        return interactionContext.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {MessageRepository.AskForWardMessage.PickRandom()}",
            new ComponentBuilder()
                .WithBackButton(interactionContext)
                .Build());
    }

    private Task OnMessageReceived(MessageVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        var match = new Regex("\\b\\d+\\b").Match(c.Interaction.Content.StripMentions());

        if (!match.Success || !ushort.TryParse(match.Value, out var ward) || ward < 1 || ward > 30)
            return c.Interaction.Channel.SendMessageAsync("Sorry, I didn't understand that, please enter a number between 1 and 30.");

        venue.Location.Ward = ward;

        var locationType = c.Session.GetItem<string>(SessionKeys.LOCATION_TYPE);
        if (locationType == "house" || locationType == "room")
            return c.MoveSessionToStateAsync<PlotEntrySessionState, VenueAuthoringContext>(authoringContext);
        else
            return c.MoveSessionToStateAsync<IsSubdivisionEntrySessionState, VenueAuthoringContext>(authoringContext);
    }
}