using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.People;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.GuildEngagment
{
    public static class SetRoleMap
    {

        public const string COMMAND_NAME = "setrolemap";
        public const string OPTION_DATA_CENTER = "datacenter";
        public const string OPTION_ROLE = "role";

        internal class CommandFactory : ICommandFactory
        {

            public SlashCommandProperties GetSlashCommand(SocketGuild guildContext = null)
            {
                var dataCenterRole = new SlashCommandOptionBuilder()
                   .WithName(OPTION_DATA_CENTER)
                   .WithType(ApplicationCommandOptionType.String)
                   .WithDescription("The data center to assign the given role to.")
                   .WithRequired(true)
                   .AddChoice("Crystal", "Crystal")
                   .AddChoice("Aether", "Aether")
                   .AddChoice("Primal", "Primal")
                   .AddChoice("Dynamis", "Dynamis")
                   .AddChoice("Light", "Light")
                   .AddChoice("Chaos", "Chaos");

                var roleArgument = new SlashCommandOptionBuilder()
                   .WithName(OPTION_ROLE)
                   .WithType(ApplicationCommandOptionType.Role)
                   .WithDescription("The role to assign the user when they own a venue in the specified data center.");

                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Set role to assign to venue managers of the specified Data Center!")
                    .WithDefaultMemberPermissions(GuildPermission.ManageRoles)
                    .WithDMPermission(false)
                    .AddOption(dataCenterRole)
                    .AddOption(roleArgument)
                    .Build();
            }

        }

        internal class CommandHandler : ICommandHandler
        {
            private readonly IRepository _repository;

            public CommandHandler(IRepository repository)
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

                var dataCenter = slashCommand.GetStringArg(OPTION_DATA_CENTER);
                var role = slashCommand.GetObjectArg<SocketRole>(OPTION_ROLE);

                guildSettings.DataCenterRoleMap[dataCenter] = role.Id;
                var upsertTask = this._repository.UpsertAsync(guildSettings);

                await slashCommand.Interaction.RespondAsync($"Great! I'll give that role to all {dataCenter} venue managers. 🥰");
                await upsertTask;
            }

        }

    }
}
