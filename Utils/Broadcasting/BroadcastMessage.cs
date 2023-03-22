using Discord;

namespace FFXIVVenues.Veni.Utils.Broadcasting;

public record BroadcastMessage(ulong UserId, IUserMessage Message, MessageStatus Status, string Log);

public enum MessageStatus
{
    Pending,
    Failed,
    FailedUserDeleted,
    Sent
}