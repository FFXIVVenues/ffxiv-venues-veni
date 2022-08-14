using Discord;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class DescriptionEntryState : IState
    {
        public Task Init(InteractionContext c)
        {
            c.Session.RegisterMessageHandler(this.OnMessageReceived);
            return c.Interaction.RespondAsync(MessageRepository.AskForDescriptionMessage.PickRandom(),
                new ComponentBuilder()
                    .WithButton("Skip", c.Session.RegisterComponentHandler(cm => {
                        if (cm.Session.GetItem<bool>("modifying"))
                            return cm.Session.ShiftState<ConfirmVenueState>(cm);
                        return cm.Session.ShiftState<HouseOrApartmentEntryState>(cm);
                    }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
        }

        public Task OnMessageReceived(MessageInteractionContext c)
        {
            var venue = c.Session.GetItem<Venue>("venue");
            venue.Description = c.Interaction.Content.StripMentions().AsListOfParagraphs();
            if (c.Session.GetItem<bool>("modifying"))
                return c.Session.ShiftState<ConfirmVenueState>(c);
            return c.Session.ShiftState<HouseOrApartmentEntryState>(c);
        }

    }
}
