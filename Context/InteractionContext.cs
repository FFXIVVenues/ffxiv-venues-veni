using Discord.WebSocket;
using FFXIVVenues.Veni.Context.InteractionWrappers;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace FFXIVVenues.Veni.Context
{

    public abstract class InteractionContext<T>: IInteractionContext where T : class
    {

        public T Interaction { get; }
        public DiscordSocketClient Client { get; }
        public SessionContext Session { get; }
        

        public InteractionContext(T message, DiscordSocketClient client, SessionContext conversation)
        {
            Interaction = message;
            Client = client;
            Session = conversation;
        }

        public abstract string GetArgument(string name);

    }

    public class InteractionContext : InteractionContext<IInteractionWrapper>
    {

        private Prediction _prediction { get; set; }

        public InteractionContext(IInteractionWrapper m,
                                  DiscordSocketClient dsc,
                                  SessionContext sc)
            : base(m, dsc, sc) { }

        public InteractionContext(IInteractionWrapper m,
                                  DiscordSocketClient dsc,
                                  SessionContext sc,
                                  Prediction prediction)
            : base(m, dsc, sc)
        {
            this._prediction = prediction;
        }

        public override string GetArgument(string name) =>
            this.Interaction.GetArgument(name) ?? (this._prediction?.Entities[name] as JArray)?.First.Value<string>();

    }

    public class MessageInteractionContext : InteractionContext<SocketMessage>, IWrappableInteraction {

        public Prediction Prediction { get; set; }


        public MessageInteractionContext(SocketMessage m,
                                         DiscordSocketClient dsc,
                                         SessionContext sc) 
            : base(m, dsc, sc) { }

        public InteractionContext ToWrappedInteraction() =>
            new InteractionContext(new MessageWrapper(this.Interaction),
                                   this.Client,
                                   this.Session,
                                   this.Prediction);

        public override string GetArgument(string name) =>
            (this.Prediction?.Entities[name] as JArray)?.First.Value<string>();


    }

    public class MessageComponentInteractionContext : InteractionContext<SocketMessageComponent>, IWrappableInteraction
    {

        public MessageComponentInteractionContext(SocketMessageComponent m,
                                                  DiscordSocketClient dsc,
                                                  SessionContext sc)
            : base(m, dsc, sc) { }

        public InteractionContext ToWrappedInteraction() =>
            new InteractionContext(new MessageComponentWrapper(this.Interaction),
                                   this.Client,
                                   this.Session);

        public override string GetArgument(string name) =>
            this.Interaction.Data?.Value;

    }

    public class SlashCommandInteractionContext : InteractionContext<SocketSlashCommand>, IWrappableInteraction
    {

        public SlashCommandInteractionContext(SocketSlashCommand m,
                                              DiscordSocketClient dsc,
                                              SessionContext sc)
            : base(m, dsc, sc) { }

        public InteractionContext ToWrappedInteraction() =>
            new InteractionContext(new SlashCommandWrapper(this.Interaction),
                                   this.Client,
                                   this.Session);

        public override string GetArgument(string name) =>
            this.Interaction.Data.Options.FirstOrDefault(c => c.Name == name)?.Value as string;

    }

}
