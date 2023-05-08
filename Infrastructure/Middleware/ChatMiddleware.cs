using FFXIVVenues.Veni.AI;
using FFXIVVenues.Veni.Utils;
using Kana.Pipelines;
using NChronicle.Core.Interfaces;
using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.AI.Davinci;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.Infrastructure.Middleware
{
    internal class ChatMiddleware : IMiddleware<MessageVeniInteractionContext>
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
        public async Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
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
