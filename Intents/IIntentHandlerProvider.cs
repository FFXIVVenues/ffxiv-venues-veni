namespace FFXIVVenues.Veni.Intents
{
    internal interface IIntentHandlerProvider
    {
        IIntentHandler ActivateIntentHandler(string interupt);
        IIntentHandler ActivateInteruptIntentHandler(string interupt);
    }
}