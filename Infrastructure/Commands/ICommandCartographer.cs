using System;
using System.Collections.Generic;
using Discord;

namespace FFXIVVenues.Veni.Infrastructure.Commands;

public interface ICommandCartographer
{
    (SlashCommandBuilder[], Dictionary<string, Type>) Discover();
}