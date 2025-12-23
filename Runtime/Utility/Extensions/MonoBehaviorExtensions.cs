using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Konfus.Utility.Extensions
{
    public static class MonoBehaviourExtensions
    {
        /// <summary>
        /// Executes action on Unity's main thread
        /// </summary>
        /// <param name="action">IEnumerator function that will be executed from the main thread.</param>
        public static void DispatchToMainThread(this MonoBehaviour behaviour, IEnumerator action)
        {
            behaviour.StartCoroutine(action);
        }

        /// <summary>
        /// Executes action on Unity's main thread
        /// </summary>
        /// <param name="action">function that will be executed from the main thread.</param>
        public static T? DispatchToMainThread<T>(this MonoBehaviour behaviour, Func<T> action)
        {
            return ExecuteActionOnMainThread(behaviour, action);
        }

        /// <summary>
        /// Executes action on Unity's main thread
        /// </summary>
        /// <param name="action">function that will be executed from the main thread.</param>
        public static void DispatchToMainThread(this MonoBehaviour behaviour, Action action)
        {
            ExecuteActionOnMainThread(behaviour, action);
        }

        /// <summary>
        /// Executes action on Unity's main thread, returning a Task which is completed when the action completes
        /// </summary>
        /// <param name="action">function that will be executed from the main thread.</param>
        /// <returns>A Task that can be awaited until the action completes</returns>
        public static Task DispatchToMainThreadAsync(this MonoBehaviour behaviour, Action action)
        {
            var tcs = new TaskCompletionSource<bool>();

            void WrappedAction()
            {
                try
                {
                    action();
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }

            ExecuteActionOnMainThread(behaviour, WrappedAction);
            return tcs.Task;
        }

        // <summary>
        /// Executes action on Unity's main thread, returning a Task which is completed when the action completes
        /// </summary>
        /// <param name="action">function that will be executed from the main thread.</param>
        /// <returns>A Task that can be awaited until the action completes</returns>
        public static Task<T> DispatchToMainThreadAsync<T>(this MonoBehaviour behaviour, Func<T> action)
        {
            var tcs = new TaskCompletionSource<T>();

            void WrappedAction()
            {
                try
                {
                    T result = action();
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }

            ExecuteActionOnMainThread(behaviour, WrappedAction);
            return tcs.Task;
        }

        private static void ExecuteActionOnMainThread(MonoBehaviour behavior, Action action)
        {
            IEnumerator Coroutine()
            {
                action();
                yield return null;
            }

            behavior.StartCoroutine(Coroutine());
        }

        private static T? ExecuteActionOnMainThread<T>(MonoBehaviour behavior, Func<T> action)
        {
            T? result = default;

            IEnumerator Coroutine()
            {
                result = action();
                yield return null;
            }

            behavior.StartCoroutine(Coroutine());
            return result;
        }
    }
}