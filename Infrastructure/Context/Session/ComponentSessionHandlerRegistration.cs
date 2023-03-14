using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Infrastructure.Context.Session
{
    internal class ComponentSessionHandlerRegistration
    {

        public Func<MessageComponentInteractionContext, Task> Delegate { get; set; }
        public ComponentPersistence Persistence { get; set; }

        public ComponentSessionHandlerRegistration(Func<MessageComponentInteractionContext, Task> @delegate, ComponentPersistence persistence)
        {
            Delegate = @delegate;
            Persistence = persistence;
        }

    }
}
