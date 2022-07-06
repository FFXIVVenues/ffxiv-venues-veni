using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Interupt
{
    internal class Escalate : IIntentHandler
    {
        private readonly IIndexersService _indexersService;

        public Escalate(IIndexersService indexersService)
        {
            this._indexersService = indexersService;
        }

        public async Task Handle(MessageContext context)
        {
            await context.RespondAsync($"Alright! I've messaged mom! She or another indexer will contact you soon!");

            await this._indexersService
                .Broadcast()
                .WithMessage($"Heyo, I need an human indexer! I have {context.Message.Author.Mention} needing some help. :cry:")
                .SendToAsync(this._indexersService.Indexers);
        }

    }
}
