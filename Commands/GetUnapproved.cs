using Discord.WebSocket;
using Discord;
using FFXIVVenues.Veni.Commands.Brokerage;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Context;
using System.Linq;
using System.Collections.Generic;
using FFXIVVenues.Veni.States;
using FFXIVVenues.Veni.Models;
using FFXIVVenues.Veni.Managers;
using FFXIVVenues.Veni.Services;

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
            private readonly IStaffManager _staffService;
            private readonly string _uiUrl;
            private readonly string _apiUrl;
            private IEnumerable<Venue> _venues;

            public CommandHandler(IApiService _apiService,
                                  UiConfiguration uiConfig,
                                  ApiConfiguration apiConfig,
                                  IStaffManager staffService)
            {
                this._apiService = _apiService;
                this._staffService = staffService;
                this._uiUrl = uiConfig.BaseUrl;
                this._apiUrl = apiConfig.BaseUrl;
            }

            public async Task HandleAsync(SlashCommandInteractionContext c) 
            {
                var asker = c.Interaction.User.Id;
                var isEditorOrApprover = this._staffService.IsEditor(asker) || this._staffService.IsApprover(asker);


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

            private Task HandleVenueSelection(MessageComponentInteractionContext c)
            {
                var selectedVenueId = c.Interaction.Data.Values.Single();
                var asker = c.Interaction.User.Id;
                var venue = this._venues.FirstOrDefault(v => v.Id == selectedVenueId);

                var isEditorOrApprover = this._staffService.IsEditor(asker) || this._staffService.IsApprover(asker);
                if (!isEditorOrApprover)
                    return c.Interaction.FollowupAsync("Sorry, you're not authorized to see unapproved venues. 😢");

                return c.Interaction.FollowupAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build(),
                        components: new ComponentBuilder()
                            .WithButton("Approve", c.Session.RegisterComponentHandler(async cm =>
                            {
                                await this._apiService.ApproveAsync(venue.Id);
                                await cm.Interaction.Channel.SendMessageAsync("Nyya! I've approved the venue! 💝");
                            }, ComponentPersistence.ClearRow), ButtonStyle.Primary)
                            .WithButton("Edit", c.Session.RegisterComponentHandler(cm =>
                            {
                                c.Session.SetItem("venue", venue);
                                return cm.Session.MoveStateAsync<ModifyVenueState>(cm);
                            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                            .WithButton("Delete", c.Session.RegisterComponentHandler(cm =>
                            {
                                c.Session.SetItem("venue", venue);
                                return cm.Session.MoveStateAsync<DeleteVenueState>(cm);
                            }, ComponentPersistence.ClearRow), ButtonStyle.Danger)
                            .WithButton("Do nothing", c.Session.RegisterComponentHandler(cm => Task.CompletedTask,
                                ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                            .Build());
            }

        }
    }
}