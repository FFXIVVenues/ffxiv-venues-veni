using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.Infrastructure.Tasks;

public abstract class Memento
{
    public TaskState TaskState { get; set; } = TaskState.Inactive;
}

