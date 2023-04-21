using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using Discord.WebSocket;
using NChronicle.Core.Interfaces;
using NChronicle.Core.Model;

namespace FFXIVVenues.Veni.Infrastructure.Logging
{
    internal class DiscordChronicleLibrary : IChronicleLibrary, IDiscordChronicleLibrary
    {

        public const string LOG_INIT_SEQUENCE = "#> ";
        private const int BUFFER_TIME = 3_000;
        

        private Dictionary<ulong, (ChronicleLevel MinLevel, ISocketMessageChannel Channel)> _channels = new();
        private Queue<(ChronicleLevel Level, string Message)> _messageBuffer = new();
        private Timer _timer;

        public DiscordChronicleLibrary()
        {
            this._timer = new Timer(this.Tick, null, BUFFER_TIME, BUFFER_TIME);
        }

        private void Tick(object state)
        {
            var channelBuffers = new Dictionary<ulong, StringBuilder>();
            foreach (var channel in _channels)
                channelBuffers.Add(channel.Key, new());

            while (_messageBuffer.TryDequeue(out var currentMessage))
                foreach (var channel in this._channels)
                    if (currentMessage.Level <= channel.Value.MinLevel)
                        channelBuffers[channel.Key].AppendLine(currentMessage.Message);

            foreach (var channelBuffer in channelBuffers)
                if (channelBuffer.Value.Length > 0)
                    _ = _channels[channelBuffer.Key].Channel.SendMessageAsync(channelBuffer.Value.ToString());
        }

        public bool IsSubscribed(ISocketMessageChannel channel) =>
            this._channels.ContainsKey(channel.Id);

        public void Subscribe(ISocketMessageChannel channel, ChronicleLevel level) =>
            this._channels.Add(channel.Id, (level, channel));

        public void Unsubscribe(ISocketMessageChannel channel) =>
            this._channels.Remove(channel.Id);

        public void Clear() => this._channels.Clear();

        public void Store(ChronicleRecord record)
        {
            if (this._channels.Keys.Any(u => record.Message.Contains(u.ToString())))
                return;

            var stringBuilder = new StringBuilder();

            stringBuilder.Append(LOG_INIT_SEQUENCE);
            stringBuilder.Append("[");
            stringBuilder.Append(Thread.CurrentThread.ManagedThreadId);
            stringBuilder.Append("] ");
            stringBuilder.Append("[");
            stringBuilder.Append(record.Level);
            stringBuilder.Append("] ");

            if (record.Message != null)
            {
                stringBuilder.Append(record.Message);
                if (record.Exception != null)
                    stringBuilder.AppendLine();
            }

            if (record.Exception != null)
            {
                stringBuilder.AppendLine("```");
                stringBuilder.AppendLine(record.Exception.ToString());
                stringBuilder.AppendLine("```");
            }

            _messageBuffer.Enqueue((record.Level, stringBuilder.ToString()));
        }


        public XmlSchema GetSchema() => throw new NotImplementedException();
        public void ReadXml(XmlReader reader) => throw new NotImplementedException();
        public void WriteXml(XmlWriter writer) => throw new NotImplementedException();

    }
}
