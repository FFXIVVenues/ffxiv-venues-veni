using Discord;
using FFXIVVenues.Veni.Intents;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.Intents.Interupt
{
    internal class Help : IntentHandler
    {

        public override Task Handle(VeniInteractionContext context) =>
            context.Interaction.RespondAsync(
"Here's what I can do for you!\n\n" +
"`create my venue`\t\tYou can ask me to create a new venue and place it on the index.\n" +
"`edit my venue`\t\t\tIf you already own a venue, I can edit it's details for you.\n" +
"`show my venue`\t\t\tI can show you the details of one of your existing venues.\n" +
"`open my venue`\t\t\tYou can also ask me to mark your venue as open on the index for two hours.\n" +
"`close my venue`\t\t  Or ask me to mark your venue as closed for the next 18 hours (or if you're already open, end your current opening).\n\n" +

"You can also use slash commands; just type `/` in the chat below to see the guided commands available.\n\n" +

$"If you need any help or have any questions about managing your venue, please meow at {MentionUtils.MentionUser(People.People.Kana)} or {MentionUtils.MentionUser(People.People.Sumi)} :heart:.");

    }
}
