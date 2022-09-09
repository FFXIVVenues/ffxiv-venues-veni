using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class ShowCount : IntentHandler
    {
        private readonly IApiService _apiService;

        public ShowCount(IApiService apiService)
        {
            this._apiService = apiService;
        }

        public override async Task Handle(InteractionContext context)
        {
            var venues = await this._apiService.GetAllVenuesAsync();

            int crystalSum = 0;
            int primalSum = 0;
            int aetherSum = 0;

            foreach (var venue in venues)
            {
                if (venue.Location.DataCenter.Equals("Crystal"))
                {
                    crystalSum++;
                }
                else if (venue.Location.DataCenter.Equals("Primal"))
                {
                    primalSum++;
                }
                else if (venue.Location.DataCenter.Equals("Aether"))
                {
                    aetherSum++;
                }
            }
            await context.Interaction.RespondAsync(" We have **" + (aetherSum + crystalSum + primalSum) + "** total venues! 🤗.\n **" +
                aetherSum + "** from Aether, **" +
                crystalSum + "** from Crystal, and **" +
                primalSum + "** in Primal."
                );
        }

    }
}
