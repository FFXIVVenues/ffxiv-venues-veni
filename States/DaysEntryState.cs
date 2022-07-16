using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.VenueModels.V2022;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venue = FFXIVVenues.Veni.Api.Models.Venue;

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

        private Dictionary<Day, string> _days = new();
        private Dictionary<Day, string> _daysHandlers = new();

        public Task Init(MessageContext c)
        {
            var component = this.BuildDaysComponent(c);
            return c.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {MessageRepository.AskDaysOpenMessage.PickRandom()}", component: component.Build());
        }

        private ComponentBuilder BuildDaysComponent(MessageContext c)
        {
            var component = new ComponentBuilder();
            foreach (var (Label, Value) in _availableDays)
                this.AddDayButton(component, c, Label, Value);
            component.WithButton("Complete",
                c.Conversation.RegisterComponentHandler(this.OnComplete, ComponentPersistence.ClearRow), ButtonStyle.Success);
            return component;
        }

        private Task OnComplete(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");
            venue.Openings = new();
            foreach (var day in this._days.Keys)
                venue.Openings.Add(new Opening { Day = day });

            if (venue.Openings.Count > 1)
                return c.Conversation.ShiftState<AskIfConsistentTimeEntryState>(c);

            return c.Conversation.ShiftState<ConsistentOpeningEntryState>(c);
        }

        private void AddDayButton(ComponentBuilder component, MessageContext c, string dayLabel, Day dayValue) =>
            component.WithButton(dayLabel,
                this._daysHandlers.ContainsKey(dayValue)
                    ? this._daysHandlers[dayValue]
                    : this._daysHandlers[dayValue] = c.Conversation.RegisterComponentHandler(async cm =>
                    {
                        if (this._days.ContainsKey(dayValue))
                            this._days.Remove(dayValue);
                        else
                            this._days[dayValue] = dayLabel;
                        await cm.MessageComponent.ModifyOriginalResponseAsync(props =>
                        {
                            var rebuilder = this.BuildDaysComponent(c);
                            props.Components = rebuilder.Build();
                        });
                    }, ComponentPersistence.PersistRow), this._days.ContainsKey(dayValue) ? ButtonStyle.Primary : ButtonStyle.Secondary);

    }
}
