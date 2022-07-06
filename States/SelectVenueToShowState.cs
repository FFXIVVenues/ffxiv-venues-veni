using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class SelectVenueToShowState : IState
    {

        private static string[] _messages = new[]
        {
            "Which venue would you like to see?",
            "Which one would you like me to show you?",
            "Oki, which one? 🙂"
        };
        private readonly string _apiUrl;
        private readonly string _uiUrl;
        private readonly IIndexersService _indexersService;
        private IEnumerable<Venue> _managersVenues;

        public SelectVenueToShowState(UiConfiguration uiConfig, ApiConfiguration apiConfig, IIndexersService indexersService)
        {
            this._uiUrl = uiConfig.BaseUrl;
            this._apiUrl = apiConfig.BaseUrl;
            this._indexersService = indexersService;
        }

        public Task Init(MessageContext c)
        {
            _managersVenues = c.Conversation.GetItem<IEnumerable<Venue>>("venues");

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

        public Task Handle(MessageContext c)
        {
            _ = c.MessageComponent.DeleteOriginalResponseAsync();
            var selectedVenueId = c.MessageComponent.Data.Values.Single();
            var venue = _managersVenues.FirstOrDefault(v => v.Id == selectedVenueId);

            if (!this._indexersService.IsIndexer(c.MessageComponent.User.Id)
                && !venue.Managers.Contains(c.MessageComponent.User.Id.ToString()))
                return c.RespondAsync("Sorry, you're not a manager of this venue!", flags: MessageFlags.Ephemeral);

            c.Conversation.ClearItem("venues");
            c.Conversation.ClearState();

            return c.RespondAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build());
        }
    }
}
