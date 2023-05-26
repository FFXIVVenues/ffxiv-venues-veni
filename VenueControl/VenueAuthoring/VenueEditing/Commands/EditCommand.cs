using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.ComponentHandlers;
using FFXIVVenues.Veni.VenueRendering;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.Commands
{
    public static class EditCommand
    {

        public const string COMMAND_NAME = "edit";

        internal class Factory : ICommandFactory
        {

            public SlashCommandProperties GetSlashCommand()
            {
                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Edit your venue(s)!")
                    .Build();
            }

        }

        internal class Handler : ICommandHandler
        {
            private readonly IApiService _apiService;
            private readonly IVenueRenderer _venueRenderer;

            public Handler(IApiService apiService, IVenueRenderer venueRenderer)
            {
                this._apiService = apiService;
                this._venueRenderer = venueRenderer;
            }

            public async Task HandleAsync(SlashCommandVeniInteractionContext context)
            {
                var user = context.Interaction.User.Id;
                var venues = await this._apiService.GetAllVenuesAsync(user);

                if (venues == null || !venues.Any())
                {
                    await context.Interaction.RespondAsync("You don't seem to be an assigned manager for any venues. 🤔");
                    return;
                }

                // ReSharper disable once PossibleMultipleEnumeration
                // Enumerating next once for the Any is better than enumerating all on a chance
                venues = venues.ToList();
                if (venues.Count() == 1)
                {
                    var venue = venues.Single();
                    await context.Interaction.RespondAsync(embed: this._venueRenderer.RenderEmbed(venue).Build(),
                        components: this._venueRenderer.RenderEditComponents(venue, user).Build());
                }
                
                if (venues.Count() > 25)
                    venues = venues.Take(25);
                
                await context.Interaction.RespondAsync(VenueControlStrings.SelectVenueToEdit,
                    components: this._venueRenderer.RenderVenueSelection(venues, SelectVenueToEditHandler.Key).Build());
            }

        }

    }
}
