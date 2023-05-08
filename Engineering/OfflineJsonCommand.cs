using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.Engineering
{
    internal class OfflineJsonCommand
    {
        public const string COMMAND_NAME = "offlinejson";
        private const string OPTION_NAME = "inludemanagers";
        private const string FILE_NAME = "venues.json";

        internal class CommandFactory : ICommandFactory
        {
            public SlashCommandProperties GetSlashCommand(SocketGuild guildContext = null)
            {
                var includeManagersOption = new SlashCommandOptionBuilder()
                    .WithName(OPTION_NAME)
                    .WithDescription("Whether to include managers in the resultant json.")
                    .WithType(ApplicationCommandOptionType.Boolean)
                    .WithDefault(false);

                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Generate an offline list of venues.")
                    .AddOption(includeManagersOption)
                    .Build();
            }
        }

        internal class CommandHandler : ICommandHandler
        {
            private readonly HttpClient _httpClient;
            private readonly IAuthorizer _authorizer;

            public CommandHandler(HttpClient httpClient, IAuthorizer authorizer)
            {
                this._httpClient = httpClient;
                _authorizer = authorizer;
            }

            public async Task HandleAsync(SlashCommandVeniInteractionContext c)
            {
                await c.Interaction.DeferAsync();

                if (!this._authorizer.Authorize(c.Interaction.User.Id, Permission.DownloadOfflineJson).Authorized)
                {
                    await c.Interaction.FollowupAsync("Sorry, I'll only give that to my parents. 😓");
                    return;
                }

                var includeManagers = c.GetBoolArg(OPTION_NAME) ?? false;
                var response = await _httpClient.GetAsync($"/venue");
                var venues = await response.Content.ReadFromJsonAsync<OfflineVenue[]>();
                venues = venues.Where(v => v.Approved).ToArray();
                if (!includeManagers)
                    foreach (var venue in venues)
                        venue.Managers = null;
                var stream = new MemoryStream();
                await JsonSerializer.SerializeAsync(stream, venues);
                await c.Interaction.FollowupWithFileAsync(stream, FILE_NAME);
            }
        }

        public class OfflineVenue : VenueModels.Venue
        {
            public new List<OfflineOpening> Openings { get; set; } = new ();
            public new List<OfflineOpenOverride> OpenOverrides { get; set; } = new();
            [JsonIgnore(Condition = JsonIgnoreCondition.Always)] public new bool Approved => true;
            [JsonIgnore(Condition = JsonIgnoreCondition.Always)] public new bool Open => false;
        }

        public class OfflineOpening : VenueModels.Opening
        {
            [JsonIgnore(Condition = JsonIgnoreCondition.Always)] public new bool IsNow => false;
        }

        public class OfflineOpenOverride : VenueModels.OpenOverride
        {
            [JsonIgnore(Condition = JsonIgnoreCondition.Always)] public new bool IsNow => false;
        }
        
    }
}