using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Interupt
{
    internal class Escalate : IntentHandler
    {
        private readonly IIndexersService _indexersService;

        public Escalate(IIndexersService indexersService)
        {
            this._indexersService = indexersService;
        }

        public override async Task Handle(InteractionContext context)
        {
            await context.Interaction.RespondAsync($"Alright! I've messaged mom! She or another indexer will contact you soon!");

            await this._indexersService
                .Broadcast()
                .WithMessage($"Heyo, I need an human indexer! I have {context.Interaction.User.Mention} needing some help. :cry:")
                .SendToAsync(this._indexersService.Indexers);
        }

    }
}
