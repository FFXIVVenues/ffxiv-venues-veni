using Discord.WebSocket;
using FFXIVVenues.Veni.Utils.TypeConditioning;
using System.Linq;

namespace FFXIVVenues.Veni.Context.InteractionWrappers
{
    public class ComponentDataWrapper : IInteractionDataWrapper
    {

        private readonly SocketMessageComponentData _data;

        public string Name => _data.CustomId;


        public ComponentDataWrapper(SocketMessageComponentData data)
        {
            _data = data;
        }

        public ResolutionCondition<T> If<T>() =>
            new ResolutionCondition<T>(_data);

        public string GetArgument(string name) =>
            this._data.Value;
    }

}
