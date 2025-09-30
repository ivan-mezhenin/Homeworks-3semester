// <copyright file="MyTask.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace ThreadPool;

/// <summary>
/// task of thread pool.
/// </summary>
/// <typeparam name="TResult">type of task completion result.</typeparam>
public class MyTask<TResult> : IMyTask<TResult>
{
    private readonly Func<TResult> func;
    private readonly object locker = new object();
    private readonly ManualResetEvent completionEvent = new ManualResetEvent(false);
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
        this.isCompleted = false;
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

                return this.result!;
            }
        }
    }

    /// <inheritdoc/>
    public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation)
    {
        ArgumentNullException.ThrowIfNull(continuation);

        lock (this.locker)
        {
            var newTask = new MyTask<TNewResult>(
                () => continuation(
                    this.exception != null
                ? throw new AggregateException(this.exception) : this.result!),
                this.pool);

            if (this.IsCompleted)
            {
                this.pool.EnqueueTask(newTask.Complete());
            }
            else
            {
                this.continuations.Add(() => this.pool.EnqueueTask(newTask.Complete));
            }

            return newTask;
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

        lock (this.locker)
        {
            this.isCompleted = true;
            this.completionEvent.Set();
            foreach (var continuation in this.continuations)
            {
                this.pool.EnqueueTask(continuation);
            }
        }
    }
}