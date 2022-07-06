using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class DiscordEntryState : IState
    {

        static HttpClient _discordClient = new HttpClient();
        static Regex _discordPattern = new Regex(@"(https?:\/\/)?(www\.)?((discord(app)?(\.com|\.io)(\/invite)?)|(discord\.gg))\/(\w+)");

        public Task Init(MessageContext c) =>
            c.RespondAsync(MessageRepository.AskForDiscordMessage.PickRandom());

        public async Task OnMessageReceived(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");
            string message = c.Message.Content.StripMentions();
            if (message.ToLower().MatchesAnyRegex("skip", "no")
                || message.ToLower().MatchesAnyPhrase("don't have one", "not available", "not public", "don't want to give"))
            {
                if (c.Conversation.GetItem<bool>("modifying"))
                {
                    await c.Conversation.ShiftState<ConfirmVenueState>(c);
                    return;
                }

                await c.Conversation.ShiftState<HaveScheduleEntryState>(c);
                return;
            }

            var rawDiscordString = c.Message.Content.StripMentions();
            if (!new Regex("^https?://").IsMatch(rawDiscordString))
                rawDiscordString = "https://" + rawDiscordString;

            var match = _discordPattern.Match(rawDiscordString);
            if (!match.Success)
            {
                await c.RespondAsync("That doesn't look like a valid Discord invite to me. :think:");
                return;
            }

            var inviteCode = match.Groups[9].ToString();
            var responseMessage = await _discordClient.GetAsync($"https://discordapp.com/api/invite/{inviteCode}");

            if (!responseMessage.IsSuccessStatusCode)
            {
                await c.RespondAsync("I tried that invite link but it seems to be invalid. :cry:");
                return;
            }

            var response = await responseMessage.Content.ReadAsStreamAsync();
            var invite = await JsonSerializer.DeserializeAsync<DiscordInvite>(response);

            if (invite.expires_at != null)
            {
                await c.RespondAsync($"That invite link is not permanent, it'll expire in {invite.expires_at.Value:m}");
                return;
            }

            venue.Discord = new Uri(rawDiscordString);

            if (c.Conversation.GetItem<bool>("modifying"))
            {
                await c.Conversation.ShiftState<ConfirmVenueState>(c);
                return;
            }

            await c.Conversation.ShiftState<HaveScheduleEntryState>(c);
        }

    }

    public class DiscordInvite
    {
        public DateTime? expires_at { get; set; }
    }
}
