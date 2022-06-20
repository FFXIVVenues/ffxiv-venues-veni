using FFXIVVenues.Veni.Intents.Conversation;
using FFXIVVenues.Veni.Intents.Interupt;
using FFXIVVenues.Veni.Intents.Operation;
using FFXIVVenues.Veni.Utils;
using System;

namespace FFXIVVenues.Veni.Intents
{
    internal class IntentHandlerProvider : IIntentHandlerProvider
    {

        private readonly TypeMap<IIntentHandler> _intentMap;
        private readonly TypeMap<IIntentHandler> _interuptMap;

        public IntentHandlerProvider(IServiceProvider serviceProvider)
        {
            _intentMap = new TypeMap<IIntentHandler>(serviceProvider)
                .Add<AboutYou>(IntentNames.Conversation.AboutYou)
                .Add<Age>(IntentNames.Conversation.Age)
                .Add<Bye>(IntentNames.Conversation.Bye)
                .Add<Hello>(IntentNames.Conversation.Hello)
                .Add<HowAreYou>(IntentNames.Conversation.HowAreYou)
                .Add<Meow>(IntentNames.Conversation.Meow)
                .Add<Name>(IntentNames.Conversation.Name)
                .Add<SayWhat>(IntentNames.Conversation.SayWhat)
                .Add<Thanks>(IntentNames.Conversation.Thanks)
                .Add<Affection>(IntentNames.Conversation.Affection)
                .Add<HitOn>(IntentNames.Conversation.HitOn)
                .Add<Close>(IntentNames.Operation.Close)
                .Add<Create>(IntentNames.Operation.Create)
                .Add<Delete>(IntentNames.Operation.Delete)
                .Add<Edit>(IntentNames.Operation.Edit)
                .Add<Show>(IntentNames.Operation.Show)
                .Add<Open>(IntentNames.Operation.Open)
                .Add<None>(IntentNames.None);

            _interuptMap = new TypeMap<IIntentHandler>(serviceProvider)
                .Add<Cancel>(IntentNames.Interupt.Cancel)
                .Add<Escalate>(IntentNames.Interupt.Escalate)
                .Add<Help>(IntentNames.Interupt.Help);
        }

        public IIntentHandler ActivateInteruptIntentHandler(string interupt) =>
            _interuptMap.Activate(interupt);

        public IIntentHandler ActivateIntentHandler(string interupt) =>
            _intentMap.Activate(interupt);

    }
}
