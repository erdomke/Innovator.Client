using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client
{
  /// <inheritdoc/>
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

    public Action<Delegate, object[]> Invoker { get; set; }

    /// <inheritdoc/>
    public virtual bool IsRejected
    {
      get { return _status == Status.Rejected || _status == Status.Canceled; }
    }

    /// <inheritdoc/>
    public virtual bool IsResolved
    {
      get { return _status == Status.Resolved; }
    }

    /// <summary>Indicates that the operation completed (either successfully or with an error)</summary>
    public virtual bool IsComplete
    {
      get { return _status != Status.Pending; }
    }
    /// <inheritdoc/>
    public int PercentComplete
    {
      get { return _percentComplete; }
    }
    /// <inheritdoc/>
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
    /// <inheritdoc/>
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
    /// Set the object to be canceled when the promise is canceled
    /// </summary>
    public C CancelTarget<C>(C cancelTarget) where C : ICancelable
    {
      _cancelTarget = cancelTarget;
      return cancelTarget;
    }
    /// <inheritdoc/>
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
    /// <inheritdoc/>
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
    /// <inheritdoc/>
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
      Success = 1,
      Failure = 2,
      Always = 3,
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

    /// <inheritdoc/>
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
