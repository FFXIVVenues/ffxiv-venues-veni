using Discord.WebSocket;
using NChronicle.Core.Interfaces;
using NChronicle.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;

namespace FFXIVVenues.Veni.Logging
{
    internal class DiscordChronicleLibrary : IChronicleLibrary, IDiscordChronicleLibrary
    {

        private Dictionary<ulong, (ChronicleLevel MinLevel, ISocketMessageChannel Channel)> _channels = new();

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

            stringBuilder.Append("[");
            stringBuilder.Append(Thread.CurrentThread.ManagedThreadId);
            stringBuilder.Append("] ");

            stringBuilder.Append(record.Level switch {
                ChronicleLevel.Critical => "🔴 ",
                ChronicleLevel.Warning => "🟡 ",
                ChronicleLevel.Info => "⚪ ",
                ChronicleLevel.Success => "🟢 ",
                ChronicleLevel.Debug => "⚫ ",
                _ => "⚫ "
            });

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

            foreach (var channel in this._channels)
                if (record.Level <= channel.Value.MinLevel)
                    _ = channel.Value.Channel.SendMessageAsync(stringBuilder.ToString());
        }


        public XmlSchema GetSchema() => throw new NotImplementedException();
        public void ReadXml(XmlReader reader) => throw new NotImplementedException();
        public void WriteXml(XmlWriter writer) => throw new NotImplementedException();

    }
}
