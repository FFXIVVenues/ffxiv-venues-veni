using FFXIVVenues.Veni.Persistance.Abstraction;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FFXIVVenues.Veni.Models
{
    internal class GuildSettings : IEntity
    {

        public string id { get; set; }

        public ulong GuildId
        {
            get => this.id != null && ulong.TryParse(this.id, out var result) ? result : 0;
            set => this.id = value.ToString();
        }

        public Dictionary<string, ulong> DataCenterRoleMap { get; set; } = new();

        public bool FormatNames { get; set; }

        public bool WelcomeJoiners { get; set; }

    }
}
