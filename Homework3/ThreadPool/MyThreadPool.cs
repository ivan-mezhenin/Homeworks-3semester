// <copyright file="MyThreadPool.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace ThreadPool;

public class MyThreadPool
{
    private readonly int threadCount;
    private readonly Thread[] threads;
    private readonly Queue<Action> taskQueue = new Queue<Action>();
    private readonly object queueLock = new object();
    private readonly AutoResetEvent taskAvailable = new AutoResetEvent(false);
    private readonly CancellationTokenSource cts = new CancellationTokenSource();

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
                    // dsds.
                }
            }
            else
            {
                this.taskAvailable.WaitOne();
            }
        }
    }
}