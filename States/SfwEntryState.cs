using System.Threading.Tasks;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;

namespace FFXIVVenues.Veni.States
{
    class SfwEntryState : IState
    {
        public Task Init(MessageContext c)
        {
            c.Conversation.RegisterMessageHandler(this.OnMessageReceived);
            return c.RespondAsync(MessageRepository.AskForSfwMessage.PickRandom());
        }

        public Task OnMessageReceived(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");

            if (c.Prediction.TopIntent == IntentNames.Response.Yes)
                venue.Sfw = true;
            else if (c.Prediction.TopIntent == IntentNames.Response.No)
                venue.Sfw = false;
            else
                return c.RespondAsync(MessageRepository.DontUnderstandResponses.PickRandom());

            if (c.Conversation.GetItem<bool>("modifying"))
                return c.Conversation.ShiftState<ConfirmVenueState>(c);
            return c.Conversation.ShiftState<TypeEntryState>(c);
        }
    }

}
