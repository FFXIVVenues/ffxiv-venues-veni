using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Commands.Brokerage;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using FFXIVVenues.Veni.Models;
using FFXIVVenues.Veni.Persistance.Abstraction;
using NChronicle.Core.Model;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Commands
{
    public static class SetRole
    {

        public const string COMMAND_NAME = "setrole";
        public const string OPTION_DATA_CENTER = "datacenter";
        public const string OPTION_ROLE = "role";

        internal class CommandFactory : ICommandFactory
        {

            public SlashCommandProperties GetSlashCommand(SocketGuild guildContext = null)
            {
                new SlashCommandOptionBuilder()
                   .WithName(OPTION_ROLE)
                   .WithType(ApplicationCommandOptionType.Role)
                   .WithDescription("The role to assign the user when they own a venue in the specified data center.");

                var roleArgument = new ArgumentBuilder();

                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Set role to assign to venue managers of the specified Data Center!")
                    .WithDefaultMemberPermissions(GuildPermission.ManageRoles)
                    .WithDMPermission(false)
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

            public async Task HandleAsync(SlashCommandInteractionContext slashCommand)
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

                guildSettings.DataCenterRoleMap
                return Task.CompletedTask;
            }

        }

    }
}
