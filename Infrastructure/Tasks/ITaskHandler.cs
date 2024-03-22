using System.Threading;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Infrastructure.Tasks;

public interface ITask
{
    Task RunAsync(CancellationToken cancellationToken);
}

public interface ITask<in T> : ITask
{
}