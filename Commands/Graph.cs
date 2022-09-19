using Discord.WebSocket;
using Discord;
using FFXIVVenues.Veni.Commands.Brokerage;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Utils;
using System;
using System.Linq;

namespace FFXIVVenues.Veni.Commands
{
    internal class Graph
    {
        public const string COMMAND_NAME = "graph";
        public const string OPTION_NAME = "Period";
        public const string OPTION_DESCRIPTION = "Period of time for graph";

        internal class CommandFactory : ICommandFactory
        {
            public SlashCommandProperties GetSlashCommand(SocketGuild guildContext = null)
            {
                var periodOption = new SlashCommandOptionBuilder()
                    .WithName(OPTION_NAME)
                    .WithDescription(OPTION_DESCRIPTION)
                    .WithType(ApplicationCommandOptionType.Integer)
                    .WithRequired(true)
                    .AddChoice("1 week", 7)
                    .AddChoice("2 weeks", 14)
                    .AddChoice("1 month", 30)
                    .AddChoice("3 months", 90)
                    .AddChoice("6 months", 180);

                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Show how many venues were indexed over a period of time.")
                    .AddOption(periodOption)
                    .Build();
            }
        }
        internal class CommandHandler : ICommandHandler
        {
            private readonly IApiService _apiService;

            public CommandHandler(IApiService _apiService)
            {
                this._apiService = _apiService;
            }
            public async Task HandleAsync(SlashCommandInteractionContext c) 
            {
                var period = (int)c.GetLongArg(OPTION_NAME);
                var date = DateTime.Now;
                var venues = await this._apiService.GetAllVenuesAsync();
                var venuesForPeriod = venues.Where(venue => venue.Added >= date.AddDays((int)-period));

                if (!venuesForPeriod.Any())
                    await c.Interaction.RespondAsync("No venue indexed on the past **"+ period +"** days :sob:");
                else
                    await c.Interaction.RespondAsync(" We had **" + venuesForPeriod.Count() + "** total venues indexed " +
                        "on the last **" + period + "** days! 🤗.\n");
            }
        }
    }
}