// <copyright file="MyThreadPool.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace ThreadPool;

/// <summary>
/// class for management of thread pool.
/// </summary>
public class MyThreadPool
{
    private readonly int threadCount;
    private readonly Thread[] threads;
    private readonly Queue<Action> taskQueue = new Queue<Action>();
    private readonly object queueLock = new object();
    private readonly ManualResetEvent taskAvailable = new(false);
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private Exception? exception;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyThreadPool"/> class.
    /// </summary>
    /// <param name="threadCount">amount of threads.</param>
    public MyThreadPool(int threadCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(threadCount);
        this.threadCount = threadCount;

        this.threads = new Thread[threadCount];
        for (var i = 0; i < threadCount; i++)
        {
            this.threads[i] = new Thread(this.WorkerLoop);
            this.threads[i].Start();
        }
    }

    /// <summary>
    /// Gets pool exception.
    /// </summary>
    /// <returns>exception.</returns>
    public Exception? PoolException
    {
        get
        {
            lock (this.queueLock)
            {
                return this.exception;
            }
        }
    }

    /// <summary>
    /// to submit task.
    /// </summary>
    /// <param name="func">function.</param>
    /// <typeparam name="TResult">type of function completion result.</typeparam>
    /// <returns>task.</returns>
    public IMyTask<TResult> Submit<TResult>(Func<TResult> func)
    {
        if (this.cts.IsCancellationRequested)
        {
            throw new InvalidOperationException("Cannot submit tasks after shutdown");
        }

        var newTask = new MyTask<TResult>(func, this);
        this.EnqueueTask(newTask.Complete);
        return newTask;
    }

    /// <summary>
    /// to stop work of thread pool.
    /// </summary>
    public void Shutdown()
    {
        lock (this.queueLock)
        {
            if (!this.cts.Token.IsCancellationRequested)
            {
                this.taskQueue.Clear();
                this.cts.Cancel();
                this.taskAvailable.Set();
            }
        }

        foreach (var thread in this.threads)
        {
            thread.Join();
        }
    }

    /// <summary>
    /// to enqueue task.
    /// </summary>
    /// <param name="task">task to enqueue.</param>
    internal void EnqueueTask(Action task)
    {
        lock (this.queueLock)
        {
            if (this.cts.IsCancellationRequested)
            {
                throw new InvalidOperationException("Cannot enqueue tasks after shutdown");
            }

            this.taskQueue.Enqueue(task);
            this.taskAvailable.Set();
        }
    }

    /// <summary>
    /// The main flow cycle of the thread pool.
    /// </summary>
    private void WorkerLoop()
    {
        while (!this.cts.Token.IsCancellationRequested)
        {
            Action? task = null;
            lock (this.queueLock)
            {
                if (this.taskQueue.Count > 0)
                {
                    task = this.taskQueue.Dequeue();
                }
            }

            if (task != null)
            {
                try
                {
                    task();
                }
                catch (Exception e)
                {
                    lock (this.queueLock)
                    {
                        this.exception = e;
                    }

                    this.Shutdown();
                }
            }
            else
            {
                if (!this.cts.Token.IsCancellationRequested)
                {
                    this.taskAvailable.WaitOne();
                }
            }
        }
    }
}