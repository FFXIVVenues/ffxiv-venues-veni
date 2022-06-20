using FFXIVVenues.Veni.Utils;
using System;

namespace FFXIVVenues.Veni.Context
{
    internal class ConversationContextProvider : IDisposable, IConversationContextProvider
    {
        private readonly RollingCache<ConversationContext> _conversationContexts = new();
        private readonly IServiceProvider _serviceProvider;
        private bool _disposed;

        public ConversationContextProvider(IServiceProvider serviceProvider) =>
            _serviceProvider = serviceProvider;

        public ConversationContext GetContext(string key) =>
            _conversationContexts.GetOrSet(key, new ConversationContext(_serviceProvider));

        public void Dispose()
        {
            if (!_disposed)
                _conversationContexts.Dispose();

            _disposed = true;
        }
    }
}
