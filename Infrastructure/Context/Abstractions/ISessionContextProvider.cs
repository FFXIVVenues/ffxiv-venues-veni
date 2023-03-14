using FFXIVVenues.Veni.Infrastructure.Context.Session;

namespace FFXIVVenues.Veni.Infrastructure.Context.Abstractions
{
    internal interface ISessionContextProvider
    {
        SessionContext GetContext(string key);
    }
}