using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Conversation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.UserSupport;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueCreation.ConversationalIntent;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.ConversationalIntents;
using FFXIVVenues.Veni.VenueControl.VenueClosing.ConversationalIntent;
using FFXIVVenues.Veni.VenueControl.VenueDeletion.ConversationalIntent;
using FFXIVVenues.Veni.VenueControl.VenueOpening.ConversationalIntent;
using FFXIVVenues.Veni.VenueDiscovery.Intents;

namespace FFXIVVenues.Veni.Infrastructure.Intent
{
    internal class IntentHandlerProvider : IIntentHandlerProvider
    {

        private readonly TypeMap<IIntentHandler> _intentMap;
        private readonly TypeMap<IIntentHandler> _interuptMap;

        public IntentHandlerProvider(IServiceProvider serviceProvider)
        {
            _intentMap = new TypeMap<IIntentHandler>(serviceProvider)
                .Add<AboutYouIntent>(IntentNames.Conversation.AboutYou)
                .Add<AgeIntent>(IntentNames.Conversation.Age)
                .Add<ByeIntent>(IntentNames.Conversation.Bye)
                .Add<HelloIntent>(IntentNames.Conversation.Hello)
                .Add<HowAreYouIntent>(IntentNames.Conversation.HowAreYou)
                .Add<MeowIntent>(IntentNames.Conversation.Meow)
                .Add<NameIntent>(IntentNames.Conversation.Name)
                .Add<SayWhatIntent>(IntentNames.Conversation.SayWhat)
                .Add<ThanksIntent>(IntentNames.Conversation.Thanks)
                .Add<AffectionIntent>(IntentNames.Conversation.Affection)
                .Add<HitOnIntent>(IntentNames.Conversation.HitOn)
                .Add<CloseIntent>(IntentNames.Operation.Close)
                .Add<CreateIntent>(IntentNames.Operation.Create)
                .Add<DeleteIntent>(IntentNames.Operation.Delete)
                .Add<ModifyIntentHandler>(IntentNames.Operation.Edit)
                .Add<OpenIntent>(IntentNames.Operation.Open)
                .Add<Show>(IntentNames.Operation.Show)
                .Add<ShowOpen>(IntentNames.Operation.ShowOpen)
                .Add<ShowForManager>(IntentNames.Operation.ShowForManager)
                .Add<Search>(IntentNames.Operation.Search)
                .Add<NoneIntent>(IntentNames.None);

            _interuptMap = new TypeMap<IIntentHandler>(serviceProvider)
                .Add<CancelIntent>(IntentNames.Interupt.Cancel)
                .Add<EscalateIntent>(IntentNames.Interupt.Escalate)
                .Add<HelpIntent>(IntentNames.Interupt.Help);
        }

        public Task HandleIteruptIntent(string interupt, MessageVeniInteractionContext context) =>
            _interuptMap.Activate(interupt)?.Handle(context);

        public Task HandleIteruptIntent(string interupt, MessageComponentVeniInteractionContext context) =>
            _interuptMap.Activate(interupt)?.Handle(context);

        public Task HandleIteruptIntent(string interupt, SlashCommandVeniInteractionContext context) =>
            _interuptMap.Activate(interupt)?.Handle(context);

        public Task HandleIntent(string interupt, MessageVeniInteractionContext context) =>
           _intentMap.Activate(interupt)?.Handle(context) ?? new NoneIntent().Handle(context);

        public Task HandleIntent(string interupt, MessageComponentVeniInteractionContext context) =>
           _intentMap.Activate(interupt)?.Handle(context) ?? new NoneIntent().Handle(context);

        public Task HandleIntent(string interupt, SlashCommandVeniInteractionContext context) =>
            _intentMap.Activate(interupt)?.Handle(context) ?? new NoneIntent().Handle(context);

    }
}
