namespace FFXIVVenues.Veni.Context.Abstractions
{
    internal interface ISessionContextProvider
    {
        SessionContext GetContext(string key);
    }
}