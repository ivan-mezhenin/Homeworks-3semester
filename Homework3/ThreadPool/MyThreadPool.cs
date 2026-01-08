// <copyright file="MyThreadPool.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace ThreadPool;

/// <summary>
/// class for management of thread pool.
/// </summary>
public class MyThreadPool
{
    private readonly Thread[] threads;
    private readonly Queue<Action> taskQueue = new Queue<Action>();
    private readonly Lock queueLock = new();
    private readonly ManualResetEvent taskAvailable = new(false);
    private readonly CancellationTokenSource cts = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MyThreadPool"/> class.
    /// </summary>
    /// <param name="threadCount">amount of threads.</param>
    public MyThreadPool(int threadCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(threadCount);

        this.threads = new Thread[threadCount];
        for (var i = 0; i < threadCount; i++)
        {
            this.threads[i] = new Thread(this.WorkerLoop);
            this.threads[i].Start();
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
        while (true)
        {
            Action? task = null;
            lock (this.queueLock)
            {
                if (this.taskQueue.Count > 0)
                {
                    task = this.taskQueue.Dequeue();
                }
                else if (this.cts.Token.IsCancellationRequested)
                {
                    break;
                }
            }

            if (task != null)
            {
                task();
            }
            else
            {
                this.taskAvailable.WaitOne();

                if (this.cts.Token.IsCancellationRequested)
                {
                    lock (this.queueLock)
                    {
                        if (this.taskQueue.Count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}