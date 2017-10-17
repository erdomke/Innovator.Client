using System;

namespace Innovator.Client
{
  /// <summary>
  /// Represents a promise that a result of the specified type will be provided at some point in the future
  /// </summary>
  /// <typeparam name="T">Type of data returned by the promise</typeparam>
  /// <remarks>
  /// <para>A promise is very similar to a <see cref="System.Threading.Tasks.Task"/> and can be awaited
  /// just like a task.  The API of a promise is very similar to that of a 
  /// <a href="http://api.jquery.com/category/deferred-object/">JQuery Promise</a></para>
  /// <para>To create a promise in a given state, use one of the helper methods from the 
  /// <see cref="Promises"/> class such as <see cref="Promises.Resolved{T}(T)"/> or
  /// <see cref="Promises.Rejected{T}(Exception)"/>.</para>
  /// </remarks>
  /// <seealso cref="Innovator.Client.IPromise{T}" />
  public class Promise<T> : IPromise<T>
  {
    private enum Status
    {
      Pending,
      Rejected,
      Resolved,
      Canceled
    }

    private Callback _callback;
    private Status _status = Status.Pending;
    private int _percentComplete = 0;
    private object _arg;
    private ICancelable _cancelTarget;

    /// <summary>
    /// Initializes a new instance of the <see cref="Promise{T}"/> class.
    /// </summary>
    public Promise()
    {
      Invoker = (d, a) =>
      {
        if (a == null)
        {
          ((Action)d).Invoke();
        }
        else
        {
          d.DynamicInvoke(a);
        }
      };
    }

    /// <summary>
    /// Gets or sets the invoker.
    /// </summary>
    /// <value>
    /// The invoker.
    /// </value>
    public Action<Delegate, object[]> Invoker { get; set; }

    /// <summary>
    /// Whether an error occurred causing the promise to be rejected
    /// </summary>
    public virtual bool IsRejected
    {
      get { return _status == Status.Rejected || _status == Status.Canceled; }
    }

