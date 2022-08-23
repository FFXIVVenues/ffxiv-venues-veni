using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States.Abstractions;
using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Utils
{
    internal static class ComponentBuilderExtensions
    {

        public static ComponentBuilder WithSkipButton<SkipTarget, ModifyTarget>(this ComponentBuilder builder, Context.IInteractionContext context) 
            where SkipTarget : IState 
            where ModifyTarget : IState
        {
            return builder.WithButton("►  Skip", context.Session.RegisterComponentHandler(c =>
            {
                if (c.Session.GetItem<bool>("modifying"))
                    return c.Session.MoveStateAsync<ModifyTarget>(c);
                return c.Session.MoveStateAsync<SkipTarget>(c);
            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary);
        }

        public static ComponentBuilder WithBackButton(this ComponentBuilder builder, Context.IInteractionContext context, Func<Task<bool>> @override = null)
        {
            return builder.WithButton("◄  Back", context.Session.RegisterComponentHandler(async c =>
            {
                var result = false;
                if (@override != null) result = await @override();
                else result = await c.Session.TryBackStateAsync(c);
                if (result) _ = c.Interaction.ModifyOriginalResponseAsync(props => props.Components = new ComponentBuilder().Build());
                else _ = c.Interaction.RespondAsync(embed: new EmbedBuilder().WithDescription("Cannot not go back any more!").Build());
            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary);
        }

    }
}
