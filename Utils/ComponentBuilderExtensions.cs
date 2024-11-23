using Discord;
using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.Utils
{
    internal static class VenueAuthoringComponentBuilderExtensions
    {

        public static ComponentBuilder WithSkipButton<TSkipTarget, TModifyTarget>(this ComponentBuilder builder, IVeniInteractionContext interactionContext, VenueAuthoringContext authoringContext) 
            where TSkipTarget : ISessionState<VenueAuthoringContext> 
            where TModifyTarget : ISessionState<VenueAuthoringContext>
        {
            return builder.WithButton("►  Next/Skip", interactionContext.RegisterComponentHandler(c =>
            {
                if (authoringContext.ControlType is VenueAuthoringType.Edit)
                    return c.MoveSessionToStateAsync<TModifyTarget, VenueAuthoringContext>(authoringContext);
                return c.MoveSessionToStateAsync<TSkipTarget, VenueAuthoringContext>(authoringContext);
            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary);
        }

        public static ComponentBuilder WithBackButton(this ComponentBuilder builder, IVeniInteractionContext context, Func<Task<bool>> @override = null)
        {   
            return builder.WithButton("◄  Back", context.RegisterComponentHandler(async c =>
            {
                var result = false;
                if (@override != null) result = await @override();
                else result = await c.TryBackStateAsync();
                if (result) _ = c.Interaction.ModifyOriginalResponseAsync(props => props.Components = new ComponentBuilder().Build());
                else await c.Interaction.Channel.SendMessageAsync(embed: new EmbedBuilder().WithDescription("Cannot not go back any more!").Build());
            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary);
        }

    }
}
