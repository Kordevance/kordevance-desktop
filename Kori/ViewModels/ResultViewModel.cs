using System.Threading.Tasks;

namespace Kori.ViewModels;

/// <summary>
/// Defines a contract for view models that can return a results.
/// It creates a Task which is used to bound the views model result operation
/// </summary>
public abstract class ResultViewModel<T> : ViewModelBase
{
    private readonly TaskCompletionSource<T> _tcs = new();
    
    /// <summary>
    /// Access the task result.
    /// </summary>
    /// <returns>The Task result</returns>
    public Task<T> Result => _tcs.Task;
    
    /// <summary>
    /// Attempts to complete the task.
    /// </summary>
    protected void TryComplete(T result)
    {
        _tcs.TrySetResult(result);
    }
    
    /// <summary>
    /// Attempts to cancel the current task.
    /// </summary>
    public void TryCancel()
    {
        _tcs.TrySetCanceled();
    }
}