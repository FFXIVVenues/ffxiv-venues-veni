using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Utils;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Context
{

    public class MessageContext
    {
        public SocketMessage Message { get; }
        public SocketMessageComponent MessageComponent { get; }
        public Prediction Prediction { get; set; }
        public DiscordSocketClient Client { get; }
        public ConversationContext Conversation { get; }

        private Dictionary<string, Func<MessageContext, Task>> _componentHandlers;
        private readonly ILogger _logger;

        public MessageContext(SocketMessage message, DiscordSocketClient client, ConversationContext conversation, ILogger logger)
        {
            Message = message;
            Client = client;
            Conversation = conversation;
            _logger = logger;
            this._componentHandlers = new();
        }

        public MessageContext(SocketMessageComponent messageComponent, DiscordSocketClient client, ConversationContext conversation, ILogger logger)
        {
            MessageComponent = messageComponent;
            Client = client;
            Conversation = conversation;
            _logger = logger;
            this._componentHandlers = new();
        }

        public Task RespondAsync(string message = null, MessageComponent component = null, Embed embed = null, MessageFlags flags = MessageFlags.None)
        {
            _ = _logger.LogAsync("Reply", $"Veni Ki: {message}");
            if (this.Message != null)
                return Message.Channel.SendMessageAsync(message, components: component, embed: embed, flags: flags);
            else
                return MessageComponent.Channel.SendMessageAsync(message, components: component, embed: embed, flags: flags);
        }


    }
}
