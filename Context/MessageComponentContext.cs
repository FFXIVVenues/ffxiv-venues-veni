using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Context
{

    class MessageComponentContext
    {
        public SocketMessageComponent MessageComponent { get; }
        public DiscordSocketClient Client { get; }
        public ConversationContext Conversation { get; }

        private readonly ILogger _logger;

        public MessageComponentContext(SocketMessageComponent message, DiscordSocketClient client, ConversationContext conversation, ILogger logger)
        {
            this.MessageComponent = message;
            this.Client = client;
            this.Conversation = conversation;
            this._logger = logger;
        }

        public Task SendMessageAsync(string message, MessageComponent component = null, Embed embed = null)
        {
            _ = this._logger.LogAsync("Reply", $"Veni Ki: {message}");
            return this.MessageComponent.Channel.SendMessageAsync(message, components: component, embed: embed);
        }

        public Task SendEmbedAsync(Embed embed)
        {
            _ = this._logger.LogAsync("Reply", $"Veni Ki: Embed [{embed.Title}]");
            return this.MessageComponent.Channel.SendMessageAsync(embed: embed);
        }

    }
}
