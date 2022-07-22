using Discord;
using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class IsSubdivisionEntryState : IState
    {
        public Task Init(MessageContext c)
        {
            return c.RespondAsync(MessageRepository.AskForSubdivisionMessage.PickRandom(), new ComponentBuilder()
                .WithButton("Yes, it's subdivision", c.Conversation.RegisterComponentHandler(cm =>
                {
                    var venue = c.Conversation.GetItem<Venue>("venue");
                    venue.Location.Subdivision = true;
                    return c.Conversation.ShiftState<ApartmentEntryState>(cm);
                }, ComponentPersistence.ClearRow))
                .WithButton("No, the first division", c.Conversation.RegisterComponentHandler(cm =>
                {
                    var venue = c.Conversation.GetItem<Venue>("venue");
                    venue.Location.Subdivision = false;
                    return c.Conversation.ShiftState<ApartmentEntryState>(cm);
                }, ComponentPersistence.ClearRow))
                .Build());
        }
    }
}
