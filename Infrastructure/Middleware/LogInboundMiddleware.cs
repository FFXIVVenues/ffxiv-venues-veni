using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Infrastructure.Logging;
using Kana.Pipelines;
using NChronicle.Core.Interfaces;

namespace FFXIVVenues.Veni.Infrastructure.Middleware
{
    internal class LogInboundMiddleware : IMiddleware<MessageVeniInteractionContext>
    {
        private readonly IChronicle _chronicle;

        public LogInboundMiddleware(IChronicle chronicle)
        {
            this._chronicle = chronicle;
        }

        public Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
        {
            var stateText = "";
            ISessionState currentSessionState = null;
            context.Session.StateStack?.TryPeek(out currentSessionState);
            if (currentSessionState != null)
                stateText = "[" + currentSessionState.GetType().Name + "] ";
            this._chronicle.Info($"{stateText}{context.Interaction.Author.Mention}:\n> {context.Interaction.Content}");
            
            return next();
        }
    }
}
