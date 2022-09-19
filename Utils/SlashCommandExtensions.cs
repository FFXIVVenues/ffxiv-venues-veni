using Discord.WebSocket;
using FFXIVVenues.Veni.Context;
using System;
using System.Linq;

namespace FFXIVVenues.Veni.Utils
{
    internal static class SlashCommandExtensions
    {

        public static long? GetLongArg(this SlashCommandInteractionContext command, string name)
        {
            var option = command.Interaction.Data.Options.FirstOrDefault(o => o.Name == name);
            if (option == null) return null;

            return (long)option.Value;
        }

        public static int? GetInt(this SlashCommandInteractionContext command, string name)
        {
            var option = command.Interaction.Data.Options.FirstOrDefault(o => o.Name == name);
            if (option == null) return null;

            return (int) (double) option.Value;
        }

        public static bool? GetBoolArg(this SlashCommandInteractionContext command, string name)
        {
            var option = command.Interaction.Data.Options.FirstOrDefault(o => o.Name == name);
            if (option == null) return null;

            return (bool) option.Value;
        }

        public static string GetStringArg(this SlashCommandInteractionContext command, string name)
        {
            var option = command.Interaction.Data.Options.FirstOrDefault(o => o.Name == name);
            if (option == null) return null;

            return option.Value as string;
        }

        public static T GetObjectArg<T>(this SlashCommandInteractionContext command, string name) where T : class
        {
            var option = command.Interaction.Data.Options.FirstOrDefault(o => o.Name == name);
            if (option == null) return null;

            return option.Value as T;
        }

        public static T? GetEnumArg<T>(this SlashCommandInteractionContext command, string name) where T : struct, Enum
        {
            var option = command.Interaction.Data.Options.FirstOrDefault(o => o.Name == name);
            if (option == null) return null;

            var value = (long)option?.Value;

            if (value < 0)
                return null;

            var enumValues = Enum.GetValues<T>();

            if (value >= enumValues.Length)
                return null;

            return (T?)enumValues[value];
        }

    }
}
