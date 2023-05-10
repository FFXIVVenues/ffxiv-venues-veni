﻿using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.Conversation
{
    internal class NameIntent : IntentHandler
    {

        private static string[] _nameMessages = new[]
        {
            "My name is Veni!",
            "I'm Veni. :3",
            "It's Veni! Nice to meet you! ♥️",
            "It's Veni. ♥️",
        };

        public override Task Handle(VeniInteractionContext context) =>
            context.Interaction.RespondAsync(_nameMessages.PickRandom());

    }
}