// <copyright file="IMyTask.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace ThreadPool;

public interface IMyTask<out TResult>
{
    /// <summary>
    /// Gets a value indicating whether the task is completed.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets the result of task completion.
    /// </summary>
    TResult Result { get; }

    /// <summary>
    /// to create new task, which use result of current task completion.
    /// </summary>
    /// <param name="continuation">function witch use result of current task completion.</param>
    /// <typeparam name="TNewResult">type of the result of continuation completion.</typeparam>
    /// <returns>result of continuation completion.</returns>
    IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation);
}