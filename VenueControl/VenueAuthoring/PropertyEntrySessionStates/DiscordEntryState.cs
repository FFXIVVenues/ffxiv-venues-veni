using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation.Blacklist;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.ScheduleEntry;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates
{
    class DiscordEntrySessionState : ISessionState
    {
        private readonly IRepository db;
        private readonly IDiscordValidator _discordValidator;

        public DiscordEntrySessionState(IRepository db, IDiscordValidator discordValidator)
        {
            this.db = db;
            this._discordValidator = discordValidator;
        }

        public Task Enter(VeniInteractionContext c)
        {
            c.Session.RegisterMessageHandler(this.OnMessageReceived);
            return c.Interaction.RespondAsync(MessageRepository.AskForDiscordMessage.PickRandom(),
                new ComponentBuilder()
                    .WithBackButton(c)
                    .WithSkipButton<HaveScheduleEntrySessionState, ConfirmVenueSessionState>(c)
                .Build());
        }

        public async Task OnMessageReceived(MessageVeniInteractionContext c)
        {
            var venue = c.Session.GetVenue();
            var rawDiscordString = c.Interaction.Content.StripMentions();

            if (!new Regex("^https?://").IsMatch(rawDiscordString))
                rawDiscordString = "https://" + rawDiscordString;

            var (discordValidity, invite) = await this._discordValidator.CheckInviteAsync(rawDiscordString);
            switch (discordValidity)
            {
                case DiscordCheckResult.BadFormat:
                    await c.Interaction.Channel.SendMessageAsync("That doesn't look like a valid Discord invite to me. :thinking:");
                    return;
                case DiscordCheckResult.InvalidInvite:
                    await c.Interaction.Channel.SendMessageAsync("I tried that invite link but it seems to be invalid. :cry:");
                    return;
                case DiscordCheckResult.IsTemporaryInvite:
                    await c.Interaction.Channel.SendMessageAsync(
                        $"That invite link is not permanent, it'll expire on {invite.expires_at.Value:m}.");
                    return;
            }

            if (await db.ExistsAsync<BlacklistEntry>(invite.guild.id))
            {
                await c.Interaction.Channel.SendMessageAsync("This Discord server was blacklisted, please contact staff for further information. 😢" +
                    " Please use a different server or skip this step.");
                return;
            }

            venue.Discord = new Uri(rawDiscordString);

            if (c.Session.GetItem<bool>("modifying"))
            {
                await c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);
                return;
            }
            await c.Session.MoveStateAsync<HaveScheduleEntrySessionState>(c);
        }

        
    }

}
