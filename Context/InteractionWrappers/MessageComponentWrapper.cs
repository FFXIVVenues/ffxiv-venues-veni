
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Utils.TypeConditioning;
using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Context.InteractionWrappers
{
    public class MessageComponentWrapper : IInteractionWrapper
    {

        public SocketUser User => _messageComponent?.User;
        public string Content => null;
        public IInteractionDataWrapper InteractionData { get; set; }


        private SocketMessageComponent _messageComponent { get; }


        public MessageComponentWrapper(SocketMessageComponent messageComponent)
        {
            _messageComponent = messageComponent;
            InteractionData = new ComponentDataWrapper(messageComponent.Data);
        }

        public ResolutionCondition<T> If<T>() =>
            new ResolutionCondition<T>(_messageComponent);

        public Task RespondAsync(string message = null, MessageComponent component = null, Embed embed = null)
        {
            Console.WriteLine($"\t\tReply\tVeni Ki: {message}");
            return _messageComponent.HasResponded ? 
                _messageComponent.Channel.SendMessageAsync(message, components: component, embed: embed) : 
                _messageComponent.RespondAsync(message, components: component, embed: embed);
        }

        public string GetArgument(string name) =>
            this.InteractionData.GetArgument(name);
    }

}
