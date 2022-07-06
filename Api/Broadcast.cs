using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace FFXIVVenues.Veni.Api
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
            this.Message = message;
            return this;
        }

        public Broadcast WithComponent(Func<ComponentContext, ComponentBuilder> componentBuilder)
        {
            this.Component = componentBuilder(new ComponentContext(this));
            return this;
        }

        public Broadcast WithEmbed(EmbedBuilder embedBuilder)
        {
            this.Embed = embedBuilder;
            return this;
        }

        public async Task SendToAsync(params ulong[] users)
        {
            foreach (var indexer in users)
            {
                var user = await this.Client.GetUserAsync(indexer);
                var channel = await user.CreateDMChannelAsync();
                var userMessage = await channel.SendMessageAsync(this.Message,
                                               components: this.Component?.Build(),
                                               embed: this.Embed?.Build());
                this.SentMessages[indexer] = userMessage;
            }

        }

        public async Task<bool> HandleComponentInteraction(SocketMessageComponent c)
        {
            if (this._handlers.ContainsKey(c.Data.CustomId))
            {
                var context = new BroadcastInteractionContext(this, c);
                await this._handlers[c.Data.CustomId](context);
                return true;
            }
            return false;
        }

        public record ComponentContext(Broadcast Broadcast)
        {

            public string RegisterComponentHandler(Func<BroadcastInteractionContext, Task> handler)
            {
                var key = Guid.NewGuid().ToString();
                this.Broadcast._handlers[key] = handler;
                return key;
            }

        }

        public record BroadcastInteractionContext(Broadcast Broadcast, SocketMessageComponent Component)
        {

            public IEnumerable<ulong> OtherUsersIds => this.Broadcast.SentMessages.Keys.Where(u => u == this.Component.User.Id);
            public SocketUser CurrentUser => this.Component.User;

            public async Task ModifyForOtherUsers(Action<MessageProperties, IUserMessage> modifier)
            {
                var currentUser = this.Component.User.Id;
                foreach (var sentMessage in this.Broadcast.SentMessages)
                {
                    if (sentMessage.Key == currentUser)
                        continue;
                    await sentMessage.Value.ModifyAsync(props => modifier(props, sentMessage.Value));
                }
            }

            public async Task ModifyForCurrentUser(Action<MessageProperties, IUserMessage> modifier)
            {
                var currentUser = this.Component.User.Id;
                foreach (var sentMessage in this.Broadcast.SentMessages)
                {
                    if (sentMessage.Key != currentUser)
                        continue;
                    await sentMessage.Value.ModifyAsync(props => modifier(props, sentMessage.Value));
                }
            }



        }
    }


}

