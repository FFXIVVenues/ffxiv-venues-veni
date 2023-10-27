using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Logging;
using FFXIVVenues.Veni.Utils;
using NChronicle.Core.Model;

namespace FFXIVVenues.Veni.Engineering;

[DiscordCommand("root inspect start", "Start following Veni's logs in this channel.")]
[DiscordCommandOption("verbosity", "The maximum verbosity level of logging output.", ApplicationCommandOptionType.Number)]
[DiscordCommandOptionChoice("verbosity", "Debug", (int)ChronicleLevel.Debug)]
[DiscordCommandOptionChoice("verbosity", "Info", (int)ChronicleLevel.Info)]
[DiscordCommandOptionChoice("verbosity", "Success", (int)ChronicleLevel.Success)]
[DiscordCommandOptionChoice("verbosity", "Warning", (int)ChronicleLevel.Warning)]
[DiscordCommandOptionChoice("verbosity", "Critical", (int)ChronicleLevel.Critical)]
public class InspectStartCommand
{
    private readonly IAuthorizer _authorizer;
    private readonly IDiscordChronicleLibrary _chronicle;

    public InspectStartCommand(IAuthorizer authorizer, IDiscordChronicleLibrary chronicle)
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

        var verbosity = slashCommand.GetInt("verbosity");
        if (this._chronicle.IsSubscribed(slashCommand.Interaction.Channel))
            this._chronicle.Unsubscribe(slashCommand.Interaction.Channel);
        this._chronicle.Subscribe(slashCommand.Interaction.Channel, (ChronicleLevel) (verbosity ?? 3));
        await slashCommand.Interaction.FollowupAsync("Oki, I've **started inspection**. 👀");
    }
    
}