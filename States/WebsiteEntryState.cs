using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Models;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class WebsiteEntryState : IState
    {
        public Task Enter(InteractionContext c)
        {
            c.Session.RegisterMessageHandler(this.OnMessageReceived);
            return c.Interaction.RespondAsync(MessageRepository.AskForWebsiteMessage.PickRandom(),
                new ComponentBuilder()
                    .WithBackButton(c)
                    .WithNextButton<DiscordEntryState, ConfirmVenueState>(c)
                    .Build());
        }

        public Task OnMessageReceived(MessageInteractionContext c)
        {
            var venue = c.Session.GetItem<Venue>("venue");
            if (new Regex("\\bskip\\b").IsMatch(c.Interaction.Content.ToLower()))
            {
                if (c.Session.GetItem<bool>("modifying"))
                    return c.Session.MoveStateAsync<ConfirmVenueState>(c);
                return c.Session.MoveStateAsync<DiscordEntryState>(c);
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

            if (c.Session.GetItem<bool>("modifying"))
                return c.Session.MoveStateAsync<ConfirmVenueState>(c);

            return c.Session.MoveStateAsync<DiscordEntryState>(c);
        }
    }
}
