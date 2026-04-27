using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.TagsEntry;

class FeaturesEntrySessionState : ISessionState
{
    private Venue _venue;

    private static List<(string Label, string Description, string Value)> _availableTags = new()
    {
        ("Courtesans", "The venue offers erotic role-play services in a consensual environment.", "Courtesans"),
        ("Gambling", "The venue offers various games of chance with set amounts of gils and house rules.", "Gambling"),
        ("Artists", "Artists are present in the venue to offer various custom artwork.", "Artists"),
        ("Dancers", "Dancers populate the venue to hype the party and/or host customers.", "Dancers"),
        ("Bards", "A bard provides theme-based or general music to entertain guests with a selection of songs.", "Bards"),
        ("Food", "The venue offers food in their service.", "Food"),
        ("Drink", "The venue offers drinks in their service, this may be soft drinks or include alcohol.", "Drink"),
        ("Twitch DJ", "The venue hosts a DJ who provides music in twitch streams and hype the venue.", "Twitch DJ"),
        ("Sync DJ", "The venue hosts a sync DJ who offers music via a syncshell.", "Sync DJ"),
        ("Bar", "The venue offers a bar in the environment.", "Bar"),
        ("Tarot", "The venue hosts a Tarot Reader who offers to read drawn cards and provide insights about the possible meaning they hold.", "Tarot"),
        ("Pillow talk", "The venue offers Pillows who provide a safe, confidential space to promote an SFW companionship for a limited time.", "Pillow"),
        ("Photography", "The venue hosts photographers who take venue snapshots or more complex gpose of guests to display in the venue discord and create memories.", "Photography"),
        ("Open stage", "The venue offers the stage for a patron to perform to the audience.", "Open stage"),
        ("Void", "The venue is build in the void; created outside the walls of the house.", "Void"),
        ("Stylists", "The venue hosts stylists who offer a variety of glamour services, including hair styling, makeup, and wardrobe assistance.", "Stylists"),
        ("Novel performances", "The venue offers unique schedule performances, this could be SFW (theater, synch dancers, etc) or NSFW (erotic voyeur shows, live BDSM, etc).", "Performances"),
        ("Giveaways", "The venue offers giveaways through various channels, in chat, twitch stream or discord.", "Giveaways"),
        ("Syncshell available", "This venue has a syncshell available for guests to join and see each other as they see themselves.", "Syncshell available"),
        ("VIP available", "The venue offers various perks through VIP tiers. This could include free drinks, gpose, gambling perk or more.", "VIP"),
        ("LGBTQIA+ focused", "The venue is a safe space focused on LGBTQIA+.", "LGBTQIA+"),
        ("IC RP encouraged", "The venue encourages an environment for players to role play as their characters, though interacting as themselves is allowed.", "RP Heavy"),
        ("IC RP only", "The venue offers an environment for players to role play as their characters only, and does not allow open interaction as themselves.", "IC RP Only"),
        ("24/7 Open House", "This venue allows anyone to visit at anytime, even outside of any scheduled hours.", "24/7 Open House")
    };

    public Task Enter(VeniInteractionContext c)
    {
        this._venue = c.Session.GetVenue();

        var component = this.BuildTagsComponent(c);
        return c.Interaction.RespondAsync(VenueControlStrings.AskForFeaturesMessage, component.Build());
    }

    private ComponentBuilder BuildTagsComponent(VeniInteractionContext c)
    {
        var selectComponent = new SelectMenuBuilder()
            .WithCustomId(c.Session.RegisterComponentHandler(OnComplete, ComponentPersistence.ClearRow))
            .WithMaxValues(_availableTags.Count);
        foreach (var (label, desc, value) in _availableTags)
            selectComponent.AddOption(label, value, isDefault: this._venue.Tags.Contains(value), description: desc);

        return new ComponentBuilder()
            .WithSelectMenu(selectComponent)
            .WithBackButton(c)
            .WithSkipButton<WebsiteEntrySessionState, ConfirmVenueSessionState>(c);
    }

    private Task OnComplete(ComponentVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        venue.Tags ??= new();
        venue.Tags.RemoveAll(existingTag => _availableTags.Any(availableTag => existingTag == availableTag.Value));
        venue.Tags.AddRange(c.Interaction.Data.Values);

        return c.Session.MoveStateAsync<GamesEntrySessionState>(c);
    }

}