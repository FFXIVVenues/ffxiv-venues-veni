using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.GuildEngagement;

[DiscordCommand("config managerrole", "Set role to assign to venue managers of the specified Data Center.", GuildPermission.ManageRoles, false)]
[DiscordCommandOption("datacenter", "The data center to assign the given role to.", ApplicationCommandOptionType.String)]
[DiscordCommandOptionChoice("datacenter", "Crystal", "Crystal")]
[DiscordCommandOptionChoice("datacenter", "Aether", "Aether")]
[DiscordCommandOptionChoice("datacenter", "Primal", "Primal")]
[DiscordCommandOptionChoice("datacenter", "Dynamis", "Dynamis")]
[DiscordCommandOptionChoice("datacenter", "Materia", "Materia")]
[DiscordCommandOptionChoice("datacenter", "Light", "Light")]
[DiscordCommandOptionChoice("datacenter", "Chaos", "Chaos")]
[DiscordCommandOption("role", "The role to assign the user when they own a venue in the specified data center.", ApplicationCommandOptionType.Role)]
public class SetRollMapCommand : ICommandHandler
{
    private readonly IRepository _repository;

    public SetRollMapCommand(IRepository repository)
    {
        this._repository = repository;
    }

    public async Task HandleAsync(SlashCommandVeniInteractionContext slashCommand)
    {
        var guildId = slashCommand.Interaction.GuildId ?? 0;
        if (guildId == 0)
            return;

        var guildSettings = await this._repository.GetByIdAsync<GuildSettings>(guildId.ToString());
        if (guildSettings == null)
            guildSettings = new GuildSettings
            {
                GuildId = guildId
            };

        var dataCenter = slashCommand.GetStringArg("datacenter");
        var role = slashCommand.GetObjectArg<SocketRole>("role");

        guildSettings.DataCenterRoleMap[dataCenter] = role.Id;
        var upsertTask = this._repository.UpsertAsync(guildSettings);

        await slashCommand.Interaction.RespondAsync($"Great! I'll give that role to all {dataCenter} venue managers. 🥰");
        await upsertTask;
    }

}