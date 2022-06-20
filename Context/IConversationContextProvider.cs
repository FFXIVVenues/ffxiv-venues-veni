namespace FFXIVVenues.Veni.Context
{
    internal interface IConversationContextProvider
    {
        ConversationContext GetContext(string key);
    }
}