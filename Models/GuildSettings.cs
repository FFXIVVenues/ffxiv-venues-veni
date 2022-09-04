using FFXIVVenues.Veni.Persistance.Abstraction;
using System.Collections.Generic;

namespace FFXIVVenues.Veni.Models
{
    internal class GuildSettings : IEntity
    {

        public string Id { get; set; }
        public ulong GuildId {
            get => ulong.TryParse(this.Id, out var result) ? result : 0;
            set => this.Id = value.ToString(); 
        }

        public Dictionary<string, ulong> DataCenterRoleMap { get; set; }

        public bool FormatNames { get; set; }

    }
}
