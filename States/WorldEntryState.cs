using Discord;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class WorldEntryState : IState
    {

        private static Dictionary<string, string> _worldMap = new()
        {
            { "Adamantoise", "Aether" },
            { "Cactuar", "Aether" },
            { "Faerie", "Aether" },
            { "Gilgamesh", "Aether" },
            { "Jenova", "Aether" },
            { "Midgardsormr", "Aether" },
            { "Sargatanas", "Aether" },
            { "Siren", "Aether" },

            { "Behemoth", "Primal" },
            { "Excalibur", "Primal" },
            { "Exodus", "Primal" },
            { "Famfrit", "Primal" },
            { "Hyperion", "Primal" },
            { "Lamia", "Primal" },
            { "Leviathan", "Primal" },
            { "Ultros", "Primal" },

            { "Balmung", "Crystal" },
            { "Brynhildr", "Crystal" },
            { "Coeurl", "Crystal" },
            { "Diabolos", "Crystal" },
            { "Goblin", "Crystal" },
            { "Malboro", "Crystal" },
            { "Mateus", "Crystal" },
            { "Zalera", "Crystal" },

            //{ "Aegis", "Elemental" },
            //{ "Atamos", "Elemental" },
            //{ "Carbuncle", "Elemental" },
            //{ "Garuda", "Elemental" },
            //{ "Gungnir", "Elemental" },
            //{ "Kujata", "Elemental" },
            //{ "Ramuh", "Elemental" },
            //{ "Tonberry", "Elemental" },

            //{ "Alexander", "Gaia" },
            //{ "Bahamut", "Gaia" },
            //{ "Durandal", "Gaia" },
            //{ "Fenrir", "Gaia" },
            //{ "Ifrit", "Gaia" },
            //{ "Ridill", "Gaia" },
            //{ "Tiamat", "Gaia" },
            //{ "Ultima", "Gaia" },
            //{ "Valefor", "Gaia" },
            //{ "Yojimbo", "Gaia" },
            //{ "Zeromus", "Gaia" },

            //{ "Anima", "Mana" },
            //{ "Asura", "Mana" },
            //{ "Belias", "Mana" },
            //{ "Chocobo", "Mana" },
            //{ "Hades", "Mana" },
            //{ "Ixion", "Mana" },
            //{ "Mandragora", "Mana" },
            //{ "Masamune", "Mana" },
            //{ "Pandaemonium", "Mana" },
            //{ "Shinryu", "Mana" },
            //{ "Titan", "Mana" },

            //{ "Cerberus", "Chaos" },
            //{ "Louisoix", "Chaos" },
            //{ "Moogle", "Chaos" },
            //{ "Omega", "Chaos" },
            //{ "Ragnarok", "Chaos" },
            //{ "Spriggan", "Chaos" },

            //{ "Lich", "Light" },
            //{ "Odin", "Light" },
            //{ "Phoenix", "Light" },
            //{ "Shiva", "Light" },
            //{ "Twintania", "Light" },
            //{ "Zodiark", "Light" }
        };

        public Task Init(MessageContext c)
        {
            var worlds = _worldMap.Select(world => new SelectMenuOptionBuilder(world.Key, world.Key)).ToList();
            var selectMenu = new SelectMenuBuilder();
            selectMenu.WithOptions(worlds);
            selectMenu.WithCustomId(c.Conversation.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));
            
            return c.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {MessageRepository.AskForWorldMessage.PickRandom()}", 
                                  new ComponentBuilder().WithSelectMenu(selectMenu).Build());
        }

        public Task Handle(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");

            var world = c.MessageComponent.Data.Values.Single();

            venue.Location.World = world;
            venue.Location.DataCenter = _worldMap[venue.Location.World];

            return c.Conversation.ShiftState<HousingDistrictEntryState>(c);
        }

    }
}
