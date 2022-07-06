using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class SelectVenueToCloseState : IState
    {

        private static string[] _messages = new[]
        {
            "Which one are we closing?",
            "Sleepy time! Which venue are we closing?",
            "Which one?"
        };

        private static string[] _responses = new[]
        {
            "The doors are closed! I can't wait til next time. ♥️",
            "We're no longer green! We're all closed up.",
            "Okay! I'll lock up. 😉"
        };

        private IEnumerable<Venue> _managersVenues;
        private readonly IApiService _apiService;
        private readonly IIndexersService _indexersService;

        public SelectVenueToCloseState(IApiService apiService, IIndexersService indexersService)
        {
            this._apiService = apiService;
            this._indexersService = indexersService;
        }

        public Task Init(MessageContext c)
        {
            this._managersVenues = c.Conversation.GetItem<IEnumerable<Venue>>("venues");

            var selectMenuKey = c.Conversation.RegisterComponentHandler(this.Handle);
            var componentBuilder = new ComponentBuilder();
            var selectMenuBuilder = new SelectMenuBuilder() { CustomId = selectMenuKey };
            foreach (var venue in _managersVenues.OrderBy(v => v.Name))
            {
                var selectMenuOption = new SelectMenuOptionBuilder
                {
                    Label = venue.Name,
                    Description = venue.Location.ToString(),
                    Value = venue.Id
                };
                selectMenuBuilder.AddOption(selectMenuOption);
            }
            componentBuilder.WithSelectMenu(selectMenuBuilder);
            return c.RespondAsync(_messages.PickRandom(), componentBuilder.Build());
        }

        public Task OnMessageReceived(MessageContext c) => Task.CompletedTask;

        public async Task Handle(MessageContext c)
        {
            _ = c.MessageComponent.DeleteOriginalResponseAsync();
            var selectedVenueId = c.MessageComponent.Data.Values.Single();
            var venue = _managersVenues.FirstOrDefault(v => v.Id == selectedVenueId);

            if (!this._indexersService.IsIndexer(c.MessageComponent.User.Id)
                && !venue.Managers.Contains(c.MessageComponent.User.Id.ToString()))
            {
                _ = c.RespondAsync("Sorry, you're not a manager of this venue!", flags: MessageFlags.Ephemeral);
                return;
            }

            c.Conversation.ClearItem("venues");
            c.Conversation.ClearState();

            await _apiService.CloseVenueAsync(venue.Id);
            await c.RespondAsync(_responses.PickRandom());
        }
    }
}
