
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Utils.TypeConditioning;
using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Context.InteractionWrappers
{
    public class SlashCommandWrapper : IInteractionWrapper
    {

        public SocketUser User => _slashCommand?.User;
        public string Content => null;
        public IInteractionDataWrapper InteractionData { get; set; }
        public bool IsDM => this._slashCommand.IsDMInteraction;

        private SocketSlashCommand _slashCommand { get; }


        public SlashCommandWrapper(SocketSlashCommand slashCommand)
        {
            _slashCommand = slashCommand;
            InteractionData = new SlashCommandDataWrapper(slashCommand.Data);
        }

        public ResolutionCondition<T> If<T>() =>
            new ResolutionCondition<T>(_slashCommand);

        public Task RespondAsync(string message = null, MessageComponent component = null, Embed embed = null)
        {
            Console.WriteLine($"\t\tReply\tVeni Ki: {message}");
            return _slashCommand.HasResponded ?
                   _slashCommand.Channel.SendMessageAsync(message, components: component, embed: embed) :
                   _slashCommand.RespondAsync(message, components: component, embed: embed);
        }

        public string GetArgument(string name) =>
            this.InteractionData.GetArgument(name);


    }

}
