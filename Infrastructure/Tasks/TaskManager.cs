using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace FFXIVVenues.Veni.Infrastructure.Tasks;

public class TaskManager(IServiceProvider serviceProvider, IRepository repository)
{
    private readonly List<MementoHandle> _tasks = [];

    public T CreateTask<T, TM>() 
        where T : class, ITask<TM> 
        where TM : Memento
    {
        var mementoType = typeof(TM);
        var memento = Activator.CreateInstance(mementoType) as TM;
        return this.CreateTask<T, TM>(memento);
    }

    public T CreateTask<T, TM>(TM memento) 
        where T : class, ITask<TM> 
        where TM : Memento
    {
        var taskType = typeof(T);
        var taskRecord = new TaskRecord { TaskType = taskType.FullName, Memento = memento };
        var taskContext = new TaskContext<TM>(this, taskRecord);
        var task = ActivatorUtilities.CreateInstance(serviceProvider, taskType, [ taskContext ]) as T;
        var taskHandle = new MementoHandle(taskRecord) { Task = task };
        this._tasks.Add(taskHandle);
        return task;
    }

    public Task CommitMementoAsync(TaskRecord record) =>
        repository.UpsertAsync(record);
    
}


public class MementoHandle(TaskRecord record)
{
    public string TaskId => @record.id;
    public Memento Memento => record.Memento;
    public ITask Task { get; set; }
    public Task Thread { get; set; }
}

public class TaskRecord : IEntity
{
    public string id { get; } = IdHelper.GenerateId(8);
    public required string TaskType { get; init; }
    public required Memento Memento { get; init; }
}

public class TaskContext<TM>(TaskManager taskManager, TaskRecord record) where TM : Memento
{
    public TM Memento => record.Memento as TM;
    
    public Task CommitMementoAsync() =>
        taskManager.CommitMementoAsync(record);
};
