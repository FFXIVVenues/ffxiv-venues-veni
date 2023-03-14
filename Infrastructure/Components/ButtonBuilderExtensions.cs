using System;
using System.Configuration;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.Session;

namespace FFXIVVenues.Veni.Infrastructure.Components;

public static class ButtonBuilderExtensions
{

    public static ButtonBuilder WithSessionHandler(
        this ButtonBuilder builder,
        SessionContext session,
        Func<MessageComponentInteractionContext, Task> @delegate,
        ComponentPersistence persistence) =>
        builder.WithCustomId(session.RegisterComponentHandler(@delegate, persistence));

    public static ButtonBuilder WithStaticHandler(
        this ButtonBuilder builder,
        string handler,
        params string[] args)
    {
        var argCollection = new CommaDelimitedStringCollection();
        argCollection.AddRange(args);
        return builder.WithCustomId($"{handler}:{argCollection}");
    }

}