using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Utils.TypeConditioning;
using NChronicle.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Context.InteractionWrappers
{
    public class SlashCommandWrapper : IInteractionWrapper
    {

        public SocketUser User => _slashCommand?.User;
        public ISocketMessageChannel Channel => _slashCommand?.Channel;
        public string Content => null;
        public IInteractionDataWrapper InteractionData { get; set; }
        public bool IsDM => this._slashCommand.IsDMInteraction;

        private readonly SocketSlashCommand _slashCommand;
        private readonly IChronicle _chronicle;

        public SlashCommandWrapper(SocketSlashCommand slashCommand, IChronicle chronicle)
        {
            this._slashCommand = slashCommand;
            this._chronicle = chronicle;
            this.InteractionData = new SlashCommandDataWrapper(slashCommand.Data);
        }

        public ResolutionCondition<T> If<T>() =>
            new (_slashCommand);

        public Task RespondAsync(string message = null, MessageComponent component = null, Embed embed = null)
        {
            this._chronicle.Info($"**Veni Ki** [bot]: {message} (Components: {component?.Components?.Count ?? 0}) (Embeds: {(embed != null ? "Yes" : "No")})");
            return _slashCommand.HasResponded ?
                   _slashCommand.Channel.SendMessageAsync(message, components: component, embed: embed) :
                   _slashCommand.RespondAsync(message, components: component, embed: embed);
        }

        public string GetArgument(string name) =>
            this.InteractionData.GetArgument(name);


    }

}
