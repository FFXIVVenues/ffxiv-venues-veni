using Discord;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class WebsiteEntryState : IState
    {
        public Task Init(MessageContext c)
        {
            c.Conversation.RegisterMessageHandler(this.OnMessageReceived);
            return c.RespondAsync(MessageRepository.AskForWebsiteMessage.PickRandom(),
                new ComponentBuilder()
                    .WithButton("Skip", c.Conversation.RegisterComponentHandler(cm => 
                    {
                        if (c.Conversation.GetItem<bool>("modifying"))
                            return c.Conversation.ShiftState<ConfirmVenueState>(cm);
                        return c.Conversation.ShiftState<HaveScheduleEntryState>(cm);
                    }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
        }

        public Task OnMessageReceived(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");
            if (new Regex("\\bskip\\b").IsMatch(c.Message.Content.ToLower()))
            {
                if (c.Conversation.GetItem<bool>("modifying"))
                    return c.Conversation.ShiftState<ConfirmVenueState>(c);
                return c.Conversation.ShiftState<DiscordEntryState>(c);
            }

            var rawWebsiteString = c.Message.Content.StripMentions();
            if (!new Regex("^https?://").IsMatch(rawWebsiteString))
                rawWebsiteString = "https://" + rawWebsiteString;

            if (!Uri.TryCreate(rawWebsiteString, UriKind.Absolute, out var website))
            {
                c.RespondAsync("Sorry, that doesn't look like a valid website address.");
                return Task.CompletedTask;
            }

            venue.Website = website;

            if (c.Conversation.GetItem<bool>("modifying"))
                return c.Conversation.ShiftState<ConfirmVenueState>(c);

            return c.Conversation.ShiftState<DiscordEntryState>(c);
        }
    }
}
