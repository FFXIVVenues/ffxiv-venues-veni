using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.People;
using FFXIVVenues.Veni.Services.Api;
using FFXIVVenues.Veni.SessionStates;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.Veni.VenueControl.SessionStates;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.Commands
{
    internal class GetUnapproved
    {
        public const string COMMAND_NAME = "getunapproved";

        internal class CommandFactory : ICommandFactory
        {
            public SlashCommandProperties GetSlashCommand(SocketGuild guildContext = null)
            {
                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Get venues that are currently not approved and therefore hidden from the index.")
                    .Build();
            }
        }

        internal class CommandHandler : ICommandHandler
        {
            private readonly IApiService _apiService;
            private readonly IVenueRenderer _venueRenderer;
            private readonly IAuthorizer _authorizer;
            private IEnumerable<Venue> _venues;

            public CommandHandler(IApiService apiService,
                                IVenueRenderer venueRenderer,
                                IAuthorizer authorizer)
            {
                this._apiService = apiService;
                this._venueRenderer = venueRenderer;
                this._authorizer = authorizer;
            }

            public async Task HandleAsync(SlashCommandVeniInteractionContext c) 
            {
                var asker = c.Interaction.User.Id;
                var auth = this._authorizer.Authorize(asker, Permission.ApproveVenue);
                if (!auth.Authorized)
                {
                    await c.Interaction.FollowupAsync("Sorry, you're not authorized to see unapproved venues. 😢");
                    return;
                }
                
                this._venues = await this._apiService.GetUnapprovedVenuesAsync();
                if (this._venues.Count() > 25)
                    this._venues = this._venues.Take(25);

                if (this._venues == null || !this._venues.Any())
                {
                    await c.Interaction.RespondAsync("I don't have any venues needing approval. 🙂");
                    return;
                }

                var selectMenuKey = c.Session.RegisterComponentHandler(this.HandleVenueSelection, ComponentPersistence.ClearRow);
                var componentBuilder = new ComponentBuilder();
                var selectMenuBuilder = new SelectMenuBuilder() { CustomId = selectMenuKey };

                foreach (var venue in this._venues)
                {
                    var selectMenuOption = new SelectMenuOptionBuilder
                    {
                        Label = venue.Name,
                        Description = venue.Location.ToString(),
                        Value = venue.Id
                    };
                    selectMenuBuilder.AddOption(selectMenuOption);
                }
                componentBuilder.WithSelectMenu(selectMenuBuilder);

                await c.Interaction.RespondAsync("Here you go! 🥰", components: componentBuilder.Build());
            }

            private Task HandleVenueSelection(MessageComponentVeniInteractionContext c)
            {
                var selectedVenueId = c.Interaction.Data.Values.Single();
                var asker = c.Interaction.User.Id;
                var venue = this._venues.FirstOrDefault(v => v.Id == selectedVenueId);

                var auth = this._authorizer.Authorize(asker, Permission.ApproveVenue);
                if (!auth.Authorized)
                    return c.Interaction.FollowupAsync("Sorry, you're not authorized to see unapproved venues. 😢");

                return c.Interaction.FollowupAsync(embed: this._venueRenderer.RenderEmbed(venue).Build(),
                        components: new ComponentBuilder()
                            .WithButton("Approve", c.Session.RegisterComponentHandler(async cm =>
                            {
                                await this._apiService.ApproveAsync(venue.Id);
                                await cm.Interaction.Channel.SendMessageAsync("Nyya! I've approved the venue! 💝");
                            }, ComponentPersistence.ClearRow), ButtonStyle.Primary)
                            .WithButton("Edit", c.Session.RegisterComponentHandler(cm =>
                            {
                                c.Session.SetItem("venue", venue);
                                return cm.Session.MoveStateAsync<ModifyVenueSessionState>(cm);
                            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                            .WithButton("Delete", c.Session.RegisterComponentHandler(cm =>
                            {
                                c.Session.SetItem("venue", venue);
                                return cm.Session.MoveStateAsync<DeleteVenueSessionState>(cm);
                            }, ComponentPersistence.ClearRow), ButtonStyle.Danger)
                            .WithButton("Do nothing", c.Session.RegisterComponentHandler(cm => Task.CompletedTask,
                                ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                            .Build());
            }

        }
    }
}