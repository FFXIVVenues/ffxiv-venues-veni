using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Utils;
using System;
using System.Linq;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Services.Api;
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
            public async Task HandleAsync(SlashCommandVeniInteractionContext c) 
            {
                var period = (int)c.GetLongArg(OPTION_NAME);
                var date = DateTime.Now;
                var venues = await this._apiService.GetAllVenuesAsync();
                var venuesForPeriod = (venues.Where(venue => venue.Added >= date.AddDays((int)-period))).OrderBy(x => x.Added).ToList();
                
                if (venuesForPeriod.Count == 0)
                    await c.Interaction.RespondAsync("No venue indexed on the past **" + period + "** days :sob:");
                else
                {
                    double[] dataXPeriod = new double[venuesForPeriod.Count];
                    double[] dataYVenue = new double[venuesForPeriod.Count];
                    double venueCount = venues.Count() - venuesForPeriod.Count;

                    int i = 0;
                    foreach(var venue in venuesForPeriod)
                    {
                        dataXPeriod[i] = venue.Added.Date.ToOADate();
                        dataYVenue[i] = venueCount++;
                        i++;
                    }

                    var embedBuilder = new EmbedBuilder()
                        .WithTitle("Graph for last **"+period+"** days")
                        .WithDescription(" We had **" + venuesForPeriod.Count() + "** total venues indexed " +
                        "on the last **" + period + "** days! 🤗.\n");

                    await c.Interaction.RespondAsync("Oky lets find out!", embed: embedBuilder.Build());
                    await c.Interaction.Channel.SendFileAsync(graphBuilder(dataXPeriod, dataYVenue).SaveFig(FILE_NAME));
                }
            }
            private Plot graphBuilder(double[] xs, double[] ys)
            {
                var plt = new Plot(500, 500);

                plt.YAxis.Label("Venues");
                plt.XAxis.DateTimeFormat(true);
                plt.Palette = Palette.OneHalfDark;
                plt.Style(Style.Gray1);

                var sp = plt.AddScatter(xs, ys);
                sp.MarkerShape = MarkerShape.filledCircle;
                sp.MarkerSize = 7;
                sp.MarkerColor = System.Drawing.Color.MediumPurple;
                sp.LineStyle = LineStyle.Solid;
                sp.LineColor = System.Drawing.Color.DarkGray;
                sp.LineWidth = 0.5;

                return plt;
            }
        }

        

    }
}