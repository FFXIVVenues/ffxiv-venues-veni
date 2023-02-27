using FFXIVVenues.Veni.AI;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using Kana.Pipelines;
using NChronicle.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Middleware
{
    internal class ChatMiddleware : IMiddleware<MessageInteractionContext>
    {
        private readonly IAIHandler aIHandler;
        private readonly IChronicle chronicle;

        private static string[] _emotes = new[]
        {
            " ",
            " :3",
            " 💕",
            " 💖",
            " ❤️",
            " 💜",
            " 💞"
        };

        public ChatMiddleware(IAIHandler aIHandler, IChronicle chronicle)
        {
            this.aIHandler = aIHandler;
            this.chronicle = chronicle;
        }
        public async Task ExecuteAsync(MessageInteractionContext context, Func<Task> next)
        {
            try
            {
                string response = await this.aIHandler.ResponseHandler(context);
                await context.Interaction.Channel.SendMessageAsync(response + _emotes.PickRandom());
            }
            catch (Exception ex)
            {
                chronicle.Warning(ex);
                await next();
            }

        }

    }
}
