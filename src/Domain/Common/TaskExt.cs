namespace Beseler.Domain.Common;

public static class TaskExt
{
    public static async Task WhenAll(IEnumerable<Task> tasks)
    {
        var taskOfTasks = Task.WhenAll(tasks);
        try
        {
            await taskOfTasks;
        }
        catch(Exception)
        {
            if (taskOfTasks.Exception is not null)
                throw taskOfTasks.Exception;

            throw;
        }
    }
}
