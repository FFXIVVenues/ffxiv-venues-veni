using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.People;

namespace FFXIVVenues.Veni.Intents.Interupt
{
    internal class Escalate : IntentHandler
    {
        private readonly IStaffManager _staffService;

        public Escalate(IStaffManager staffService)
        {
            this._staffService = staffService;
        }

        public override async Task Handle(InteractionContext context)
        {
            await context.Interaction.RespondAsync($"Alright! I've messaged the family! They'll contact you soon!");

            await this._staffService
                .Broadcast()
                .WithMessage($"Heyo, I have {context.Interaction.User.Mention} needing some help. :cry:")
                .SendToAsync(this._staffService.Approvers);
        }

    }
}
