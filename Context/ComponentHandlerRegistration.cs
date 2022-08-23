using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Context
{
    internal class ComponentHandlerRegistration
    {

        public Func<MessageComponentInteractionContext, Task> Delegate { get; set; }
        public ComponentPersistence Persistence { get; set; }

        public ComponentHandlerRegistration(Func<MessageComponentInteractionContext, Task> @delegate, ComponentPersistence persistence)
        {
            Delegate = @delegate;
            Persistence = persistence;
        }

    }
}
