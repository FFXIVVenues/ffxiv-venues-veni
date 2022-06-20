using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class NsfwEntryState : IState
    {

        public Task Enter(MessageContext c) =>
            c.SendMessageAsync(MessageRepository.AskForNsfwMessage.PickRandom());

        public Task Handle(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");

            if (c.Prediction.TopIntent == IntentNames.Response.Yes)
                venue.Nsfw = true;
            else if (c.Prediction.TopIntent == IntentNames.Response.No)
                venue.Nsfw = false;
            else
                return c.SendMessageAsync(MessageRepository.DontUnderstandResponses.PickRandom());

            if (c.Conversation.GetItem<bool>("modifying"))
                return c.Conversation.ShiftState<ConfirmVenueState>(c);

            return c.Conversation.ShiftState<WebsiteEntryState>(c);
        }

    }
}
