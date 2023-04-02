using System;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.SessionStates;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Create : IntentHandler
    {

        private const string CREATE_VALUE_KEY = "venue";

        public override async Task Handle(VeniInteractionContext context)
        {
            context.Session.SetItem("isNewVenue", true);

            var venue = new Venue();
            venue.Managers.Add(context.Interaction.User.Id.ToString());
            context.Session.Data.AddOrUpdate(CREATE_VALUE_KEY, (s, v) => v, (s, e, v) => v, venue);
            if (context.Interaction.IsDM)
                await context.Interaction.RespondAsync(MessageRepository.CreateVenueMessage.PickRandom());
            else
                await context.Interaction.RespondAsync(MessageRepository.CreateVenueMessage.PickRandom(),
                    embed: new EmbedBuilder {
                        Color = Color.Red,
                        Description = MessageRepository.MentionOrReplyToMeMessage.PickRandom()
                    }.Build());
            await context.Session.MoveStateAsync<NameEntrySessionState>(context);
        }

    }
}
