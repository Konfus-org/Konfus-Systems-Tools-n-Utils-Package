using System;
using System.Threading.Tasks;

namespace Konfus.Utility.Extensions
{
    public static class TaskExtensions
    {
        public static Task ContinueInBackground<T>(this Task<T> task, Action<T> action)
        {
            return task.ContinueWith(t =>
            {
                if (t.IsFaulted || t.IsCanceled || t.Exception != null)
                {
                    return;
                }
                action(t.Result);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
 
        // https://github.com/thedillonb/CodeHub/blob/master/CodeHub.Core/Utils/FireAndForgetTask.cs
        public static Task FireAndForget(this Task task)
        {
            return task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    AggregateException aggException = t.Exception.Flatten();
                    foreach (Exception exception in aggException.InnerExceptions)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "Fire and Forget failed: " + exception.Message + " - " + exception.StackTrace);
                    }
                }
                else if (t.IsCanceled)
                {
                    System.Diagnostics.Debug.WriteLine("Fire and forget canceled.");
                }
            });
        }
    }
}