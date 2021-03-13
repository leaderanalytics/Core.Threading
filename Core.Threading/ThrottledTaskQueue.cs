using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeaderAnalytics.Core.Threading
{
    public class ThrottledTaskQueue
    {
        private BlockingCollection<Task> queue;
        private SemaphoreSlim throttle;
        private SemaphoreSlim blocker;
        private int _maxThreads;
        private ConcurrentBag<Exception> exceptions;

        public ThrottledTaskQueue(int maxThreads)
        {
            _maxThreads = maxThreads;
            queue = new BlockingCollection<Task>();
            throttle = new SemaphoreSlim(maxThreads);
            blocker = new SemaphoreSlim(1);     // Blocks anyone who is waiting in the WaitAll method.
            exceptions = new ConcurrentBag<Exception>();

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Task task = queue.Take();

                    if (blocker.CurrentCount == 1)
                        blocker.Wait(); // At least one task is running.  We dont care exactly how many.

                    throttle.Wait();
                    task.ContinueWith(t =>
                    {
                        throttle.Release();


                        if (t.Exception != null)
                            exceptions.Add(t.Exception);

                        if (throttle.CurrentCount == _maxThreads)
                            blocker.Release();  // No tasks are running.  Release the blocker so anyone who is waiting in the WaitAll method will get notified.

                    });

                    task.Start();
                }
            });
        }

        public void Enqueue(Task task)
        {
            queue.Add(task);
            while (blocker.CurrentCount == 1) ;  // Give the task time to spin up
        }

        public void WaitAll()
        {
            blocker.Wait();

            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }
    }
}
