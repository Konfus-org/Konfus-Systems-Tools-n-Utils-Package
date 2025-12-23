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
            return task.ContinueWith(TryContinueInBackground, TaskScheduler.FromCurrentSynchronizationContext());
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
            return task.ContinueWith(TryContinueInBackground, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Will configure the task to not require the captured context to continue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable<T> ContinueOnAnyContext<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false);
        }

        /// <summary>
        /// Will configure the task to not require the captured context to continue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable ContinueOnAnyContext(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        /// <summary>
        /// Will configure the task to require the captured context to continue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable<T> ContinueOnSameContext<T>(this Task<T> task)
        {
            return task.ConfigureAwait(true);
        }

        /// <summary>
        /// Will configure the task to require the captured context to continue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable ContinueOnSameContext(this Task task)
        {
            return task.ConfigureAwait(true);
        }

        /// <summary>
        /// Fires task off without having to await it
        /// </summary>
        /// <param name="task"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FireAndForget(this Task task)
        {
            task.ContinueWith(TryContinueInBackground);
        }

        private static void TryContinueInBackground(Task t)
        {
            if (t.IsFaulted)
            {
                if (t.Exception != null)
                {
                    AggregateException aggException = t.Exception.Flatten();
                    foreach (Exception exception in aggException.InnerExceptions)
                    {
                        Debug.LogError(
                            $"Continue in background failed: {exception.Message} - {new UnityStackTraceInfo(exception.StackTrace)}");
                    }
                }
                else
                    Debug.Log("Continue in background failed! Unknown exception occurred.");
            }
            else if (t.IsCanceled) Debug.Log("Continue in background canceled.");
        }
    }
}