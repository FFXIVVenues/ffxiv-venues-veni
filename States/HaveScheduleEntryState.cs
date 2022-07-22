using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;

namespace FFXIVVenues.Veni.States
{
    class HaveScheduleEntryState : IState
    {

        private static string[] _messages = new[]
        {
            "Oki! Do you have a weekly opening schedule?",
            "Great! Does your venue have a weekly schedule for opening?",
            "Is your venue generally open at the same days and times every week?",
        };

        public Task Init(MessageContext c)
        {
            return c.RespondAsync(_messages.PickRandom(),
                new ComponentBuilder()
                    .WithButton("Yes, we have a set weekly schedule",
                        c.Conversation.RegisterComponentHandler(cm => 
                            cm.Conversation.ShiftState<TimeZoneEntryState>(cm), 
                        ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("No, we don't have a set weekly schedule",
                        c.Conversation.RegisterComponentHandler(cm =>
                        {
                            var venue = c.Conversation.GetItem<Venue>("venue");
                            venue.Openings = new();

                            if (c.Conversation.GetItem<bool>("modifying"))
                                return c.Conversation.ShiftState<ConfirmVenueState>(c);
                            return c.Conversation.ShiftState<BannerInputState>(c);
                        }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
        }

    }
}
