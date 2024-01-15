using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Konfus.Utility.Custom_Types;
using UnityEngine;

namespace Konfus.Utility.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Runs task in the background
        /// </summary>
        /// <param name="task"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task ContinueInBackground(this Task task)
        {
            return task.ContinueWith(t =>
            {
                if (t.IsFaulted || t.Exception != null)
                {
                    AggregateException aggException = t.Exception.Flatten();
                    foreach (Exception exception in aggException.InnerExceptions)
                    {
                        Debug.LogError($"Continue in background failed: {exception.Message} - {new UnityStackTraceInfo(exception.StackTrace)}");
                    }
                    return;
                }
                if (t.IsCanceled)
                {
                    Debug.Log("Continue in background canceled.");
                    return;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        
        /// <summary>
        /// Runs task in the background
        /// </summary>
        /// <param name="task"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task ContinueInBackground<T>(this Task<T> task)
        {
            return task.ContinueWith(t =>
            {
                if (t.IsFaulted || t.Exception != null)
                {
                    AggregateException aggException = t.Exception.Flatten();
                    foreach (Exception exception in aggException.InnerExceptions)
                    {
                        Debug.LogError($"Continue in background failed: {exception.Message} - {new UnityStackTraceInfo(exception.StackTrace)}");
                    }
                    return;
                }
                if (t.IsCanceled)
                {
                    Debug.Log("Continue in background canceled.");
                    return;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Will configure the task to not require the captured context to continue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable<T> ContinueOnAnyContext<T>(this Task<T> task)
        {
            return task.ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Will configure the task to not require the captured context to continue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable ContinueOnAnyContext(this Task task)
        {
            return task.ConfigureAwait(continueOnCapturedContext: false);
        }
        /// <summary>
        /// Will configure the task to require the captured context to continue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable<T> ContinueOnSameContext<T>(this Task<T> task)
        {
            return task.ConfigureAwait(continueOnCapturedContext: true);
        }

        /// <summary>
        /// Will configure the task to require the captured context to continue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable ContinueOnSameContext(this Task task)
        {
            return task.ConfigureAwait(continueOnCapturedContext: true);
        }
        
        /// <summary>
        /// Fires task off without having to await it
        /// </summary>
        /// <param name="task"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FireAndForget(this Task task)
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted || t.Exception != null)
                {
                    AggregateException aggException = t.Exception.Flatten();
                    foreach (Exception exception in aggException.InnerExceptions)
                    {
                        Debug.LogError($"Fire and forget failed: {exception.Message} - {new UnityStackTraceInfo(exception.StackTrace)}");
                    }
                }
                else if (t.IsCanceled)
                {
                    Debug.Log("Fire and forget canceled.");
                }
            });
        }
    }
}