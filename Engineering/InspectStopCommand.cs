using System.Threading.Tasks;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Logging;

namespace FFXIVVenues.Veni.Engineering;

[DiscordCommand("root inspect stop", "Stop following Veni's logs.")]
public class InspectStopCommand
{
    private readonly IAuthorizer _authorizer;
    private readonly IDiscordChronicleLibrary _chronicle;

    public InspectStopCommand(IAuthorizer authorizer, IDiscordChronicleLibrary chronicle)
    {
        this._authorizer = authorizer;
        this._chronicle = chronicle;
    }
            
    public async Task HandleAsync(SlashCommandVeniInteractionContext slashCommand)
    {
        await slashCommand.Interaction.DeferAsync();
        if (!this._authorizer.Authorize(slashCommand.Interaction.User.Id, Permission.Inspect).Authorized)
        {
            await slashCommand.Interaction.FollowupAsync("Sorry, I only let Engineers do that with me.");
            return;
        }

        this._chronicle.Unsubscribe(slashCommand.Interaction.Channel);
        await slashCommand.Interaction.FollowupAsync("Oki, I've **stopped inspection**. I hope everything looks good!");
    }
    
}