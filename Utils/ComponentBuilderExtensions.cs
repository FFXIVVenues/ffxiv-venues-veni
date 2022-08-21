using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States.Abstractions;

namespace FFXIVVenues.Veni.Utils
{
    internal static class ComponentBuilderExtensions
    {

        public static ComponentBuilder WithSkipButton<SkipTarget, ModifyTarget>(this ComponentBuilder builder, Context.IInteractionContext context) 
            where SkipTarget : IState 
            where ModifyTarget : IState
        {
            return builder.WithButton("▶️ Skip", context.Session.RegisterComponentHandler(c =>
            {
                if (c.Session.GetItem<bool>("modifying"))
                    return c.Session.ShiftState<SkipTarget>(c);
                return c.Session.ShiftState<ModifyTarget>(c);
            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary);
        }

        public static ComponentBuilder WithBackButton(this ComponentBuilder builder, Context.IInteractionContext context)
        {
            return builder.WithButton("◀️ Back", context.Session.RegisterComponentHandler(async c =>
            {
                var result = await c.Session.TryBackStateAsync(c);
                if (result) _ = c.Interaction.ModifyOriginalResponseAsync(props => props.Components = new ComponentBuilder().Build());
                else _ = c.Interaction.RespondAsync(embed: new EmbedBuilder().WithDescription("Cannot not go back any more!").Build());
            }, ComponentPersistence.PersistRow), ButtonStyle.Secondary);
        }

    }
}
