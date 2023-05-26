﻿using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.VenueDiscovery.Commands
{
    internal class ShowCountCommand
    {
        public const string COMMAND_NAME = "showcount";

        internal class CommandFactory : ICommandFactory
        {
            public SlashCommandProperties GetSlashCommand ()
            {
                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Show how many venues are indexed so far.")
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
                await c.Interaction.DeferAsync();
                var venues = await this._apiService.GetAllVenuesAsync();

                var total = 0;
                //NA
                var crystalSum = 0;
                var primalSum = 0;
                var aetherSum = 0;
                var dynamisSum = 0;
                //EU
                var chaosSum = 0;
                var lightSum = 0;

                foreach (var venue in venues)
                {
                    total++;
                    if (venue.Location?.DataCenter == null) continue;
                    if (venue.Location.DataCenter.Equals("Crystal")) crystalSum++;
                    else if (venue.Location.DataCenter.Equals("Primal")) primalSum++;
                    else if (venue.Location.DataCenter.Equals("Aether")) aetherSum++;
                    else if (venue.Location.DataCenter.Equals("Dynamis")) dynamisSum++;
                    else if (venue.Location.DataCenter.Equals("Chaos")) chaosSum++;
                    else if (venue.Location.DataCenter.Equals("Light")) lightSum++;
                }

                await c.Interaction.FollowupAsync(" We have **" + total +
                        "** total venues! 🤗.\n" +
                        "In **NA**: **" +
                        aetherSum + "** from Aether, **" +
                        dynamisSum + "** from Dynamis, **" +
                        crystalSum + "** from Crystal, and **" +
                        primalSum + "** in Primal. \n" +
                        "In **EU**: **" +
                        chaosSum + "** from Chaos, and **" +
                        lightSum + "** in Light.");
            }
            
        }
    }
}
