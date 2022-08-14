using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents
{
    internal interface IIntentHandler
    {

        Task Handle(MessageInteractionContext context);

        Task Handle(MessageComponentInteractionContext context);

        Task Handle(SlashCommandInteractionContext context);
    }

    abstract class IntentHandler : IIntentHandler
    {
        public Task Handle(MessageInteractionContext context) =>
            this.Handle(context.ToWrappedInteraction());

        public Task Handle(MessageComponentInteractionContext context) =>
            this.Handle(context.ToWrappedInteraction());

        public Task Handle(SlashCommandInteractionContext context) => 
            this.Handle(context.ToWrappedInteraction());

        public abstract Task Handle(InteractionContext context);
    }


}