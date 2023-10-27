using System;
using Discord;

namespace FFXIVVenues.Veni.Infrastructure.Commands.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class DiscordCommandAttribute : Attribute
{
    public string Command { get; }
    public string Description { get; }
    public GuildPermission MemberPermissions { get; }
    public bool DmPermission { get; }

    public DiscordCommandAttribute(string command, string description, 
        GuildPermission memberPermissions = 0,
        bool dmPermission = true)
    {
        this.Command = command;
        this.Description = description;
        this.MemberPermissions = memberPermissions;
        this.DmPermission = dmPermission;
    }
}