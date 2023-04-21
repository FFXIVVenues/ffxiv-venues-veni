using System;
using System.Text;
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
            ISessionState currentSessionState = null;
            context.Session.StateStack?.TryPeek(out currentSessionState);
            var stateTextBuilder = new StringBuilder();
            if (currentSessionState != null)
            {
                stateTextBuilder.Append("[")
                    .Append(currentSessionState.GetType().Name)
                    .Append("] ");
            }

            var messageBuilder = new StringBuilder();
            messageBuilder.Append(stateTextBuilder)
                .Append(context.Interaction.Author.Mention)
                .Append(":\n> ")
                .Append(context.Interaction.Content);

            this._chronicle.Info(messageBuilder.ToString());
            
            return next();
        }
    }
}
