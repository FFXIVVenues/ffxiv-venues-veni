using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class ConfirmVenueState : IState
    {
        private readonly IApiService _apiService;
        private readonly IIndexersService _indexersService;
        private readonly string _uiUrl;
        private readonly string _apiUrl;

        private static string[] _preexisingResponse = new[]
        {
            "Wooo! All updated!",
            "All done for you! 🥳",
            "Ok, that's updated for you! 😊"
        };

        private static string[] _workingOnItResponse = new[]
        {
            "Yaaay! I'm working on it! So exicted. 🥳",
            "Woo, working on it! This bit is always so exciting. 🎉",
            "Alright, working on it! 😊"
        };

        private static string[] _successfulResponse = new[]
        {
            "Wooo! I've sent it. Once it's approved, it'll show on the index!",
            "All done! Once Kana or Sumi approves it, it'll be live! 🥳",
            "Ok! We'll get that approved and get it live soon! 🎉"
        };

        public ConfirmVenueState(IApiService apiService, UiConfiguration uiConfig, ApiConfiguration apiConfig, IIndexersService indexersService)
        {
            this._apiService = apiService;
            this._indexersService = indexersService;
            this._uiUrl = uiConfig.BaseUrl;
            this._apiUrl = apiConfig.BaseUrl;
        }

        public Task Init(MessageContext c)
        {
            var bannerUrl = c.Conversation.GetItem<string>("bannerUrl");
            var venue = c.Conversation.GetItem<Venue>("venue");
            return c.RespondAsync($"Here's a summary of your venue!\nWould you like to make any edits?",
                                    null,
                                    venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", bannerUrl ?? $"{this._apiUrl}/venue/{venue.Id}/media").Build());
        }

        public async Task OnMessageReceived(MessageContext c)
        {
            if (c.Prediction.TopIntent == IntentNames.Response.No)
            {
                var preexisting = c.Conversation.GetItem<bool>("prexisting");
                if (preexisting)
                    _ = c.RespondAsync("Okay, working on it!");
                else
                    _ = c.RespondAsync(_workingOnItResponse.PickRandom());
                _ = c.Message.Channel.TriggerTypingAsync();

                var venue = c.Conversation.GetItem<Venue>("venue");
                var uploadVenueResponse = await _apiService.PutVenueAsync(venue);
                if (!uploadVenueResponse.IsSuccessStatusCode)
                {
                    await c.RespondAsync("Ooops! Something went wrong. 😢");
                    return;
                }
                var bannerUrl = c.Conversation.GetItem<string>("bannerUrl");
                if (bannerUrl != null) // changed
                {
                    var uploadBannerResponse = await _apiService.PutVenueBannerAsync(venue.Id, bannerUrl);
                    if (!uploadBannerResponse.IsSuccessStatusCode)
                    {
                        await c.RespondAsync("Sorry! Something went wrong while uploading your banner. 😢\nI've put the venue up for you though!");
                    }
                }

                var isIndexer = this._indexersService.IsIndexer(c.Message.Author.Id);

                if (isIndexer)
                {
                    var approvalResponse = await _apiService.ApproveAsync(venue.Id);
                    if (!approvalResponse.IsSuccessStatusCode)
                    {
                        await c.RespondAsync(":O Something went wrong while trying to auto-approve it for you. 😢");
                        return;
                    }
                }

                if (preexisting)
                    await c.RespondAsync(_preexisingResponse.PickRandom());
                else if (isIndexer)
                    await c.RespondAsync("All done and auto-approved for you. :heart:");
                else
                {
                    await c.RespondAsync(_successfulResponse.PickRandom());
                    await SendToIndexers(venue, bannerUrl);
                }

                c.Conversation.ClearState();
                c.Conversation.ClearData();
            }
            else if (c.Prediction.TopIntent == IntentNames.Response.Yes)
                await c.Conversation.ShiftState<ModifyVenueState>(c);
            else
                await c.RespondAsync(MessageRepository.DontUnderstandResponses.PickRandom());
        }

        private Task SendToIndexers(Venue venue, string bannerUrl) =>
            this._indexersService
                .Broadcast()
                .WithMessage($"Heyo indexers!\nVenue '**{venue.Name}**' ({venue.Id}) needs approving! :heart:")
                .WithEmbed(venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", bannerUrl))
                .WithComponent(bcc =>
                {
                    ComponentBuilder approveRejectComponent = null;

                    var approveHandler = bcc.RegisterComponentHandler(async approveBic =>
                    {
                        if (!this._indexersService.IsIndexer(approveBic.Component.User.Id))
                        {
                            await approveBic.Component.RespondAsync("Sorry, only indexers can do this! :sad:");
                            return;
                        }

                        await this._apiService.ApproveAsync(venue.Id);

                        _ = approveBic.ModifyForOtherUsers((props, original) =>
                        {
                            props.Components = new ComponentBuilder().Build();
                            props.Embeds = original.Embeds.Select(e => (Embed)e)
                                .Concat(new[] { new EmbedBuilder().WithDescription($"{approveBic.CurrentUser.Username} handled this and approved the venue. 🥳").Build() })
                                .ToArray();
                        });
                        await approveBic.ModifyForCurrentUser((props, original) =>
                        {
                            props.Components = new ComponentBuilder().Build();
                            props.Embeds = original.Embeds.Select(e => (Embed)e)
                                .Concat(new[] { new EmbedBuilder().WithDescription($"You handled this and approved the venue. 🥳").Build() })
                                .ToArray();
                        });
                        foreach (var managerId in venue.Managers)
                        {
                            var manager = await approveBic.Broadcast.Client.GetUserAsync(ulong.Parse(managerId));
                            var dmChannel = await manager.CreateDMChannelAsync();
                            _ = dmChannel.SendMessageAsync($"Hey hey! :heart:\n{venue.Name} has been approved and is live!\n${this._uiUrl}/#{venue.Id}\nLet me know if you'd like anything edited. 🥳");
                        }
                    });
                    var rejectHandler = bcc.RegisterComponentHandler(async rejectBic =>
                    {
                        if (!this._indexersService.IsIndexer(rejectBic.Component.User.Id))
                        {
                            await rejectBic.Component.RespondAsync("Sorry, only indexers can do this! :sad:");
                            return;
                        }

                        var confirmDeleteHandler = bcc.RegisterComponentHandler(async confirmDeleteBic =>
                        {
                            await this._apiService.DeleteVenueAsync(venue.Id);

                            _ = rejectBic.ModifyForOtherUsers((props, original) =>
                            {
                                props.Components = new ComponentBuilder().Build();
                                props.Embeds = original.Embeds.Select(e => (Embed)e)
                                    .Concat(new[] { new EmbedBuilder().WithDescription($"{rejectBic.CurrentUser.Username} handled this and rejected the venue.").Build() })
                                    .ToArray();
                            });

                            await rejectBic.ModifyForCurrentUser((props, original) =>
                            {
                                props.Components = new ComponentBuilder().Build();
                                props.Embeds = original.Embeds.Select(e => e as Embed)
                                    .Concat(new[] { new EmbedBuilder().WithDescription($"You handled this and rejected the venue.").Build() })
                                    .ToArray();
                            });
                        });

                        var cancelHandler = bcc.RegisterComponentHandler(cancelBic =>
                            cancelBic.ModifyForCurrentUser((props, original) =>
                                props.Components = approveRejectComponent.Build()));

                        await rejectBic.ModifyForCurrentUser((props, original) =>
                            props.Components = new ComponentBuilder()
                                .WithButton("Yes, delete venue", confirmDeleteHandler, ButtonStyle.Danger)
                                .WithButton("No, cancel", cancelHandler, ButtonStyle.Secondary)
                                .Build());
                    });

                    return approveRejectComponent = new ComponentBuilder()
                        .WithButton("Approve", approveHandler, ButtonStyle.Success)
                        .WithButton("Reject", rejectHandler, ButtonStyle.Secondary);
                })
                .SendToAsync(this._indexersService.Indexers);

    }
}
