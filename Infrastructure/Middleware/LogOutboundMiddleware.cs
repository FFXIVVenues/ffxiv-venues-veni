using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Infrastructure.Logging;
using Kana.Pipelines;
using NChronicle.Core.Interfaces;

namespace FFXIVVenues.Veni.Infrastructure.Middleware
{
    internal class LogOutboundMiddleware : IMiddleware<MessageVeniInteractionContext>
    {
        private readonly IChronicle _chronicle;

        public LogOutboundMiddleware(IChronicle chronicle)
        {
            this._chronicle = chronicle;
        }

        public Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
        {
            if (context.Interaction.Author.Id != context.Client.CurrentUser.Id)
                return next();
            if (context.Interaction.Content.TrimStart().StartsWith(DiscordChronicleLibrary.LOG_INIT_SEQUENCE))
                return next();

            ISessionState currentSessionState = null;
            context.Session.StateStack?.TryPeek(out currentSessionState);
            var stateTextBuilder = new StringBuilder();
            if (currentSessionState != null)
            {
                stateTextBuilder.Append("[")
                    .Append(currentSessionState.GetType().Name)
                    .Append("] ");
            }
            
            var contentBuilder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(context.Interaction.Content))
            {
                contentBuilder
                    .AppendLine()
                    .Append("> ")
                    .Append(context.Interaction.Content);
            }
            if (context.Interaction.Embeds.Count > 0 || context.Interaction.Components.Count > 0)
            {
                contentBuilder.AppendLine();
            }
            if (context.Interaction.Embeds.Count > 0)
            {
                contentBuilder.Append("(")
                    .Append(context.Interaction.Embeds.Count)
                    .Append(" Embeds) ");
            }
            if (context.Interaction.Components.Count > 0)
            {
                contentBuilder.Append("(")
                    .Append(context.Interaction.Components.Count)
                    .Append(" Components) ");
            }
            
            var messageBuilder = new StringBuilder();
            messageBuilder.Append(stateTextBuilder)
                .Append(context.Client.CurrentUser.Mention)
                .Append(":")
                .Append(contentBuilder);

            this._chronicle.Info(messageBuilder.ToString());
            return next();
        }
    }
}
