using System;
using System.Linq;
using Discord.WebSocket;
using FFXIVVenues.Veni.AI.Clu.CluModels;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionWrappers;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;

namespace FFXIVVenues.Veni.Infrastructure.Context
{

    public abstract class VeniInteractionContext<T>: IVeniInteractionContext where T : class
    {

        public T Interaction { get; }
        public DiscordSocketClient Client { get; }
        public Session Session { get; }
        public IDisposable TypingHandle { get; set; }

        public VeniInteractionContext(T message, DiscordSocketClient client, Session conversation)
        {
            Interaction = message;
            Client = client;
            Session = conversation;
        }

        public abstract string GetArgument(string name);

    }

    public class VeniInteractionContext : VeniInteractionContext<IInteractionWrapper>
    {

        private CluPrediction _prediction;

        public VeniInteractionContext(IInteractionWrapper m,
                                  DiscordSocketClient dsc,
                                  Session sc)
            : base(m, dsc, sc) { }

        public VeniInteractionContext(IInteractionWrapper m,
                                  DiscordSocketClient dsc,
                                  Session sc,
                                  CluPrediction prediction)
            : base(m, dsc, sc)
        {
            this._prediction = prediction;
        }

        public override string GetArgument(string name) =>
            this.Interaction.GetArgument(name) ?? this._prediction?.Entities.FirstOrDefault(e => e.Category == name)?.Text;

    }

    public class MessageVeniInteractionContext : VeniInteractionContext<SocketMessage>, IWrappableInteraction
    {

        public CluPrediction Prediction { get; set; }

        public MessageVeniInteractionContext(SocketMessage m,
                                         DiscordSocketClient dsc,
                                         Session sc) 
            : base(m, dsc, sc)
        { }

        public VeniInteractionContext ToWrappedInteraction() =>
            new (new MessageWrapper(this.Interaction),
                                   this.Client,
                                   this.Session,
                                   this.Prediction);

        public override string GetArgument(string name) =>
            this.Prediction?.Entities.FirstOrDefault(e => e.Category == name)?.Text;

    }

    public class MessageComponentVeniInteractionContext : VeniInteractionContext<SocketMessageComponent>, IWrappableInteraction
    {

        public MessageComponentVeniInteractionContext(SocketMessageComponent m,
                                                  DiscordSocketClient dsc,
                                                  Session sc)
            : base(m, dsc, sc) { }

        public VeniInteractionContext ToWrappedInteraction() =>
            new (new MessageComponentWrapper(this.Interaction),
                                   this.Client,
                                   this.Session);

        public override string GetArgument(string name) =>
            this.Interaction.Data?.Value;

    }

    public class SlashCommandVeniInteractionContext : VeniInteractionContext<SocketSlashCommand>, IWrappableInteraction
    {

        public SlashCommandVeniInteractionContext(SocketSlashCommand m,
                                              DiscordSocketClient dsc,
                                              Session sc)
            : base(m, dsc, sc) { }

        public VeniInteractionContext ToWrappedInteraction() =>
            new (new SlashCommandWrapper(this.Interaction),
                                   this.Client,
                                   this.Session);

        public override string GetArgument(string name) =>
            this.Interaction.Data.Options.FirstOrDefault(c => c.Name == name)?.Value as string;

    }

}
