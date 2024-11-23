using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueRendering;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.SessionStates
{
    // Currently is only used for editing from Confirm session state
    class EditVenueSessionState(IAuthorizer authorizer, IVenueRenderer venueRenderer) : ISessionState
    {
        private readonly IAuthorizer _authorizer = authorizer;

        public Task EnterState(VeniInteractionContext interactionContext)
        {
            interactionContext.Session.SetEditing(true);
            var venue = interactionContext.Session.GetVenue();

            if (interactionContext.Interaction.IsDM)
                return interactionContext.Interaction.RespondAsync(MessageRepository.EditVenueMessage.PickRandom(), 
                    component: venueRenderer.RenderEditComponents(venue, interactionContext.Interaction.User.Id).Build());

            var @warningEmbed = new EmbedBuilder
            {
                Color = Color.Red,
                Description = MessageRepository.MentionOrReplyToMeMessage.PickRandom()
            };
            return interactionContext.Interaction.RespondAsync(MessageRepository.EditVenueMessage.PickRandom(),
              embed: @warningEmbed.Build(),
              component: venueRenderer.RenderEditComponents(venue, interactionContext.Interaction.User.Id).Build());
        }

    }
}