    /// <summary>
    /// Whether the promise completed successfully
    /// </summary>
    public virtual bool IsResolved
    {
      get { return _status == Status.Resolved; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is complete.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is complete; otherwise, <c>false</c>.
    /// </value>
    public virtual bool IsComplete
    {
      get { return _status != Status.Pending; }
    }

    /// <summary>
    /// The progress of the promise represented as an integer from 0 to 100
    /// </summary>
    public int PercentComplete
    {
      get { return _percentComplete; }
    }

    /// <summary>
    /// The result of the promise.  Only valid if <see cref="P:Innovator.Client.IPromise.IsResolved" /> is <c>true</c>
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    public T Value
    {
      get
      {
        if (!this.IsComplete) throw new NotSupportedException();
        var ex = _arg as Exception;
        if (ex != null) ex.Rethrow();
        return (T)_arg;
      }
    }

    /// <summary>
    /// Callback to be executed when the promise completes regardless of whether an error occurred
    /// </summary>
    /// <param name="callback">Callback to be executed</param>
    /// <returns>The current instance for chaining additional calls</returns>
    public IPromise<T> Always(Action callback)
    {
      if (_status != Status.Pending)
      {
        callback.Invoke();
      }
      else
      {
        LinkedListOps.Add(ref _callback, new Callback(callback, Condition.Always));
      }
      return this;
    }

    /// <summary>
    /// Cancels the target.
    /// </summary>
    /// <param name="cancelTarget">The cancel target.</param>
    /// <returns><paramref name="cancelTarget"/> for chaining additional calls</returns>
    public C CancelTarget<C>(C cancelTarget) where C : ICancelable
    {
      _cancelTarget = cancelTarget;
      return cancelTarget;
    }

    /// <summary>
    /// Callback to be executed when the promise completes successfully
    /// </summary>
    /// <param name="callback">Callback to be executed with the result of the promise</param>
    /// <returns>The current instance for chaining additional calls</returns>
    public IPromise<T> Done(Action<T> callback)
    {
      if (_status == Status.Resolved)
      {
        callback.Invoke((T)_arg);
      }
      else
      {
        LinkedListOps.Add(ref _callback, new Callback(callback, Condition.Success));
      }
      return this;
    }

    /// <summary>
    /// Callback to be executed when the promise encounters an error
    /// </summary>
    /// <param name="callback">Callback to be executed with the exception of the promise</param>
    /// <returns>The current instance for chaining additional calls</returns>
    public IPromise<T> Fail(Action<Exception> callback)
    {
      if (_status == Status.Rejected)
      {
        callback.Invoke((Exception)_arg);
      }
      else
      {
        LinkedListOps.Add(ref _callback, new Callback(callback, Condition.Failure));
      }
      return this;
    }

    /// <summary>
    /// Callback to be executed when the reported progress changes
    /// </summary>
    /// <param name="callback">Callback to be executed with the progress [0, 100] and the message</param>
    /// <returns>
    /// The current instance for chaining additional calls
    /// </returns>
    public IPromise<T> Progress(Action<int, string> callback)
    {
      if (_status == Status.Pending)
      {
        LinkedListOps.Add(ref _callback, new Callback(callback, Condition.Progress));
      }
      return this;
    }

    /// <summary>
    /// Notify promise listeners of a change in the progres
    /// </summary>
    public void Notify(int progress, string message)
    {
      _percentComplete = progress;
      ExecuteCallbacks(Condition.Progress, progress, message);
    }

    /// <summary>
    /// Mark the promise as having completed successfully providing the requested data
    /// </summary>
    /// <param name="data">Data which the promise wraps</param>
    public virtual void Resolve(T data)
    {
      if (_status == Status.Pending)
      {
        _status = Status.Resolved;
        _percentComplete = 100;
        ExecuteCallbacks(Condition.Progress, 100, "");
        _arg = data;
        ExecuteCallbacks(Condition.Success, _arg);
        _callback = null;
        _cancelTarget = null;
      }
    }

    /// <summary>
    /// Mark the promise as having encountered an error
    /// </summary>
    /// <param name="error">Error which was encountered</param>
    public virtual void Reject(Exception error)
    {
      if (_status == Status.Pending)
      {
        _status = Status.Rejected;
        _arg = error;
        ExecuteCallbacks(Condition.Failure, _arg);
        _callback = null;
        _cancelTarget = null;
      }
    }

    /// <summary>
    /// Execute the stored callbacks for the given condition
    /// </summary>
    /// <param name="condition">Which call backs to execute</param>
    /// <param name="arg">First argument for the callback</param>
    /// <param name="arg2">Second argument for the callback (if applicable)</param>
    protected void ExecuteCallbacks(Condition condition, object arg, object arg2 = null)
    {
      foreach (var current in LinkedListOps.Enumerate(_callback))
      {
        if ((current.Condition & condition) == condition)
        {
          switch (current.Condition)
          {
            case Condition.Always:
              this.Invoker(current.Delegate, null);
              break;
            case Condition.Failure:
            case Condition.Success:
              this.Invoker(current.Delegate, new object[] { arg });
              break;
            case Condition.Progress:
              this.Invoker(current.Delegate, new object[] { arg, arg2 });
              break;
          }
        }
      }
    }

    /// <summary>
    /// Condition under which callbacks should be executed
    /// </summary>
    protected enum Condition
    {
      /// <summary>
      /// The callback should be called on success
      /// </summary>
      Success = 1,
      /// <summary>
      /// The callback should be called on failure
      /// </summary>
      Failure = 2,
      /// <summary>
      /// The callback should be always called
      /// </summary>
      Always = 3,
      /// <summary>
      /// The callback should be called when progress is reported
      /// </summary>
      Progress = 4
    }

    private class Callback : ILink<Callback>
    {
      public Callback(Delegate del, Condition condition)
      {
        this.Delegate = del;
        this.Condition = condition;
      }

      public Condition Condition { get; private set; }
      public Delegate Delegate { get; private set; }
      public string Name { get { return string.Empty; } }
      public Callback Next { get; set; }
    }

    IPromise IPromise.Done(Action<object> callback)
    {
      if (_status == Status.Resolved)
      {
        callback.Invoke(_arg);
      }
      else
      {
        LinkedListOps.Add(ref _callback, new Callback(callback, Condition.Success));
      }
      return this;
    }

    object IPromise.Value
    {
      get { return this.Value; }
    }


    IPromise IPromise.Always(Action callback)
    {
      return this.Always(callback);
    }

    IPromise IPromise.Fail(Action<Exception> callback)
    {
      return this.Fail(callback);
    }

    IPromise IPromise.Progress(Action<int, string> callback)
    {
      return this.Progress(callback);
    }

    /// <summary>
    /// Cancel the operation
    /// </summary>
    public virtual void Cancel()
    {
      if (_status == Status.Pending)
      {
        _status = Status.Canceled;
        if (_cancelTarget != null) _cancelTarget.Cancel();
        Reject(new OperationCanceledException());
      }
    }
  }
}
