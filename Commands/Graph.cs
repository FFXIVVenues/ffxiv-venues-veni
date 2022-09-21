using Discord.WebSocket;
using Discord;
using FFXIVVenues.Veni.Commands.Brokerage;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Utils;
using System;
using System.Linq;
using ScottPlot;

namespace FFXIVVenues.Veni.Commands
{
    internal class Graph
    {
        public const string COMMAND_NAME = "graph";
        private const string OPTION_NAME = "period";
        private const string OPTION_DESCRIPTION = "Period of time for graph";
        private const string FILE_NAME = "graph.png";

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
                var venuesForPeriod = (venues.Where(venue => venue.Added >= date.AddDays((int)-period))).OrderBy(x => x.Added).ToList();
                var venuesCountByPeriod = venues.Count(venue => venue.Added >= date.AddDays((int)-period));
                if (venuesCountByPeriod == 0)
                    await c.Interaction.RespondAsync("No venue indexed on the past **" + period + "** days :sob:");
                else
                {
                    double[] dataXPeriod = new double[venuesCountByPeriod];
                    double[] dataYVenue = new double[venuesCountByPeriod];
                    double venueCount = venues.Count() - venuesCountByPeriod;

                    for (int i = 0; i < venuesCountByPeriod; i++)
                    {
                        dataXPeriod[i] = venuesForPeriod.ElementAt(i).Added.Date.ToOADate();
                        dataYVenue[i] = venueCount++;
                    }

                    var plt = new ScottPlot.Plot(500, 500);
                    plt.YAxis.Label("Venues");
                    plt.XAxis.DateTimeFormat(true);

                    plt.Palette = ScottPlot.Palette.OneHalfDark;
                    plt.Style(ScottPlot.Style.Gray1);
                    
                    var sp = plt.AddScatter(dataXPeriod, dataYVenue);
                    sp.MarkerShape = MarkerShape.filledCircle;
                    sp.MarkerSize = 7;
                    sp.MarkerColor = System.Drawing.Color.MediumPurple;
                    sp.LineStyle = LineStyle.Solid;
                    sp.LineColor = System.Drawing.Color.DarkGray;
                    sp.LineWidth = 0.5;


                    var embedBuilder = new EmbedBuilder()
                        .WithTitle("Graph for last **"+period+"** days")
                        .WithDescription(" We had **" + venuesForPeriod.Count() + "** total venues indexed " +
                        "on the last **" + period + "** days! 🤗.\n");

                    await c.Interaction.RespondAsync("Oky lets find out!", embed: embedBuilder.Build());
                    await c.Interaction.Channel.SendFileAsync(plt.SaveFig(FILE_NAME));
                }

            }
        }
    }
}