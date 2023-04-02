using System.Linq;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionWrappers;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using NChronicle.Core.Interfaces;
using Newtonsoft.Json.Linq;

namespace FFXIVVenues.Veni.Infrastructure.Context
{

    public abstract class VeniInteractionContext<T>: IVeniInteractionContext where T : class
    {

        public T Interaction { get; }
        public DiscordSocketClient Client { get; }
        public Session Session { get; }

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

        private Prediction _prediction;

        public VeniInteractionContext(IInteractionWrapper m,
                                  DiscordSocketClient dsc,
                                  Session sc)
            : base(m, dsc, sc) { }

        public VeniInteractionContext(IInteractionWrapper m,
                                  DiscordSocketClient dsc,
                                  Session sc,
                                  Prediction prediction)
            : base(m, dsc, sc)
        {
            this._prediction = prediction;
        }

        public override string GetArgument(string name) =>
            this.Interaction.GetArgument(name) ?? (this._prediction?.Entities[name] as JArray)?.First.Value<string>();

    }

    public class MessageVeniInteractionContext : VeniInteractionContext<SocketMessage>, IWrappableInteraction
    {

        public Prediction Prediction { get; set; }

        private readonly IChronicle _chronicle;

        public MessageVeniInteractionContext(SocketMessage m,
                                         DiscordSocketClient dsc,
                                         Session sc, 
                                         IChronicle chronicle) 
            : base(m, dsc, sc)
        {
            this._chronicle = chronicle;
        }

        public VeniInteractionContext ToWrappedInteraction() =>
            new (new MessageWrapper(this.Interaction, this._chronicle),
                                   this.Client,
                                   this.Session,
                                   this.Prediction);

        public override string GetArgument(string name) =>
            (this.Prediction?.Entities[name] as JArray)?.First.Value<string>();


    }

    public class MessageComponentVeniInteractionContext : VeniInteractionContext<SocketMessageComponent>, IWrappableInteraction
    {

        private readonly IChronicle _chronicle;

        public MessageComponentVeniInteractionContext(SocketMessageComponent m,
                                                  DiscordSocketClient dsc,
                                                  Session sc,
                                                  IChronicle chronicle)
            : base(m, dsc, sc)
        {
            this._chronicle = chronicle;
        }

        public VeniInteractionContext ToWrappedInteraction() =>
            new (new MessageComponentWrapper(this.Interaction, this._chronicle),
                                   this.Client,
                                   this.Session);

        public override string GetArgument(string name) =>
            this.Interaction.Data?.Value;

    }

    public class SlashCommandVeniInteractionContext : VeniInteractionContext<SocketSlashCommand>, IWrappableInteraction
    {

        private readonly IChronicle _chronicle;

        public SlashCommandVeniInteractionContext(SocketSlashCommand m,
                                              DiscordSocketClient dsc,
                                              Session sc,
                                              IChronicle chronicle)
            : base(m, dsc, sc)
        {
            this._chronicle = chronicle;
        }

        public VeniInteractionContext ToWrappedInteraction() =>
            new (new SlashCommandWrapper(this.Interaction, this._chronicle),
                                   this.Client,
                                   this.Session);

        public override string GetArgument(string name) =>
            this.Interaction.Data.Options.FirstOrDefault(c => c.Name == name)?.Value as string;

    }

}
