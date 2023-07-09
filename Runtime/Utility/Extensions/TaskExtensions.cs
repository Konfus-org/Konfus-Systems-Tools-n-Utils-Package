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
                if (t.IsFaulted || t.Exception != null)
                {
                    AggregateException aggException = t.Exception.Flatten();
                    foreach (Exception exception in aggException.InnerExceptions)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "Continue in background failed: " + exception.Message + " - " + exception.StackTrace);
                    }
                    return;
                }
                if (t.IsCanceled)
                {
                    System.Diagnostics.Debug.WriteLine("Continue in background canceled.");
                    return;
                }
                action(t.Result);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
 
        // https://github.com/thedillonb/CodeHub/blob/master/CodeHub.Core/Utils/FireAndForgetTask.cs
        public static void FireAndForget(this Task task)
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted || t.Exception != null)
                {
                    AggregateException aggException = t.Exception.Flatten();
                    foreach (Exception exception in aggException.InnerExceptions)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "Fire and forget failed: " + exception.Message + " - " + exception.StackTrace);
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