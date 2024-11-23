using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;

namespace FFXIVVenues.Veni.AI.Davinci;

internal class AiHandler(IDavinciService davinciService, IAiContextBuilder aiPromptBuilder)
    : IAiHandler
{
    public Task<string> ResponseHandler(MessageVeniInteractionContext context)
    {
        var messageContent = context.Interaction.Content;
        var id = context.Interaction.Author.Id.ToString();

        return davinciService.AskTheAi(aiPromptBuilder.GetPrompt(id, messageContent));
    }

}