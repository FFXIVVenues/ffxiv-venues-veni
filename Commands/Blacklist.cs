using FFXIVVenues.Veni.Utils;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Authorisation.Blacklist;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using System.Text;
using System.Linq;
using FFXIVVenues.Veni.Authorisation;

namespace FFXIVVenues.Veni.Commands
{
    public static class Blacklist
    {
        public const string COMMAND_NAME = "blacklist";
        public const string SUB_COMMAND_ADD = "add";
        public const string SUB_COMMAND_REMOVE = "remove";
        public const string SUB_COMMAND_LIST = "list";

        internal class CommandFactory : ICommandFactory
        {
            public SlashCommandProperties GetSlashCommand(SocketGuild guildContext = null)
            {
                var discordIdOption = new SlashCommandOptionBuilder()
                                            .WithName("discordid")
                                            .WithDescription("Discord ID")
                                            .WithRequired(true)
                                            .WithType(ApplicationCommandOptionType.String);
                var reasonOption = new SlashCommandOptionBuilder()
                                            .WithName("reason")
                                            .WithDescription("Reason for blacklisting")
                                            .WithRequired(true)
                                            .WithType(ApplicationCommandOptionType.String);
                var listSubCommand = new SlashCommandOptionBuilder()
                                        .WithName(SUB_COMMAND_LIST)
                                        .WithDescription("List of blacklisted users/servers")
                                        .WithType(ApplicationCommandOptionType.SubCommand);
                var addSubCommand = new SlashCommandOptionBuilder()
                                        .WithName(SUB_COMMAND_ADD)
                                        .WithDescription("Add someone to the list")
                                        .WithType(ApplicationCommandOptionType.SubCommand)
                                        .AddOption(discordIdOption)
                                        .AddOption(reasonOption);
                var removeSubCommand = new SlashCommandOptionBuilder()
                                        .WithName(SUB_COMMAND_REMOVE)
                                        .WithDescription("Remove someone from the list")
                                        .WithType(ApplicationCommandOptionType.SubCommand)
                                        .AddOption(discordIdOption);

                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Blacklist")
                    .AddOption(addSubCommand)
                    .AddOption(removeSubCommand)
                    .AddOption(listSubCommand)
                    .Build();
            }
        }
        internal class CommandHandler : ICommandHandler
        {
            private readonly IRepository db;
            private readonly IAuthorizer authorizer;

            public CommandHandler(IRepository db, IAuthorizer authorizer)
            {
                this.db = db;
                this.authorizer = authorizer;
            }

            public Task HandleAsync(SlashCommandVeniInteractionContext slashCommand)
            {
                if (!authorizer.Authorize(slashCommand.Interaction.User.Id, Permission.Blacklist).Authorized)
                {
                    return Task.CompletedTask;
                }

                switch (slashCommand.Interaction.Data.Options.First().Name)
                {
                    case SUB_COMMAND_ADD:
                        return HandleAddAsync(slashCommand);
                    case SUB_COMMAND_REMOVE:
                        return HandleRemoveAsync(slashCommand);
                    case SUB_COMMAND_LIST:
                        return HandleListAsync(slashCommand);
                }
                return Task.CompletedTask;
            }

            private async Task HandleAddAsync(SlashCommandVeniInteractionContext slashCommand)
            {
                await slashCommand.Interaction.DeferAsync();
                var discordId = slashCommand.GetStringArg("discordid");
                var reason = slashCommand.GetStringArg("reason");

                var blackListedId = new BlacklistEntry
                {
                    id = discordId,
                    Reason = reason
                };

                await slashCommand.Interaction.FollowupAsync("User added to the blacklist 😢");
                await db.UpsertAsync(blackListedId);

            }
            private async Task HandleRemoveAsync(SlashCommandVeniInteractionContext slashCommand)
            {
                await slashCommand.Interaction.DeferAsync();
                await db.DeleteAsync<BlacklistEntry>(id: slashCommand.GetStringArg("discordid"));
                await slashCommand.Interaction.FollowupAsync("Discord ID either was removed or wasnt on the blacklist 😊");
            }
            
            private async Task HandleListAsync(SlashCommandVeniInteractionContext slashCommand)
            {
                await slashCommand.Interaction.DeferAsync();
                var bannedIdList = await db.GetAll<BlacklistEntry>();
                var description = new StringBuilder();

                if (bannedIdList.Any() == false)
                {
                    description.Append("There are no blacklisted IDs ☺️");
                }
                foreach (var banned in bannedIdList){
                    description.Append("**");
                    description.Append(banned.id);
                    description.Append("**: ");
                    description.Append(banned.Reason);
                    description.AppendLine();
                }
                var embed = new EmbedBuilder()
                    .WithTitle("Blacklist")
                    .WithDescription(description.ToString())
                    .Build();

                await slashCommand.Interaction.FollowupAsync(embed:embed);
            }
        }

    }
}
