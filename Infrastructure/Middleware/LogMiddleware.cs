using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.Session;
using Kana.Pipelines;
using NChronicle.Core.Interfaces;

namespace FFXIVVenues.Veni.Infrastructure.Middleware
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
            ISessionState currentSessionState = null;
            context.Session.StateStack?.TryPeek(out currentSessionState);
            if (currentSessionState != null)
                stateText = " [" + currentSessionState.GetType().Name + "]";
            this._chronicle.Info($"**{context.Interaction.Author.Mention}{stateText}**: {context.Interaction.Content}");
            return next();
        }
    }
}
