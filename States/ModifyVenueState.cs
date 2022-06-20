using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class ModifyVenueState : IState
    {

        public Task Enter(MessageContext c) =>
            c.SendMessageAsync($"What would you like to change?\nThe name, description, location, schedule, (N)SFW status, website or discord?");

        public Task Handle(MessageContext c)
        {
            c.Conversation.SetItem("modifying", true);

            if (c.Message.Content.StripMentions().MatchesAnyPhrase("name"))
                return c.Conversation.ShiftState<NameEntryState>(c);
            else if (c.Message.Content.StripMentions().MatchesAnyPhrase("description"))
                return c.Conversation.ShiftState<DescriptionEntryState>(c);
            else if (c.Message.Content.StripMentions().MatchesAnyPhrase("location"))
                return c.Conversation.ShiftState<HouseOrApartmentEntryState>(c);
            else if (c.Message.Content.StripMentions().MatchesAnyPhrase("schedule", "days", "times", "timing", "openings"))
                return c.Conversation.ShiftState<AskIfConsistentTimeEntryState>(c);
            else if (c.Message.Content.StripMentions().MatchesAnyPhrase("nsfw", "sfw", "status"))
                return c.Conversation.ShiftState<SfwEntryState>(c);
            else if (c.Message.Content.StripMentions().MatchesAnyPhrase("website"))
                return c.Conversation.ShiftState<WebsiteEntryState>(c);
            else if (c.Message.Content.StripMentions().MatchesAnyPhrase("discord"))
                return c.Conversation.ShiftState<DiscordEntryState>(c);
            else
                return c.SendMessageAsync(MessageRepository.DontUnderstandResponses.PickRandom());
        }
    }
}
