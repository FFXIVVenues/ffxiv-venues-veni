using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.AI
{
    internal class AIHandler : IAIHandler
    {
        private readonly IDavinciService davinciService;
        private readonly IAIContextBuilder aIContextBuilder;

        public AIHandler(IDavinciService davinciService, IAIContextBuilder aIContextBuilder)
        {
            this.davinciService = davinciService;
            this.aIContextBuilder = aIContextBuilder;
        }

        public Task<string> ResponseHandler(MessageInteractionContext context)
        {
            var messageContent = context.Interaction.Content;
            var id = context.Interaction.Author.Id.ToString();

            return this.davinciService.AskTheAI(this.aIContextBuilder.GetContext(id, messageContent));
        }

    }
}
