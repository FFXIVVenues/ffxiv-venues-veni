using System.Threading.Tasks;
using Discord;
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
            return c.RespondAsync(MessageRepository.AskForSfwMessage.PickRandom(), new ComponentBuilder()
                .WithButton("Yes, it's safe on entry", c.Conversation.RegisterComponentHandler(cm =>
                {
                    var venue = c.Conversation.GetItem<Venue>("venue");
                    venue.Sfw = true;
                    if (c.Conversation.GetItem<bool>("modifying"))
                        return c.Conversation.ShiftState<ConfirmVenueState>(c);
                    return c.Conversation.ShiftState<TypeEntryState>(c);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("No, we're openly NSFW", c.Conversation.RegisterComponentHandler(cm =>
                {
                    var venue = c.Conversation.GetItem<Venue>("venue");
                    venue.Sfw = false;
                    if (c.Conversation.GetItem<bool>("modifying"))
                        return c.Conversation.ShiftState<ConfirmVenueState>(c);
                    return c.Conversation.ShiftState<TypeEntryState>(c);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
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
