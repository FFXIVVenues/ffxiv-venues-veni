using System;

namespace FFXIVVenues.Veni.Infrastructure.Commands.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class DiscordCommandAttribute : Attribute
{
    public string Command { get; }
    public string Description { get; }

    public DiscordCommandAttribute(string command, string description)
    {
        this.Command = command;
        this.Description = description;
    }
}