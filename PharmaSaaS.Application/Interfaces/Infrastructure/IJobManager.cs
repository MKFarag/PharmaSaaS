using System.Linq.Expressions;

namespace PharmaSaaS.Application.Interfaces.Infrastructure;

public interface IJobManager
{
    /// <summary>Creates a new fire-and-forget job based on a given method call expression.</summary>
    /// <param name="methodCall">Method call expression that will be marshalled to a server.</param>
    /// <returns>Unique identifier of a background job.</returns>
    string Enqueue(Expression<Action> methodCall);

    /// <summary>Creates a new fire-and-forget job based on a given method call expression.</summary>
    /// <param name="methodCall">Method call expression that will be marshalled to a server.</param>
    /// <returns>Unique identifier of a background job.</returns>
    string Enqueue(Expression<Func<Task>> methodCall);

    /// <summary>Creates a new fire-and-forget job based on a given method call expression.</summary>
    /// <param name="methodCall">Method call expression that will be marshalled to a server.</param>
    /// <returns>Unique identifier of a background job.</returns>
    string Enqueue<T>(Expression<Action<T>> methodCall);

    /// <summary>Creates a new fire-and-forget job based on a given method call expression.</summary>
    /// <param name="methodCall">Method call expression that will be marshalled to a server.</param>
    /// <returns>Unique identifier of a background job.</returns>
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);

    /// <summary>Creates a new background job based on a specified method call expression and schedules it to be enqueued after a given delay.</summary>
    /// <param name="methodCall">Instance method call expression that will be marshalled to the Server.</param>
    /// <param name="delay">Delay, after which the job will be enqueued.</param>
    /// <returns>Unique identifier of the created job.</returns>
    string Schedule(Expression<Action> methodCall, TimeSpan delay);

    /// <summary>Creates a new background job based on a specified method call expression and schedules it to be enqueued after a given delay.</summary>
    /// <param name="methodCall">Instance method call expression that will be marshalled to the Server.</param>
    /// <param name="delay">Delay, after which the job will be enqueued.</param>
    /// <returns>Unique identifier of the created job.</returns>
    string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay);
}
