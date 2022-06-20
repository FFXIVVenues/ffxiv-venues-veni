using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Interupt
{
    internal class Escalate : IIntentHandler
    {

        public async Task Handle(MessageContext context)
        {
            await context.SendMessageAsync($"Alright! I've messaged mom! She'll contact you soon!");

            var kana = context.Client.GetUser(236852510688542720);
            var channel = await kana.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync($"Kaanaa! I have a {context.Message.Author.Mention} that needs some help");
        }

    }
}
