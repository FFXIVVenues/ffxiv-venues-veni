using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels.V2022;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venue = FFXIVVenues.Veni.Models.Venue;

namespace FFXIVVenues.Veni.States
{
    class DaysEntryState : IState
    {

        private static List<(string Label, Day Value)> _availableDays = new()
        {
            ("Monday", Day.Monday),
            ("Tuesday", Day.Tuesday),
            ("Wednesday", Day.Wednesday),
            ("Thursday", Day.Thursday),
            ("Friday", Day.Friday),
            ("Saturday", Day.Saturday),
            ("Sunday", Day.Sunday),
        };

        private Venue _venue;

        public Task Enter(InteractionContext c)
        {
            this._venue = c.Session.GetItem<Venue>("venue");

            var component = this.BuildDaysComponent(c).WithBackButton(c);
            if (this._venue.Openings.Count > 1)
                component.WithSkipButton<AskIfConsistentTimeEntryState, AskIfConsistentTimeEntryState>(c);
            else if (this._venue.Openings.Count == 1)
                component.WithSkipButton<ConsistentOpeningTimeEntryState, ConsistentOpeningTimeEntryState>(c);

            return c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {MessageRepository.AskDaysOpenMessage.PickRandom()}", component: component.Build());
        }

        private ComponentBuilder BuildDaysComponent(InteractionContext c)
        {
            var selectComponent = new SelectMenuBuilder()
                .WithCustomId(c.Session.RegisterComponentHandler(OnComplete, ComponentPersistence.ClearRow))
                .WithMinValues(1)
                .WithMaxValues(_availableDays.Count);
            foreach (var (label, value) in _availableDays)
                selectComponent.AddOption(label, value.ToString(), isDefault: this._venue.Openings.Any(o => o.Day == value));

            return new ComponentBuilder().WithSelectMenu(selectComponent);
        }

        private Task OnComplete(MessageComponentInteractionContext c)
        {
            this._venue.Openings = c.Interaction.Data.Values
                                    .Select(d => new Opening { Day = Enum.Parse<Day>(d) })
                                    .ToList();

            if (this._venue.Openings.Count > 1)
                return c.Session.MoveStateAsync<AskIfConsistentTimeEntryState>(c);

            return c.Session.MoveStateAsync<ConsistentOpeningTimeEntryState>(c);
        }

    }
}
