using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.Infrastructure.Intent
{
    internal interface IIntentHandler
    {

        Task Handle(MessageVeniInteractionContext context);

        Task Handle(MessageComponentVeniInteractionContext context);

        Task Handle(SlashCommandVeniInteractionContext context);
        
    }

    abstract class IntentHandler : IIntentHandler
    {
        public Task Handle(MessageVeniInteractionContext context) =>
            this.Handle(context.ToWrappedInteraction());

        public Task Handle(MessageComponentVeniInteractionContext context) =>
            this.Handle(context.ToWrappedInteraction());

        public Task Handle(SlashCommandVeniInteractionContext context) => 
            this.Handle(context.ToWrappedInteraction());

        public abstract Task Handle(VeniInteractionContext context);
    }


}