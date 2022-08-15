using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Utils.TypeConditioning;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Context.InteractionWrappers
{
    public interface IInteractionWrapper
    {

        SocketUser User { get; }
        string Content { get; }
        IInteractionDataWrapper InteractionData { get; }
        bool IsDM { get; }

        ResolutionCondition<T> If<T>();
        
        string GetArgument(string name);

        Task RespondAsync(string message = null, MessageComponent component = null, Embed embed = null);


    }
}