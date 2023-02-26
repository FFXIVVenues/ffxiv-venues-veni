using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.AI
{
    internal class AIHandler
    {
        public string ResponseHandler(InteractionContext context)
        {
            var messageContent = context.Interaction.Content.Replace($"<@994410006638239795> ", "").Trim();
            var id = context.Interaction.User.Id.ToString();

            return new DavinciProxy().AskTheAI("Me: " + new AIRepository().GetMyLore(id, messageContent) + ". You: ");
        } 
        
    }
}
