using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates
{
    class WebsiteEntrySessionState(VenueAuthoringContext authoringContext) : ISessionState<VenueAuthoringContext>
    {
        public Task EnterState(VeniInteractionContext interactionContext)
        {
            interactionContext.RegisterMessageHandler(this.OnMessageReceived);

            return interactionContext.Interaction.RespondAsync(MessageRepository.AskForWebsiteMessage.PickRandom(),
                new ComponentBuilder()
                    .WithBackButton(interactionContext)
                    .WithButton("No website", interactionContext.RegisterComponentHandler(OnNoWebsite, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .Build());
        }

        private Task OnMessageReceived(MessageVeniInteractionContext c)
        {
            var venue = c.Session.GetVenue();
            if (new Regex("\\bskip\\b").IsMatch(c.Interaction.Content.ToLower()))
            {
                if (c.Session.InEditing())
                    return c.MoveSessionToStateAsync<ConfirmVenueSessionState, VenueAuthoringContext>(authoringContext);
                return c.MoveSessionToStateAsync<DiscordEntrySessionState, VenueAuthoringContext>(authoringContext);
            }

            var rawWebsiteString = c.Interaction.Content.StripMentions();
            if (!new Regex("^https?://").IsMatch(rawWebsiteString))
                rawWebsiteString = "https://" + rawWebsiteString;

            if (!Uri.TryCreate(rawWebsiteString, UriKind.Absolute, out var website))
            {
                c.Interaction.Channel.SendMessageAsync("Sorry, that doesn't look like a valid website address.");
                return Task.CompletedTask;
            }

            venue.Website = website;

            if (c.Session.InEditing())
                return c.MoveSessionToStateAsync<ConfirmVenueSessionState, VenueAuthoringContext>(authoringContext);

            return c.MoveSessionToStateAsync<DiscordEntrySessionState, VenueAuthoringContext>(authoringContext);
        }
        
        private Task OnNoWebsite(ComponentVeniInteractionContext context)
        {
            var venue = context.Session.GetVenue();
            venue.Website = null;

            if (context.Session.InEditing()) 
                return context.MoveSessionToStateAsync<ConfirmVenueSessionState, VenueAuthoringContext>(authoringContext);
            
            return context.MoveSessionToStateAsync<DiscordEntrySessionState, VenueAuthoringContext>(authoringContext);
        }
    }
}
