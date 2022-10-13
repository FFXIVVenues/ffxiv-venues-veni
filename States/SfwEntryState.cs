using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using FFXIVVenues.Veni.Models;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.States
{
    class SfwEntryState : IState
    {
        public Task Enter(InteractionContext c)
        {
            return c.Interaction.RespondAsync(MessageRepository.AskForSfwMessage.PickRandom(), new ComponentBuilder()
                .WithBackButton(c)
                .WithButton("Yes, it's safe on entry", c.Session.RegisterComponentHandler(cm =>
                {
                    var venue = cm.Session.GetItem<Venue>("venue");
                    venue.Sfw = true;
                    if (cm.Session.GetItem<bool>("modifying"))
                        return cm.Session.MoveStateAsync<ConfirmVenueState>(cm);
                    return cm.Session.MoveStateAsync<CategoryEntryState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("No, we're openly NSFW", c.Session.RegisterComponentHandler(cm =>
                {
                    var venue = cm.Session.GetItem<Venue>("venue");
                    venue.Sfw = false;
                    if (cm.Session.GetItem<bool>("modifying"))
                        return cm.Session.MoveStateAsync<ConfirmVenueState>(cm);
                    return cm.Session.MoveStateAsync<CategoryEntryState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
        }

    }

}
