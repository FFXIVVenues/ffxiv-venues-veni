﻿using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.MareEntry;

class MarePasswordEntryState : ISessionState
{

    public Task Enter(VeniInteractionContext c)
    {
        c.Session.RegisterMessageHandler(this.OnMessageReceived);
        var isDm = c.Interaction.Channel is IDMChannel;
        return c.Interaction.RespondAsync(VenueControlStrings.AskForMarePasswordMessage,
            new ComponentBuilder()
                .WithBackButton(c)
                .Build(),
            isDm ? null : new EmbedBuilder()
                .WithDescription("**@ Veni Ki** with your Mare Password")
                .WithColor(Color.Blue)
                .Build());
    }

    public Task OnMessageReceived(MessageVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        
        venue.MarePassword = c.Interaction.Content.StripMentions();
        if (c.Session.InEditing())
            return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);
        return c.Session.MoveStateAsync<SfwEntrySessionState>(c);
    }


}