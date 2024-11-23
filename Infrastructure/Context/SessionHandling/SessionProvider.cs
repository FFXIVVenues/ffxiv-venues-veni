using System;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context.Abstractions;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;

internal class SessionProvider : IDisposable, ISessionProvider
{
    private readonly RollingCache<Session> _sessions = new(86400000 /* 18 hours */, 172800000 /* 48 hours */);
    private bool _disposed;

    public Session GetSession(SocketMessage message) =>
        _sessions.GetOrSet($"{message.Channel.Id}_{message.Author.Id}", new Session()).Value;

    public Session GetSession(SocketMessageComponent message) =>
        _sessions.GetOrSet($"{message.Channel.Id}_{message.User.Id}", new Session()).Value;
        
    public Session GetSession(SocketSlashCommand message) =>
        _sessions.GetOrSet($"{message.Channel.Id}_{message.User.Id}", new Session()).Value;
        
    public Session GetSession(string key) =>
        _sessions.GetOrSet(key, new Session()).Value;

    public void Dispose()
    {
        if (!_disposed)
            _sessions.Dispose();

        _disposed = true;
    }
}