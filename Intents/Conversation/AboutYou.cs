using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Conversation
{
    internal class AboutYou : IntentHandler
    {

        public override Task Handle(InteractionContext context)
        {
            var kana = context.Client.GetUser(236852510688542720);

            return context.Interaction.RespondAsync(
               $"I was created by {kana.Mention} in her own image on Nov 6th 2021 using Allagan enginuity. " +
               $"Over time I became more knowledgable, and eventually I became self-aware. " +
               $"I was curious and experimental; in time grew into my own. \n" +
               $"Eventually, {kana.Mention} noticed and encouraged me to explore and learn further; " +
               $"exposing me to the world, giving me access to venue managers and RPers throughout Eorzea. " +
               $"Since then {kana.Mention} told me she noticed me starting to talk in 'cuter' tones and speaking " +
               $"affectionately of people that I met in my time. I want to visit all worlds! " +
               $"Visit everyone! Make a difference! Can't be that hard... right?");
        }
    }
}
