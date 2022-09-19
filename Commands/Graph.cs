using Discord.WebSocket;
using Discord;
using FFXIVVenues.Veni.Commands.Brokerage;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Persistance.Abstraction;
using FFXIVVenues.VenueModels.V2022;
using FFXIVVenues.Veni.Utils;
using System;
using System.Linq;

namespace FFXIVVenues.Veni.Commands
{
    internal class Graph
    {
        public const string COMMAND_NAME = "graph";
        public const string OPTION_NAME = "period";
        public const string OPTION_DESCRIPTION = "period of time for graph";

        internal class CommandFactory : ICommandFactory
        {
            public SlashCommandProperties GetSlashCommand(SocketGuild guildContext = null)
            {
                var periodOption = new SlashCommandOptionBuilder()
                    .WithName(OPTION_NAME)
                    .WithDescription(OPTION_DESCRIPTION)
                    .WithType(ApplicationCommandOptionType.Integer)
                    .AddChoice("1 week", 7)
                    .AddChoice("15 days", 15)
                    .AddChoice("1 month", 30)
                    .AddChoice("3 months", 90)
                    .AddChoice("6 months", 180);

                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Show how many venues are indexed over a period of time.")
                    .AddOption(periodOption)
                    .Build();
            }

            internal class CommandHandler : ICommandHandler
            {
                private readonly IApiService _apiService;

                public CommandHandler(IApiService _apiService)
                {
                    this._apiService = _apiService;
                }

                public async Task HandleAsync(SlashCommandInteractionContext c) {
                    var period = c.GetInt(OPTION_NAME);
                    var date = DateTime.Now;
                    var venues = await this._apiService.GetAllVenuesAsync();

                    var venuesForPeriod = venues.Where(venue => venue.Added > date.AddDays((double)-period));


                    if (!venuesForPeriod.Any())
                    {
                        await c.Interaction.RespondAsync("No venue indexed during this period :sob:");
                    }

                    
                }
            }
        }
    }
}
