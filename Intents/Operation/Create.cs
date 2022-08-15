using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Create : IntentHandler
    {

        private const string CREATE_VALUE_KEY = "venue";

        public override async Task Handle(InteractionContext context)
        {
            context.Session.SetItem("isNewVenue", true);

            var venue = new Venue();
            venue.Managers.Add(context.Interaction.User.Id.ToString());
            context.Session.Data.AddOrUpdate(CREATE_VALUE_KEY, (s, v) => v, (s, e, v) => v, venue);
            if (context.Interaction.IsDM)
                await context.Interaction.RespondAsync(MessageRepository.CreateVenueMessage.PickRandom());
            else
                await context.Interaction.RespondAsync(MessageRepository.CreateVenueMessage.PickRandom()
                                                        + Environment.NewLine + MessageRepository.MentionOrReplyToMeMessage.PickRandom());
            await context.Session.ShiftState<NameEntryState>(context);
        }

    }
}
