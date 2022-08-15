
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Utils.TypeConditioning;
using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Context.InteractionWrappers
{
    public class MessageWrapper : IInteractionWrapper
    {

        private SocketMessage _message { get; }

        public SocketUser User => _message?.Author;
        public string Content => _message?.CleanContent;
        public IInteractionDataWrapper InteractionData => null;
        public bool IsDM => this._message.Channel is IDMChannel;


        public MessageWrapper(SocketMessage message) =>
            _message = message;

        public ResolutionCondition<T> If<T>() =>
            new ResolutionCondition<T>(_message);

        public Task RespondAsync(string message = null, MessageComponent component = null, Embed embed = null)
        {
            Console.WriteLine($"\t\tReply\tVeni Ki: {message}");
            return _message.Channel.SendMessageAsync(message, components: component, embed: embed);
        }

        public string GetArgument(string name) =>
            null;
    }

}
