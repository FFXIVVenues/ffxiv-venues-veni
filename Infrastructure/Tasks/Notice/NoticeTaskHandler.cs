using System.Threading;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Infrastructure.Tasks.Notice;

public class NoticeTask(TaskContext<NoticeMemento> context) : ITask<NoticeMemento>
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        context.Memento.TaskState = TaskState.Active;
        await context.CommitMementoAsync();

        
    }
}