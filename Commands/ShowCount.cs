using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Commands.Brokerage;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using FFXIVVenues.Veni.Services;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Commands
{
    internal class ShowCount
    {
        public const string COMMAND_NAME = "showcount";

        internal class CommandFactory : ICommandFactory
        {
            public SlashCommandProperties GetSlashCommand (SocketGuild guildContext = null)
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

            public async Task HandleAsync(SlashCommandInteractionContext c)
            {
                await c.Interaction.DeferAsync();
                var venues = await this._apiService.GetAllVenuesAsync();

                //NA
                int crystalSum = 0;
                int primalSum = 0;
                int aetherSum = 0;
                int dynamisSum = 0;
                //EU
                int chaosSum = 0;
                int lightSum = 0;

                foreach (var venue in venues)
                {
                    if (venue.Location.DataCenter.Equals("Crystal")) crystalSum++;
                    else if (venue.Location.DataCenter.Equals("Primal")) primalSum++;
                    else if (venue.Location.DataCenter.Equals("Aether")) aetherSum++;
                    else if (venue.Location.DataCenter.Equals("Dynamis")) dynamisSum++;
                    else if (venue.Location.DataCenter.Equals("Chaos")) chaosSum++;
                    else if (venue.Location.DataCenter.Equals("Light")) lightSum++;
                }

                

                await c.Interaction.RespondAsync(" We have **" + (aetherSum + crystalSum + primalSum + chaosSum + lightSum) +
                        "** total venues! 🤗.\n **" +
                        "In :regional_indicator_n: :regional_indicator_a:  we have: \n" +
                        aetherSum + "** from Aether, **" +
                        crystalSum + "** from Crystal, and **" +
                        primalSum + "** in Primal. \n" +
                        "In :regional_indicator_e: :regional_indicator_u: : \n" +
                        chaosSum + "** from Chaos, and **" +
                        lightSum + "** in Light.");
            }
            
        }
    }
}
