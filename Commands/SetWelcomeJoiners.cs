using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Commands.Brokerage;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Models;
using FFXIVVenues.Veni.Persistance.Abstraction;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Commands
{
    public static class SetWelcomeJoiners
    {

        public const string COMMAND_NAME = "setwelcomejoiners";
        public const string OPTION_ALLOW = "allow";

        internal class CommandFactory : ICommandFactory
        {

            public SlashCommandProperties GetSlashCommand(SocketGuild guildContext = null)
            {
                var allowArugment = new SlashCommandOptionBuilder()
                   .WithName(OPTION_ALLOW)
                   .WithType(ApplicationCommandOptionType.Boolean)
                   .WithRequired(true)
                   .WithDescription("Whether to allow Veni to welcome new joiners.");

                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Tell Veni whether to welcome new joiners to the discord server or not!")
                    .WithDefaultMemberPermissions(GuildPermission.ManageRoles)
                    .WithDMPermission(false)
                    .AddOption(allowArugment)
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

                var allow = slashCommand.GetBoolArg(OPTION_ALLOW);
                if (allow == null)
                    return;

                guildSettings.WelcomeJoiners = allow.Value;
                var upsertTask = this._repository.UpsertAsync(guildSettings);

                if (allow.Value)
                    await slashCommand.Interaction.RespondAsync($"Yaay! I'll give warm welcomes I promise! 😻");
                else
                    await slashCommand.Interaction.RespondAsync($"Oooookkk! I'll stop welcoming. 😿");

                await upsertTask;
            }

        }

    }
}
