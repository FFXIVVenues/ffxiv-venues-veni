using System.Threading.Tasks;
using FFXIVVenues.Utils;
using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;

namespace FFXIVVenues.Veni.States
{
    class SfwEntryState : IState
    {
        public Task Enter(MessageContext c) =>
            c.SendMessageAsync(MessageRepository.AskForSfwMessage.PickRandom());

        public Task Handle(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");

            if (c.Prediction.TopIntent == IntentNames.Response.Yes)
                venue.Sfw = true;
            else if (c.Prediction.TopIntent == IntentNames.Response.No)
                venue.Sfw = false;
            else
                return c.SendMessageAsync(MessageRepository.DontUnderstandResponses.PickRandom());

            return c.Conversation.ShiftState<NsfwEntryState>(c);
        }
    }

}
