using System;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context.Abstractions;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.Infrastructure.Context.SessionHandling
{
    internal class SessionProvider : IDisposable, ISessionProvider
    {
        private readonly RollingCache<Session> _sessions = new(10800000 /* 3 hours */, 86400000 /* 24 hours */);
        private readonly IServiceProvider _serviceProvider;
        private bool _disposed;

        public SessionProvider(IServiceProvider serviceProvider) =>
            _serviceProvider = serviceProvider;
        
        public Session GetSession(SocketMessage message) =>
            _sessions.GetOrSet(message.Author.Id.ToString(), new Session(_serviceProvider));

        public Session GetSession(SocketMessageComponent message) =>
            _sessions.GetOrSet(message.User.Id.ToString(), new Session(_serviceProvider));
        
        public Session GetSession(SocketSlashCommand message) =>
            _sessions.GetOrSet(message.User.Id.ToString(), new Session(_serviceProvider));
        
        public Session GetSession(string key) =>
            _sessions.GetOrSet(key, new Session(_serviceProvider));

        public void Dispose()
        {
            if (!_disposed)
                _sessions.Dispose();

            _disposed = true;
        }
    }
}
