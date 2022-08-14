using Discord;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States.Abstractions;
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
            { "Aether | Adamantoise", "Aether" },
            { "Aether | Cactuar", "Aether" },
            { "Aether | Faerie", "Aether" },
            { "Aether | Gilgamesh", "Aether" },
            { "Aether | Jenova", "Aether" },
            { "Aether | Midgardsormr", "Aether" },
            { "Aether | Sargatanas", "Aether" },
            { "Aether | Siren", "Aether" },

            { "Primal | Behemoth", "Primal" },
            { "Primal | Excalibur", "Primal" },
            { "Primal | Exodus", "Primal" },
            { "Primal | Famfrit", "Primal" },
            { "Primal | Hyperion", "Primal" },
            { "Primal | Lamia", "Primal" },
            { "Primal | Leviathan", "Primal" },
            { "Primal | Ultros", "Primal" },

            { "Primal | Balmung", "Crystal" },
            { "Primal | Brynhildr", "Crystal" },
            { "Primal | Coeurl", "Crystal" },
            { "Primal | Diabolos", "Crystal" },
            { "Primal | Goblin", "Crystal" },
            { "Primal | Malboro", "Crystal" },
            { "Primal | Mateus", "Crystal" },
            { "Primal | Zalera", "Crystal" },

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

        public Task Init(InteractionContext c)
        {
            var worlds = _worldMap.Select(world => new SelectMenuOptionBuilder(world.Key, world.Key)).ToList();
            var selectMenu = new SelectMenuBuilder();
            selectMenu.WithOptions(worlds);
            selectMenu.WithCustomId(c.Session.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));
            
            return c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {MessageRepository.AskForWorldMessage.PickRandom()}", 
                                  new ComponentBuilder().WithSelectMenu(selectMenu).Build());
        }

        public Task Handle(MessageComponentInteractionContext c)
        {
            var venue = c.Session.GetItem<Venue>("venue");

            var world = c.Interaction.Data.Values.Single();

            venue.Location.World = world;
            venue.Location.DataCenter = _worldMap[venue.Location.World];

            return c.Session.ShiftState<HousingDistrictEntryState>(c);
        }

    }
}
