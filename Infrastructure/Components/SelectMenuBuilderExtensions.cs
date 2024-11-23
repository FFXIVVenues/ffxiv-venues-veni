using System;
using System.Configuration;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;

namespace FFXIVVenues.Veni.Infrastructure.Components;

public static class SelectMenuBuilderExtensions
{

    public static SelectMenuBuilder WithStaticHandler(
        this SelectMenuBuilder builder,
        string handler,
        params string[] args)
    {
        var argCollection = new CommaDelimitedStringCollection();
        argCollection.AddRange(args);
        return builder.WithCustomId($"{handler}:{argCollection}");
    }
    
    public static SelectMenuBuilder WithValueHandlers(this SelectMenuBuilder builder) => 
        builder.WithCustomId(ComponentBroker.ValuesToHandlersKey);
    
    public static SelectMenuOptionBuilder WithStaticHandler(
        this SelectMenuOptionBuilder builder,
        string handler,
        params string[] args)
    {
        var argCollection = new CommaDelimitedStringCollection();
        argCollection.AddRange(args);
        return builder.WithValue($"{handler}:{argCollection}");
    }
    
    public static SelectMenuOptionBuilder WithSessionHandler(
        this SelectMenuOptionBuilder builder,
        VeniInteractionContext context,
        Func<ComponentVeniInteractionContext, Task> @delegate, ComponentPersistence persistence)
    {
        var handler = context.RegisterComponentHandler(@delegate, persistence);
        return builder.WithValue($"{handler}");
    }

}