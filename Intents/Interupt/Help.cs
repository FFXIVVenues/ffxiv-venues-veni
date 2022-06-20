using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Interupt
{
    internal class Help : IIntentHandler
    {

        public async Task Handle(MessageContext context)
        {

            var kana = await context.Client.Rest.GetUserAsync(236852510688542720);
            //var kana = context.Client.GetUser(236852510688542720);

            await context.SendMessageAsync(
"Here's what I can do for you!\n\n" +
"`create venue`\t\tYou can ask me to create a new venue and place it on the index.\n" +
"`edit venue`\t\t\tIf you already own a venue, I can edit it's details for you.\n" +
"`show venue`\t\t\tI can show you the details of one of your existing venues.\n" +
"`delete venue`\t\tOr I can completely delete a venue. Sadge. \n" +
"`open venue`\t\t\tYou can also ask me to mark your venue as open on the index for two hours.\n" +
"`close venue`\t\t  Or ask me to mark your venue as closed for the next 18 hours (or if you're already open, end your current opening).\n\n" +

$"If you need any help or have any questions, please meow at {kana.Mention} :heart:.");
        }

    }
}
