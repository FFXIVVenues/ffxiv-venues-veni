using System;
using System.Collections.Generic;
using System.Linq;

namespace FFXIVVenues.Veni.Utils;

public static class FfxivWorlds
{

    private static readonly Dictionary<string, bool> Regions = new()
    {
        { "North America", true },
        { "Oceania", false },
        { "Europe", true },
        { "Japan", false },
    };

    private static readonly Dictionary<string, string[]> DataCenterMap = new()
    {
        { "North America", new[] { "Aether", "Primal", "Crystal", "Dynamis" } },
        { "Oceania", new[] { "Materia" }},
        { "Europe", new[] { "Chaos", "Light" }},
        { "Japan", new[] { "Elemental", "Gaia", "Mana", "Meteor" }},
    };

    private static readonly Dictionary<string, string[]> WorldMap = new()
    {
        // North America            
        { "Aether", new [] { "Adamantoise", "Cactuar", "Faerie", "Gilgamesh", "Jenova", "Midgardsormr", "Sargatanas", "Siren" }},
        { "Primal", new [] { "Behemoth", "Excalibur", "Exodus", "Famfrit", "Hyperion", "Lamia", "Leviathan", "Siren" }},
        { "Crystal", new [] { "Balmung", "Brynhildr", "Coeurl", "Diabolos", "Goblin", "Malboro", "Mateus", "Zalera" }},
        { "Dynamis", new[] { "Halicarnassus", "Maduin", "Marilith", "Seraph" }},

        // Europe
        { "Chaos", new [] { "Cerberus", "Louisoix", "Moogle", "Omega", "Ragnarok", "Spriggan" }},
        { "Light", new [] { "Lich", "Odin", "Phoenix", "Shiva", "Twintania", "Zodiark", "Light" }},
            
        // Oceanian
        { "Materia", new[] { "Bismark", "Ravana", "Sephirot", "Sophia", "Zurvan" }},
        
        // Asia
        { "Elemental", new [] { "Aegis", "Atamos", "Carbuncle", "Garuda", "Gungnir", "Kujata",  "Tonberry" }},
        { "Gaia", new [] { "Alexander", "Bahamut", "Durandal", "Fenrir", "Ifrit", "Ridill", "Tiamat", "Ultima",  }},
        { "Mana", new [] {"Anima", "Asura", "Chocobo", "Hades", "Ixion",  "Masamune", "Pandaemonium",  "Titan" }},
        { "Meteor", new [] {"Belias","Mandragora", "Ramuh", "Shinryu" , "Unicorn", "Valefor", "Yojimbo", "Zeromus" }}
    };

    public static string[] GetWorldsFor(params string[] dataCenters) =>
        dataCenters.SelectMany(dataCenter => WorldMap.ContainsKey(dataCenter) ? WorldMap[dataCenter] : Array.Empty<string>()).ToArray();

    public static string[] GetDataCentersFor(params string[] regions) =>
        regions.SelectMany(region => DataCenterMap.ContainsKey(region) ? DataCenterMap[region] : Array.Empty<string>()).ToArray();

    public static string[] GetSupportedRegions() =>
        Regions.Where(kv => kv.Value).Select(kv => kv.Key).ToArray();
    
}