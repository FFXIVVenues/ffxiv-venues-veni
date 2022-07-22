using Discord;
using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class DescriptionEntryState : IState
    {
        public Task Init(MessageContext c)
        {
            c.Conversation.RegisterMessageHandler(this.OnMessageReceived);
            return c.RespondAsync(MessageRepository.AskForDescriptionMessage.PickRandom(),
                new ComponentBuilder()
                    .WithButton("Skip", c.Conversation.RegisterComponentHandler(cm => {
                        if (c.Conversation.GetItem<bool>("modifying"))
                            return c.Conversation.ShiftState<ConfirmVenueState>(cm);
                        return c.Conversation.ShiftState<HouseOrApartmentEntryState>(cm);
                    }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
        }

        public Task OnMessageReceived(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");
            venue.Description = c.Message.Content.StripMentions().AsListOfParagraphs();
            if (c.Conversation.GetItem<bool>("modifying"))
                return c.Conversation.ShiftState<ConfirmVenueState>(c);
            return c.Conversation.ShiftState<HouseOrApartmentEntryState>(c);
        }

    }
}
