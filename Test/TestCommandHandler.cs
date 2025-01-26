using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni._Test;

[DiscordCommand("test", "Test command, using new async interactions")]
public class TestCommand : ICommandHandler
{
    public async Task HandleAsync(SlashCommandVeniInteractionContext slashCommand)
    {

        await slashCommand.Interaction.RespondAsync("Welcome to FFXIV Venues!",
            ephemeral: true, components: new ComponentBuilder().WithButton("Start!", "rAnDoMsElEcTIdEnTiFiEr").Build());
        var buttonPress = await slashCommand.Session.AwaitComponentResponseAsync();

        await buttonPress.RespondWithModalAsync(
            new ModalBuilder("New Venue", "venue-name-desc-modal", new ModalComponentBuilder()
                .WithTextInput("Venue name", "venueName", TextInputStyle.Short)
                .WithTextInput("Venue description", "venueDescription", TextInputStyle.Paragraph))
                .Build());


        var modalResponse = await slashCommand.Session.AwaitModalResponseAsync();
        
        var venueName = modalResponse.Data.Components.First(c => c.CustomId == "venueName").Value;
        var venueDescription = modalResponse.Data.Components.First(c => c.CustomId == "venueDescription").Value;

        
        await modalResponse.RespondAsync("Pick an option!", ephemeral: true, 
            components: new ComponentBuilder()
                .WithSelectMenu(new SelectMenuBuilder { CustomId = "rAnDoMsElEcTIdEnTiFiEr" }
                    .AddOption("NSFW", "nsfw")
                    .AddOption("SFW", "sfw")).Build());
        await slashCommand.Interaction.DeleteOriginalResponseAsync();

        var response = await slashCommand.Session.AwaitComponentResponseAsync();
        var isSfw = response.Data.Values.First() == "sfw";

        await modalResponse.ModifyOriginalResponseAsync(properties =>
        {
            properties.Content = $"Created!\nVenue Name: {venueName}\nVenue Description: {venueDescription}\n SFW: {isSfw}!";
            properties.Components = new ComponentBuilder().Build();
        });
    }
}