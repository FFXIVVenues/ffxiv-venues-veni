using System.Threading.Tasks;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Authorisation.Blacklist;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.Engineering;

[DiscordCommand("root blacklist remove", "Remove a discord guild or user from the blacklist.")]
public class BlacklistRemoveCommand : ICommandHandler
{
    private readonly IRepository _db;
    private readonly IAuthorizer _authorizer;

    public BlacklistRemoveCommand(IRepository db, IAuthorizer authorizer)
    {
        this._db = db;
        this._authorizer = authorizer;
    }

    public async Task HandleAsync(SlashCommandVeniInteractionContext slashCommand)
    {
        if (!_authorizer.Authorize(slashCommand.Interaction.User.Id, Permission.Blacklist).Authorized)
            return;

        await slashCommand.Interaction.DeferAsync();
        await _db.DeleteAsync<BlacklistEntry>(id: slashCommand.GetStringArg("discordid"));
        await slashCommand.Interaction.FollowupAsync("Discord ID either was removed or wasnt on the blacklist 😊");
    }
}