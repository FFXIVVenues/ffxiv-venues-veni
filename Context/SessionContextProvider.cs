using FFXIVVenues.Veni.Context.Abstractions;
using FFXIVVenues.Veni.Utils;
using System;

namespace FFXIVVenues.Veni.Context
{
    internal class SessionContextProvider : IDisposable, ISessionContextProvider
    {
        private readonly RollingCache<SessionContext> _conversationContexts = new();
        private readonly IServiceProvider _serviceProvider;
        private bool _disposed;

        public SessionContextProvider(IServiceProvider serviceProvider) =>
            _serviceProvider = serviceProvider;

        public SessionContext GetContext(string key) =>
            _conversationContexts.GetOrSet(key, new SessionContext(_serviceProvider));

        public void Dispose()
        {
            if (!_disposed)
                _conversationContexts.Dispose();

            _disposed = true;
        }
    }
}
