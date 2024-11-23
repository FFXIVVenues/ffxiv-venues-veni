using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using Newtonsoft.Json.Linq;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates;

class ManagerEntrySessionState(IAuthorizer authorizer, VenueAuthoringContext authoringContext) : ISessionState<VenueAuthoringContext>
{
    public Task EnterState(VeniInteractionContext interactionContext)
    {
        var venue = interactionContext.Session.GetVenue();

        if (!authorizer.Authorize(interactionContext.Interaction.User.Id, Permission.EditManagers, venue).Authorized)
            return interactionContext.MoveSessionToStateAsync<ConfirmVenueSessionState, VenueAuthoringContext>(authoringContext);

        interactionContext.RegisterMessageHandler(this.OnMessageReceived);
        return interactionContext.Interaction.RespondAsync("Who is/are the manager(s)? :heart:",
            new ComponentBuilder()
                .WithBackButton(interactionContext)
                .WithSkipButton<ConfirmVenueSessionState, ConfirmVenueSessionState>(interactionContext, authoringContext)
                .Build());
    }

    public Task OnMessageReceived(MessageVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();

        var regex = new Regex("[0-9]{17,}");
        var input = c.Interaction.Content.StripMentions(c.Client.CurrentUser.Id);
        var discordIds = regex.Matches(input).Select(m => m.Value).ToList();

        if (discordIds is not { Count: > 0})
            return c.Interaction.Channel.SendMessageAsync(MessageRepository.DontUnderstandResponses.PickRandom());

        venue.Managers = discordIds;
        return c.MoveSessionToStateAsync<ConfirmVenueSessionState, VenueAuthoringContext>(authoringContext);
    }

}