using Discord.WebSocket;
using FFXIVVenues.Veni.Utils;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Context
{

    class MessageContext
    {
        public SocketMessage Message { get; }
        public Prediction Prediction { get; set; }
        public DiscordSocketClient Client { get; }
        public ConversationContext Conversation { get; }

        private readonly ILogger _logger;

        public MessageContext(SocketMessage message, DiscordSocketClient client, ConversationContext conversation, ILogger logger)
        {
            Message = message;
            Client = client;
            Conversation = conversation;
            _logger = logger;
        }

        public Task SendMessageAsync(string message)
        {
            _ = _logger.LogAsync("Reply", $"Veni Ki: {message}");
            return Message.Channel.SendMessageAsync(message);
        }

    }
}
