using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class SelectVenueToOpenState : IState
    {

        private static string[] _messages = new[]
        {
            "Wooo! Which one are we opening?",
            "Yaay! 🥳 Which venue are we opening?",
            "🎉 Which one?"
        };

        private static string[] _responses = new[]
        {
            "Woo! The doors are open. You're green and announcements have been sent! Let's have fun today! ♥️",
            "Yay! It's that time again. 😀 You're all green on the index, and everyone's been notified. ♥️",
            "Let's do it! We... are... live!!! We're green on the index and the pings are flying! So excited. 🙂"
        };


        private IEnumerable<Venue> _managersVenues;
        private readonly IApiService _apiService;

        public SelectVenueToOpenState(IApiService apiService)
        {
            this._apiService = apiService;
        }

        public Task Init(MessageContext c)
        {
            this._managersVenues = c.Conversation.GetItem<IEnumerable<Venue>>("venues");

            var selectMenuKey = c.Conversation.RegisterComponentHandler(this.Handle, ComponentPersistence.DeleteMessage);
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

        public async Task Handle(MessageContext c)
        {
            var selectedVenueId = c.MessageComponent.Data.Values.Single();
            var venue = _managersVenues.FirstOrDefault(v => v.Id == selectedVenueId);

            c.Conversation.ClearItem("venues");
            c.Conversation.ClearState();

            await _apiService.OpenVenueAsync(venue.Id);
            await c.RespondAsync(_responses.PickRandom());
        }
    }
}
