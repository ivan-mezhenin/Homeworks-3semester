// <copyright file="MyTask.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace ThreadPool;

/// <summary>
/// task of thread pool.
/// </summary>
/// <typeparam name="TResult">type of task completion result.</typeparam>
internal class MyTask<TResult> : IMyTask<TResult>
{
    private readonly Func<TResult> func;
    private readonly Lock locker = new();
    private readonly ManualResetEvent completionEvent = new(false);
    private readonly List<Action> continuations = [];
    private readonly MyThreadPool pool;
    private volatile bool isCompleted;
    private TResult? result;
    private Exception? exception;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyTask{TResult}"/> class.
    /// </summary>
    /// <param name="func">func to complete in task.</param>
    /// <param name="pool">thread pool.</param>
    public MyTask(Func<TResult> func, MyThreadPool pool)
    {
        this.func = func ?? throw new ArgumentNullException(nameof(func));
        this.pool = pool;
    }

    /// <inheritdoc/>
    public bool IsCompleted => this.isCompleted;

    /// <inheritdoc/>
    public TResult Result
    {
        get
        {
            if (!this.isCompleted)
            {
                this.completionEvent.WaitOne();
            }

            lock (this.locker)
            {
                if (this.exception != null)
                {
                    throw new AggregateException(this.exception);
                }

                if (this.result == null)
                {
                    throw new InvalidOperationException(
                        "Task completed with unexpected null result. ");
                }

                return this.result;
            }
        }
    }

    /// <inheritdoc/>
    public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation)
    {
        ArgumentNullException.ThrowIfNull(continuation);

        lock (this.locker)
        {
            var newTask = new MyTask<TNewResult>(ContinuationFunc, this.pool);

            if (this.IsCompleted)
            {
                this.pool.EnqueueTask(newTask.Complete);
            }
            else
            {
                this.continuations.Add(newTask.Complete);
            }

            return newTask;

            TNewResult ContinuationFunc()
            {
                var sourceResult = this.Result;

                return continuation(sourceResult);
            }
        }
    }

    /// <summary>
    /// to complete task.
    /// </summary>
    public void Complete()
    {
        try
        {
            this.result = this.func();
        }
        catch (Exception ex)
        {
            this.exception = ex;
        }
        finally
        {
            List<Action> continuationsToExecute;

            lock (this.locker)
            {
                this.isCompleted = true;
                this.completionEvent.Set();

                continuationsToExecute = new List<Action>(this.continuations);
                this.continuations.Clear();
            }

            foreach (var continuation in continuationsToExecute)
            {
                    this.pool.EnqueueTask(continuation);
            }
        }
    }
}