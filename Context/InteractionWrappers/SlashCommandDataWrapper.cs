using Discord.WebSocket;
using FFXIVVenues.Veni.Utils.TypeConditioning;
using System.Linq;

namespace FFXIVVenues.Veni.Context.InteractionWrappers
{
    public class SlashCommandDataWrapper : IInteractionDataWrapper
    {

        public string Name => _data.Name;

        private readonly SocketSlashCommandData _data;

        public SlashCommandDataWrapper(SocketSlashCommandData data)
        {
            _data = data;
        }

        public ResolutionCondition<T> If<T>() =>
            new ResolutionCondition<T>(_data);

        public string GetArgument(string name) =>
            this._data.Options?.FirstOrDefault(o => o.Name == name)?.Value as string;
    }

}
