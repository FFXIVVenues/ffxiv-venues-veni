using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States.Abstractions;
using Kana.Pipelines;
using NChronicle.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Middleware
{
    internal class LogMiddleware : IMiddleware<MessageInteractionContext>
    {
        private readonly IChronicle _chronicle;

        public LogMiddleware(IChronicle chronicle)
        {
            this._chronicle = chronicle;
        }

        public Task ExecuteAsync(MessageInteractionContext context, Func<Task> next)
        {
            var stateText = "";
            IState currentState = null;
            context.Session.StateStack?.TryPeek(out currentState);
            if (currentState != null)
                stateText = "[" + currentState.GetType().Name + "] ";
            this._chronicle.Debug($"{stateText}{context.Interaction.Author}: {context.Interaction.Content}");
            return next();
        }
    }
}
