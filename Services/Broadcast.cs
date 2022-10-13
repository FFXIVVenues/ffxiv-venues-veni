using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace FFXIVVenues.Veni.Services
{
    public record Broadcast(string Id, DiscordSocketClient Client)
    {
        public ConcurrentDictionary<ulong, IUserMessage> SentMessages { get; private set; } = new();
        public string Message { get; private set; }
        public ComponentBuilder Component { get; private set; }
        public EmbedBuilder Embed { get; private set; }

        private ConcurrentDictionary<string, Func<BroadcastInteractionContext, Task>> _handlers = new();


        public Broadcast WithMessage(string message)
        {
            Message = message;
            return this;
        }

        public Broadcast WithComponent(Func<ComponentContext, ComponentBuilder> componentBuilder)
        {
            Component = componentBuilder(new ComponentContext(this));
            return this;
        }

        public Broadcast WithEmbed(EmbedBuilder embedBuilder)
        {
            Embed = embedBuilder;
            return this;
        }

        public async Task SendToAsync(params ulong[] users)
        {
            foreach (var indexer in users)
            {
                var user = await Client.GetUserAsync(indexer);
                var channel = await user.CreateDMChannelAsync();
                var userMessage = await channel.SendMessageAsync(Message,
                                               components: Component?.Build(),
                                               embed: Embed?.Build());
                SentMessages[indexer] = userMessage;
            }

        }

        public async Task<bool> HandleComponentInteraction(SocketMessageComponent c)
        {
            if (_handlers.ContainsKey(c.Data.CustomId))
            {
                var context = new BroadcastInteractionContext(this, c);
                await _handlers[c.Data.CustomId](context);
                return true;
            }
            return false;
        }

        public record ComponentContext(Broadcast Broadcast)
        {

            public string RegisterComponentHandler(Func<BroadcastInteractionContext, Task> handler)
            {
                var key = Guid.NewGuid().ToString();
                Broadcast._handlers[key] = handler;
                return key;
            }

        }

        public record BroadcastInteractionContext(Broadcast Broadcast, SocketMessageComponent Component)
        {

            public IEnumerable<ulong> OtherUsersIds => Broadcast.SentMessages.Keys.Where(u => u == Component.User.Id);
            public SocketUser CurrentUser => Component.User;

            public async Task ModifyForOtherUsers(Action<MessageProperties, IUserMessage> modifier)
            {
                var currentUser = Component.User.Id;
                foreach (var sentMessage in Broadcast.SentMessages)
                {
                    if (sentMessage.Key == currentUser)
                        continue;
                    await sentMessage.Value.ModifyAsync(props => modifier(props, sentMessage.Value));
                }
            }

            public async Task ModifyForCurrentUser(Action<MessageProperties, IUserMessage> modifier)
            {
                var currentUser = Component.User.Id;
                foreach (var sentMessage in Broadcast.SentMessages)
                {
                    if (sentMessage.Key != currentUser)
                        continue;
                    await sentMessage.Value.ModifyAsync(props => modifier(props, sentMessage.Value));
                }
            }



        }
    }


}

